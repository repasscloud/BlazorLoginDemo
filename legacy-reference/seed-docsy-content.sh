#!/usr/bin/env bash
set -euo pipefail

# Seeds Docsy starter docs content into:
#   Cinturon360.Docs/content/en/docs
#
# Usage:
#   ./seed-docsy-content.sh
#   FORCE=1 ./seed-docsy-content.sh   # overwrite existing files
#
# Run from the repo root (or anywhere; it uses a relative path).

BASE="Cinturon360.Docs/content/en/docs"
FORCE="${FORCE:-0}"

write_file() {
  local target="$1"
  local tmp="${target}.tmp.$$"

  if [[ -f "$target" && "$FORCE" != "1" ]]; then
    echo "SKIP (exists): $target"
    return 0
  fi

  mkdir -p "$(dirname "$target")"
  cat > "$tmp"
  mv "$tmp" "$target"
  echo "WROTE: $target"
}

# Create directories
mkdir -p \
  "$BASE/getting-started" \
  "$BASE/guides" \
  "$BASE/how-to" \
  "$BASE/reference" \
  "$BASE/architecture" \
  "$BASE/architecture/adr" \
  "$BASE/runbooks" \
  "$BASE/release-notes" \
  "$BASE/contributing"

# docs landing page
write_file "$BASE/_index.md" <<'EOF'
---
title: "Documentation"
linkTitle: "Docs"
weight: 10
---

Start here:

- **New to the project?** → [Quickstart](/docs/getting-started/quickstart/)
- **Need a task?** → [How-to](/docs/how-to/)
- **Need a fact?** → [Reference](/docs/reference/)
- **Need the “why”?** → [Architecture + ADRs](/docs/architecture/)
- **Operating it?** → [Runbooks](/docs/runbooks/)
EOF

# Getting started
write_file "$BASE/getting-started/_index.md" <<'EOF'
---
title: "Getting Started"
linkTitle: "Getting Started"
weight: 10
---

Onboarding and first-run docs.
EOF

write_file "$BASE/getting-started/prerequisites.md" <<'EOF'
---
title: "Prerequisites"
weight: 10
---

## Tooling

- Git
- Your language/runtime SDK (example: .NET, Node, Go)
- Docker (recommended)

## Access

- Repo access
- Dev environment credentials (if applicable)
- CI/CD access (read-only is fine to start)

{{< alert title="Pattern" color="warning" >}}
Document access as *capabilities*, not individuals. Example: “Needs read access to X and write access to Y”.
{{< /alert >}}
EOF

write_file "$BASE/getting-started/quickstart.md" <<'EOF'
---
title: "Quickstart"
weight: 20
---

This is the “it runs on my machine” path.

## 1) Clone

```bash
git clone https://example.com/your/repo.git
cd repo
```

## 2) Configure

Create a local config file:

```bash
cp .env.example .env
# edit .env
```

## 3) Run

{{< tabs >}}
{{% tab name="Docker Compose" %}}
```bash
docker compose up --build
```
{{% /tab %}}
{{% tab name="Local (example)" %}}
```bash
# Replace with your actual commands
make run
```
{{% /tab %}}
{{< /tabs >}}

## 4) Verify

- App: `http://localhost:8080`
- Health: `http://localhost:8080/health`

{{< alert title="Next" color="success" >}}
Once it runs, read: [Local Development Guide](/docs/guides/local-dev/).
{{< /alert >}}
EOF

# Guides
write_file "$BASE/guides/_index.md" <<'EOF'
---
title: "Guides"
linkTitle: "Guides"
weight: 20
---

Guides explain conventions and “how we do things”.
EOF

write_file "$BASE/guides/local-dev.md" <<'EOF'
---
title: "Local Development"
weight: 10
---

## Repo layout (example)

```text
src/
  api/
  web/
  shared/
docs/
```

## Common workflows

### Run tests

```bash
# Replace with your project
make test
```

### Format/lint

```bash
make fmt
make lint
```

### Debugging

- Prefer structured logs (JSON or key=value)
- Correlate requests using a request/trace id
- Capture repro steps in an issue template

{{< alert title="Checklist" color="info" >}}
Before pushing:
- Tests pass
- Lint passes
- Docs updated (if behaviour changed)
{{< /alert >}}
EOF

write_file "$BASE/guides/logging.md" <<'EOF'
---
title: "Logging & Tracing"
weight: 20
---

## Goals

- Debug quickly with minimal guesswork
- Correlate events across services
- Keep PII out of logs by default

## Recommended fields

| Field | Meaning |
|------|---------|
| `rid` | Request/correlation id |
| `tid` | Transaction/trace id |
| `uid` | User id (if available) |
| `org` | Tenant/org id |
| `dur_ms` | Duration |
| `evt` | Event name |

