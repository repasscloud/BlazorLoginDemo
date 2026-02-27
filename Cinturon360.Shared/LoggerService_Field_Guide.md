# LoggerService field guide

This describes the fields in your `ILoggerService` / `LoggerService` logging calls and what they’re typically used for.

Source: your interface and implementation (`ILoggerService`, `LoggerService`).

## Big picture

Your `LoggerService` creates an `AvaSystemLog` row and saves it to the database (`_db.AvaSystemLogs.Add(entry); await _db.SaveChangesAsync();`).  
It standardizes logging into a consistent schema: **who/what/where**, plus **timing**, plus **HTTP context**, plus **an event code**.

It also enforces a **minimum log level** from config (`Logging:LogLevel:Default`) and drops anything below that level.

## Core “dimensions” (not in your question, but important context)

- **level** (`SysLogLevel`): Verbose/Debug/Information/Warning/Error/Fatal.  
  Used for filtering and alerting. Calls like `ErrorAsync(...)` map to `SysLogLevel.Error`.

- **evt** (string): short event key / code (example: `TRAVEL_POLICY_CREATE`).  
  This is the thing you use to aggregate and search logs reliably without text searching.

- **cat** (`SysLogCatType`): broad category (e.g., Auth, Data, Network, etc.).  
  Lets you slice logs by domain area.

- **act** (`SysLogActionType`): what kind of action occurred (Create/Read/Update/Delete/Execute/etc.).  
  Useful for auditing.

- **outcome** (`SysLogOutcome`): OK/WARN/ERR/FAIL (or whatever your enum defines).  
  In the helpers, you default outcomes by severity (Info→OK, Warning→WARN, Error→ERR, Fatal→FAIL), unless `overrideOutcome` is provided.

## Field reference (the ones you asked about)

### `message`
Human-readable description of what happened. Keep it short and structured.

- In your implementation, the stored message is **`ComposeMessage(message, ex)`**:
  - if there’s no exception: it stores `message` (or `""` if null)
  - if there *is* an exception: it stores `"<message or 'Exception'> | <ExceptionType>: <Exception.Message>"`

Recommendation:
- Put “what you were trying to do” and the key identifiers in `message`, and use the structured fields for the identifiers.

### `ent`
“Entity” name: the thing in your domain model that the log line is about.

Examples:
- `"TravelPolicy"`, `"Organization"`, `"QueuedJob"`, `"User"`, `"Invoice"`

Purpose:
- Lets you quickly filter logs by entity without parsing `message`.

### `entId`
The identifier of the entity in `ent`.

Examples:
- policy id, queued job id, org unified id, etc.

Purpose:
- Enables "show me everything that happened to entity X" queries.

### `rid` (Request ID)
Correlation id for a **single inbound request** / operation.

In your code:
- If `rid` wasn’t provided, it auto-generates a new GUID string using `"N"` format (32 hex chars, no dashes).

Purpose:
- When one request triggers multiple log rows, you can group them by `rid` to get a full timeline.

Where to set it:
- In a web API, set `rid` once per request (middleware), then pass it through all service calls/logs.

### `tid` (Trace ID / Transaction ID)
A higher-level correlation id, typically one of:

- Distributed tracing id (e.g., OpenTelemetry `traceId`)
- Cross-service transaction id (covers many requests)
- “Business transaction” id (booking flow id, checkout flow id)

Purpose:
- Correlates activity across boundaries where `rid` changes, e.g. async workflows, background jobs, fan-out.

Guideline:
- If you adopt OpenTelemetry, set `tid` to the current `Activity.TraceId`.

### `uid`
User id (the actor) performing the action.

Examples:
- your internal user id, auth subject, etc.

Purpose:
- auditing (“who changed what”), security investigations, support.

Guideline:
- Store a stable identifier (not email, unless emails are immutable in your system).
- Avoid putting secrets/PII here unless it’s your explicit design.

### `org`
Organization id / tenant id (in a multi-tenant system).

Examples:
- “Platform/Vendor org unified id”, “Client org unified id”.

Purpose:
- tenant scoping (“show errors for org X”), and critical for multi-tenant debugging.

### `durMs`
Duration in milliseconds for the operation being logged.

Examples:
- time to call an upstream API, time to complete a DB query, time for a handler.

Purpose:
- performance tracking, latency heatmaps, regression detection.

Guideline:
- Use it consistently for “end of operation” logs (or for key spans within a request).

### `http`
HTTP method (or a compact HTTP descriptor).

Most common expected values:
- `"GET"`, `"POST"`, `"PUT"`, `"PATCH"`, `"DELETE"`

Purpose:
- makes API logs searchable and allows “errors by method” analysis.

Note:
- In some schemas people store `"GET https://.../"` here, but in your model you already have `path`, so keep `http` to the method.

### `stat`
HTTP status code (integer).

Examples:
- `200`, `400`, `401`, `403`, `404`, `409`, `422`, `500`.

Purpose:
- fast triage and dashboards.

Guideline:
- For non-HTTP events (background jobs), leave null.

### `path`
HTTP route/path or logical resource path.

Examples:
- `"/api/orgs/{id}"` (route template)
- `"/api/orgs/abc123"` (raw path)

Purpose:
- “what endpoint is failing?” and routing-specific analysis.

Guideline:
- Prefer route template if you can (reduces cardinality), but raw path can be useful for debugging.

### `note`
Free-form extra context that doesn’t fit elsewhere.

Examples:
- “retry=2”, “remote=SendGrid”, “featureFlag=NewCheckout”, “validation=PolicyName missing”.

Purpose:
- additional context without expanding the schema.

Guideline:
- Don’t put secrets here (tokens, passwords, auth headers).

## Suggested conventions (so the fields stay useful)

1. **evt is mandatory**, stable, and upper snake case.  
   Treat it like a “log event API”.

2. Use **ent + entId** whenever any domain object is involved.

3. Always set **rid** for request-scoped work; pass it down into deeper calls.

4. Set **org** and **uid** whenever known; missing these kills multi-tenant debugging.

5. Use **durMs** on operation completion logs.

6. Keep **message** short; store details in structured fields.

## Example (HTTP request)

Information log at the start:
- evt: `ORG_LIST`
- cat: `Api`
- act: `Read`
- rid: request correlation id
- tid: trace id
- uid: authenticated user id
- org: tenant id
- http: `GET`
- path: `/api/orgs`
- note: `pageSize=50`

Then an Error with:
- stat: `500`
- durMs: `1234`
- message: `"Failed to list orgs"`
- ex: exception

That gives you both human context and machine-filterable fields.
