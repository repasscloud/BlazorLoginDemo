# ADR-0008: Job Scheduler — Supercronic vs IHostedService

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

`v5-plan.md §9` states: "A dedicated Cinturon360.Jobs container runs with a custom Supercronic image" with sub-second cron support, implying jobs are crontab entries invoked by `supercronic` as process-level triggers.

The actual implementation diverges:

- `deploy/docker/Dockerfile.jobs` installs `supercronic v0.2.33` into the image.
- The `ENTRYPOINT` is `dotnet Cinturon360.Jobs.dll` — the .NET worker host, not `supercronic`.
- Registered jobs (`ExchangeRateSyncJob`, `ExchangeRateCleanupJob`) are `IHostedService` background services that run their own `ExecuteAsync` loops with `await Task.Delay(...)` intervals.
- The `# QUESTION: confirm base image` comment in `Dockerfile.jobs` is still present, indicating this was never resolved.

A `Jobs` tracking table exists in the database (`src/Cinturon360.Domain/Entities/System/SystemEntities.cs`), but no job logs state, heartbeat, or lock into this table.

The ambiguity affects:

- Whether `supercronic` is removed from the Dockerfile (dead weight now) or wired as the real scheduler.
- Whether the scheduling interval is controlled in code (`Task.Delay`) or in a crontab file.
- Whether jobs can be triggered on-demand via the admin `MapJobEndpoints` API.

Options:

**Option A — Commit to `IHostedService` + remove supercronic:**

- Keep `ExchangeRateSyncJob` / `ExchangeRateCleanupJob` as `IHostedService` loops.
- Remove `supercronic` from `Dockerfile.jobs`.
- Add the `Jobs` table as an execution log (start time, duration, outcome) for observability.
- On-demand trigger continues to work via the existing `MapJobEndpoints` API.

**Option B — Commit to supercronic:**

- Convert each job to a short-lived CLI entrypoint (or a `dotnet run --job <name>` invocation).
- Write a `crontab` file (e.g., `deploy/docker/jobs.crontab`).
- Change `ENTRYPOINT` to `supercronic /etc/crontab`.
- Admin on-demand trigger becomes a separate endpoint that calls the job binary directly.

**Option C — Hybrid: supercronic for cron-scheduled jobs, IHostedService for event-driven workers:**

- Event-driven jobs (queue processors, webhook processors) remain `IHostedService`.
- Time-scheduled jobs (FX sync, cleanup) become crontab entries.

## Decision

*Not yet decided.* The recommended option is **A** (commit to `IHostedService`, remove supercronic) because:

- The current implementation already works; reverting to supercronic requires CLI decomposition of every job.
- `IHostedService` integrates naturally with .NET dependency injection and graceful shutdown.
- The admin on-demand trigger is simpler with a hosted service (a `CancellationToken` or a `Channel<T>` signal).
- Supercronic's sub-second precision is not required for FX rate sync (5 min) or daily cleanup.

## Consequences

**If Option A is chosen:**

- Remove `supercronic` installation from `Dockerfile.jobs`.
- Correct `v5-plan.md §9` to reflect `IHostedService`-based scheduling.
- Add job logging to the `Jobs` table (start, finish, outcome, next-scheduled) for the admin job queue page.
- Define a standard `BaseScheduledJob` base class with `ExecuteAsync` + interval + `Jobs` table write.

**If Option B is chosen:**

- Write a `deploy/docker/jobs.crontab` file.
- Decompose `ExchangeRateSyncJob` and `ExchangeRateCleanupJob` into CLI-invocable entry points.
- Change `Dockerfile.jobs` `ENTRYPOINT` to `supercronic /etc/crontab`.
- Ensure DI container initialises and disposes correctly for short-lived CLI invocations.

**Risks (all options):**

- Without a distributed lock, multiple replicas of the Jobs container will run the same job simultaneously. A database-level advisory lock or a `Jobs` table row lock is required in either option.

**Required follow-up:**

- Decision must be made before Phase 11 deployment scaffolding is authored.
- Add distributed-lock mechanism regardless of which scheduler is chosen.
- Populate `src/Cinturon360.Jobs/Recurring/` and `src/Cinturon360.Infrastructure/Jobs/` according to the chosen model.