## Example log

```json
{
  "lvl": "info",
  "evt": "BOOKING_DRAFT_CREATED",
  "rid": "0f0c2a...",
  "tid": "c2c3d9...",
  "org": "acme",
  "dur_ms": 42
}
```
EOF

write_file "$BASE/guides/testing.md" <<'EOF'
---
title: "Testing Strategy"
weight: 30
---

## Test pyramid (practical)

- Unit tests: fast, pure logic
- Integration tests: DB, queue, external APIs (containerized)
- End-to-end tests: small number, high value

## Naming

- `Foo_Should_Bar_When_Baz`
- Make failure messages obvious

{{< alert title="Rule" color="warning" >}}
If a bug reaches prod, add a test that would have caught it.
{{< /alert >}}
EOF

# How-to
write_file "$BASE/how-to/_index.md" <<'EOF'
---
title: "How-to"
linkTitle: "How-to"
weight: 30
---

Task-oriented procedures.
EOF

write_file "$BASE/how-to/add-endpoint.md" <<'EOF'
---
title: "Add an API endpoint"
weight: 10
---

## Steps

1. Add route/controller/handler
2. Validate input
3. Add logging + correlation id
4. Add tests
5. Update API reference docs

{{< alert title="Keep it boring" color="info" >}}
Prefer conventional REST patterns unless you have a strong reason not to.
{{< /alert >}}
EOF

write_file "$BASE/how-to/add-migration.md" <<'EOF'
---
title: "Add a database migration"
weight: 20
---

## Steps

1. Create migration
2. Review generated SQL
3. Apply locally
4. Add rollback notes
5. Confirm deploy order

```bash
# Replace with your tooling
make db-migration name=add_foo_table
make db-update
```

{{< alert title="Production safety" color="warning" >}}
Always document whether the migration is:
- backward compatible (safe to deploy before code), or
- requires code-first rollout.
{{< /alert >}}
EOF

write_file "$BASE/how-to/debug-prod.md" <<'EOF'
---
title: "Debug a production issue"
weight: 30
---

## Fast path

1. Confirm impact + scope
2. Check dashboards (errors, latency, saturation)
3. Use correlation ids to trace a single failing request
4. Identify rollback vs fix-forward
5. Write a short incident note

## Capture this info

- Exact time window
- Affected tenants/users
- Error codes and sample `rid`/`tid`
- Mitigation performed
EOF

# Reference
write_file "$BASE/reference/_index.md" <<'EOF'
---
title: "Reference"
linkTitle: "Reference"
weight: 40
---

Canonical facts and specs.
EOF

write_file "$BASE/reference/configuration.md" <<'EOF'
---
title: "Configuration"
weight: 10
---

## Environment variables (example)

| Key | Required | Default | Notes |
|-----|----------|---------|------|
| `APP_ENV` | yes | `dev` | `dev/stage/prod` |
| `PORT` | no | `8080` | HTTP port |
| `DATABASE_URL` | yes | - | Connection string |

{{< alert title="Doc pattern" color="info" >}}
Document config in one place. Link to it from Quickstart and Runbooks.
{{< /alert >}}
EOF

write_file "$BASE/reference/cli.md" <<'EOF'
---
title: "CLI"
weight: 20
---

## Commands (example)

```text
tool run            Run locally
tool test           Run tests
tool migrate up     Apply DB migrations
tool migrate down   Rollback DB migrations
```

## Examples

```bash
tool run --port 8080
tool migrate up
```
EOF

write_file "$BASE/reference/api.md" <<'EOF'
---
title: "API"
weight: 30
---

## OpenAPI / Swagger

- Local: `http://localhost:8080/swagger`
- Spec file: `docs/openapi.yaml` (example)

## Error format

Prefer a consistent problem-details style response.

```json
{
  "type": "https://example.com/problems/validation-error",
  "title": "Validation error",
  "status": 400,
  "detail": "One or more fields are invalid",
  "errors": {
    "email": ["Must be a valid email"]
  }
}
```
EOF

# Architecture
write_file "$BASE/architecture/_index.md" <<'EOF'
---
title: "Architecture"
linkTitle: "Architecture"
weight: 50
---

System overview and major design choices.
EOF

write_file "$BASE/architecture/overview.md" <<'EOF'
---
title: "System Overview"
weight: 10
---

## Components (example)

```mermaid
flowchart LR
  UI[Web UI] --> API[API Service]
  API --> DB[(Database)]
  API --> Q[Queue]
  W[Worker] --> DB
  W --> Q
```

## Key flows

