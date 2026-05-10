# ADR-0016: Testing Pyramid — Coverage Targets and Project Responsibilities

**Status:** Proposed  
**Date:** 2026-05-10  
**Deciders:** TBD

## Context

All seven test projects contain only the default xUnit stub `UnitTest1.Test1()`. No real test exercises any of the mature testing packages already in `Directory.Packages.props`:

- `xunit 2.9.3`
- `Moq 4.20.72`
- `FluentAssertions 8.3.0`
- `Microsoft.EntityFrameworkCore.InMemory 10.0.4`
- `Testcontainers.PostgreSql 4.4.0`
- `TngTech.ArchUnitNET 0.13.3`

There is no CI pipeline, no `dotnet test` invocation, and no coverage gate.

The seven test projects are:

- `tests/Cinturon360.Api.Tests/`
- `tests/Cinturon360.Application.Tests/`
- `tests/Cinturon360.Architecture.Tests/`
- `tests/Cinturon360.Data.Tests/`
- `tests/Cinturon360.Domain.Tests/`
- `tests/Cinturon360.Integration.Tests/`
- `tests/Cinturon360.Web.Tests/`

The `v5-plan.md` does not define coverage thresholds. `BACKLOG.md` lists "all test projects empty" as a known gap.

The recommended testing pyramid for this platform:

| Layer | Project | Tool | Target |
| --- | --- | --- | --- |
| Architecture | `Architecture.Tests` | ArchUnitNET | Layer boundaries, naming, ADR rules |
| Domain | `Domain.Tests` | xUnit + FluentAssertions | Entity factory methods, value objects, invariants |
| Application | `Application.Tests` | xUnit + Moq + FluentAssertions | Command/query handlers, validators, behaviors |
| Data | `Data.Tests` | xUnit + Testcontainers.PostgreSql | Repository queries, EF configs, migrations |
| Integration | `Integration.Tests` | xUnit + Testcontainers.PostgreSql | End-to-end slice tests (API → DB) |
| API | `Api.Tests` | xUnit + WebApplicationFactory | Endpoint routing, auth, contract |
| Web | `Web.Tests` | xUnit + bUnit | Blazor component rendering (deferred) |

## Decision

*Not yet decided.* The decision must specify:

1. **Coverage targets per layer** (line, branch, or mutation coverage %).
2. **Execution order** — which test layer must pass before the next runs in CI.
3. **Test data strategy** — builder pattern, fixture classes, or shared container instances.
4. **EF Core in-memory vs Testcontainers** — in-memory is forbidden for Data and Integration tests (see ADR-0001 consequences); Testcontainers is required.
5. **First tests to write** — which tests unblock the most high-risk work.

Recommended coverage thresholds:

| Layer | Minimum |
| --- | --- |
| Architecture | 100% (all rules pass or the build fails) |
| Domain | 80% line coverage |
| Application | 70% line coverage |
| Data | 60% line coverage (repository contract tests) |
| Integration | Key happy-path slices only; no % threshold |
| API | Key auth/routing tests; no % threshold |
| Web | Deferred to v5.2 |

Recommended first tests (unblock the most risk):

1. `Architecture.Tests` — layer boundary rules (catches the Web→Domain and Contracts→Domain leaks already identified).
2. `Domain.Tests` — `User.Create`, `Organisation.Create`, `Booking.Create` factory invariants.
3. `Application.Tests` — `LoginCommandHandler`, `CreateLicenseAgreementCommandHandler`.
4. `Integration.Tests` — one booking creation slice: request → API → DB → response.

## Consequences

**If these decisions are accepted:**

- Remove `Microsoft.EntityFrameworkCore.InMemory` from `Directory.Packages.props` for `Data.Tests` and `Integration.Tests` (or add a lint rule prohibiting its use in those projects).
- Add a shared `TestContainerFixture` in `Integration.Tests` for the Postgres container.
- Define a test builder pattern (e.g., `UserBuilder`, `OrganisationBuilder`) in a `tests/TestCommon/` project.
- Add `dotnet test` to CI with `--collect:"XPlat Code Coverage"` and a coverage threshold gate.
- ArchUnitNET tests should run in CI before all other tests (fastest feedback on architectural violations).

**Required follow-up:**

- Create `tests/TestCommon/` project with shared builders and container fixtures.
- Write ArchUnitNET layer-boundary tests first (highest leverage, lowest cost).
- Define coverage enforcement in the CI workflow (Coverlet + ReportGenerator).
- Populate `Domain.Tests` with entity invariant tests as domain work progresses.