- UI calls API
- API writes job to queue
- Worker consumes queue and persists results

{{< alert title="Keep diagrams small" color="info" >}}
Prefer multiple small diagrams over one “everything” diagram.
{{< /alert >}}
EOF

write_file "$BASE/architecture/modules.md" <<'EOF'
---
title: "Modules"
weight: 20
---

Describe your modules/packages and boundaries:

- `api/` – HTTP surface area
- `domain/` – business logic
- `infra/` – DB, queues, external providers
- `web/` – UI

## Dependency direction

- UI → API → Domain
- Infra plugs into Domain via interfaces
EOF

write_file "$BASE/architecture/data-model.md" <<'EOF'
---
title: "Data Model"
weight: 30
---

## Tables (example)

- `users`
- `organizations`
- `bookings`
- `jobs`

## Notes

- Put invariants in the domain layer
- Keep migrations reviewed and reversible where possible
EOF

write_file "$BASE/architecture/adr/_index.md" <<'EOF'
---
title: "Architecture Decision Records (ADRs)"
linkTitle: "ADRs"
weight: 40
---

ADRs capture *why* decisions were made, not just *what* changed.
EOF

write_file "$BASE/architecture/adr/0001-use-queue-for-long-tasks.md" <<'EOF'
---
title: "ADR-0001: Use a queue for long-running tasks"
weight: 10
---

## Status

Accepted

## Context

Some requests take >30s due to external providers and heavy processing.

## Decision

Use a queue + worker model for long tasks. API enqueues, worker processes asynchronously.

## Consequences

- Better API responsiveness
- Need observability (job status, retries, DLQ)
- Requires runbooks for stuck jobs
EOF

# Runbooks
write_file "$BASE/runbooks/_index.md" <<'EOF'
---
title: "Runbooks"
linkTitle: "Runbooks"
weight: 60
---

Operational procedures and incident playbooks.
EOF

write_file "$BASE/runbooks/deploy.md" <<'EOF'
---
title: "Deploy"
weight: 10
---

## Preconditions

- CI green on main
- Migration reviewed
- Rollback plan ready

## Steps (example)

1. Apply migrations (if backward compatible)
2. Deploy API
3. Deploy worker
4. Verify health + key workflows
5. Announce completion

## Rollback

- Roll back code first
- Roll back migration only if explicitly safe
EOF

write_file "$BASE/runbooks/incident.md" <<'EOF'
---
title: "Incident Response"
weight: 20
---

## Severity (example)

- SEV1: full outage / data loss risk
- SEV2: major feature broken
- SEV3: degraded / workaround exists

## Triage checklist

- What changed?
- Is it isolated to one tenant/region?
- Any elevated 5xx / latency?
- Recent deploy or config change?

## Comms template

- Impact:
- Start time:
- Current status:
- Next update:
EOF

# Release notes
write_file "$BASE/release-notes/_index.md" <<'EOF'
---
title: "Release Notes"
linkTitle: "Release Notes"
weight: 70
---

Chronological changes and upgrade notes.
EOF

write_file "$BASE/release-notes/v0.1.0.md" <<'EOF'
---
title: "v0.1.0"
weight: 10
---

## Added

- Initial docs structure
- Quickstart
- Reference placeholders

## Changed

- N/A

## Fixed

- N/A
EOF

# Contributing
write_file "$BASE/contributing/_index.md" <<'EOF'
---
title: "Contributing"
linkTitle: "Contributing"
weight: 80
---

How to work on this repo.
EOF

write_file "$BASE/contributing/style-guide.md" <<'EOF'
---
title: "Docs style guide"
weight: 10
---

## Page types

- **Guide**: explain concepts and conventions
- **How-to**: procedures
- **Reference**: facts and specs
- **Runbook**: operational procedures

## Writing rules

- Use headings + lists
- Put commands in code fences
- Link to the canonical reference page (don’t duplicate)
- Prefer screenshots only when necessary

{{< alert title="Doc review" color="info" >}}
Any PR that changes behaviour should change docs (or explicitly say why not).
{{< /alert >}}
EOF

# A simple FAQ at /docs/faq/
write_file "$BASE/faq.md" <<'EOF'
---
title: "FAQ"
weight: 90
---

## Where do I put X?

- “How do I do a task?” → **How-to**
- “What is the value of a config key?” → **Reference**
- “Why are we doing it this way?” → **ADR**
- “What do I do during an outage?” → **Runbooks**
EOF

echo
echo "Done."
echo "Seeded content under: $BASE"
echo
echo "Re-run with FORCE=1 to overwrite existing files:"
echo "  FORCE=1 ./seed-docsy-content.sh"
