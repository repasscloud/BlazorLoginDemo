
# Unified Logging Spec (Platform + Web + Workers)

Purpose: single, compact, structured logging format across API, automations, workflows, UI, data, security, integrations, and system health. Tight header for grepping. Full structured properties for analytics.

## Design Rules

- Always structured. Header + properties.
- Short keys. Stable enums. Fixed field order.
- Every event has a correlation id `RID` and tenant `TID`.
- Never hide identifiers in free text. Put in fields.
- `NOTE` is short human hint only.
- Exceptions logged with `Error`/`Fatal`. Include full `ex` object.

## Header Shape

Header is a single, small line. Example:
```
EVT={Code} CAT={Cat} ACT={Act} OUT={Out} ENT={Ent} EntId={EntId} RID={Rid} TID={TenantId} UID={UserId} ORG={OrgId} DUR={DurMs}MS
```

Emit the same as structured properties so log backends index each token.

## Canonical Fields

| Field | Key | Type | Example | Why |
|---|---|---|---|---|
| Event code | `EVT` | string | `API_REQ_END` | Durable selector for dashboards and alerts |
| Category | `CAT` | enum | `API`,`AUTO`,`WF`,`UI`,`SEC`,`DATA`,`INT`,`SYS` | High-level bucket |
| Action | `ACT` | enum | `READ`,`CREATE`,`UPDATE`,`DELETE`,`EXEC`,`START`,`END`,`STEP`,`VIEW`,`CLICK`,`LOGIN` | Verb |
| Outcome | `OUT` | enum | `OK`,`WARN`,`ERR`,`FAIL`,`DENY`,`TIMEOUT`,`RETRY`,`CANCEL` | Normalized result |
| Entity type | `ENT` | string | `Discount`,`Quote`,`User`,`Org`,`Job`,`Workflow` | Domain noun |
| Entity id | `EntId` | string | `dc_123` | Target identifier |
| Request/Correlation id | `RID` | string | `0f1…` | Cross-service join |
| Span id | `SPAN` | string | `a1b2c3` | Trace span |
| Parent span | `PSPAN` | string | `…` | Parent span |
| Tenant | `TID` | string | `EvoTss:acme` | Multi-tenant pivot |
| Org | `ORG` | string | GUID | Org pivot |
| User | `UID` | string | `auth0|abc` | Actor |
| Role | `ROLE` | string | `Admin` | Access context |
| HTTP verb | `HTTP` | string | `GET` | API |
| HTTP status | `STAT` | int | `200` | API |
| Path/Route | `PATH` | string | `/api/discounts/{id}` | API/UI |
| Duration ms | `DUR` | int | `42` | SLOs |
| Attempt | `TRY` | int | `2` | Retries |
| Count | `CNT` | int | `137` | Rows/items |
| Bytes in/out | `BIN`,`BOUT` | long | `512`,`2048` | Payload sizes |
| Scheduler | `SCH` | string | `cron(0 2 * * *)` | Automations |
| Job id | `JOB` | string | `tax-validate-2025Q4` | Automations |
| Step breadcrumb | `STEP` | string | `Validate->Price->Ticket` | Workflows |
| Provider | `PROV` | string | `Amadeus`,`Stripe` | Integrations |
| Region | `REG` | string | `AU-SYD` | Locality |
| Host/Pod | `HOST` | string | `web-7d9c` | Instance |
| Version | `VER` | string | `web 1.12.3+sha` | Build |
| Note | `NOTE` | string | short text | Human hint |

## Categories and Typical Events

| CAT | Typical `EVT` codes | Minimal header sample |
|---|---|---|
| `API` | `API_REQ_START`,`API_REQ_END`,`API_REQ_ERR`,`API_VALIDATION_FAIL`,`API_DEP_CALL` | `EVT=API_REQ_END CAT=API ACT=READ OUT=OK ENT=Discount EntId=dc_123 HTTP=GET STAT=200 PATH=/api/discounts/dc_123 RID=… TID=… UID=… DUR=38MS` |
| `AUTO` | `AUTO_JOB_START`,`AUTO_JOB_END`,`AUTO_JOB_ERR`,`AUTO_RETRY` | `EVT=AUTO_JOB_END CAT=AUTO ACT=EXEC OUT=OK JOB=tax-validate-2025Q4 TRY=1 CNT=842 DUR=9134MS RID=…` |
| `WF` | `WF_STEP_START`,`WF_STEP_END`,`WF_ERR`,`WF_COMPENSATE` | `EVT=WF_STEP_END CAT=WF ACT=STEP OUT=OK STEP=Price->Hold->Ticket ENT=Quote EntId=q_991 DUR=212MS RID=…` |
| `UI` | `UI_PAGE_VIEW`,`UI_CLICK`,`UI_ACTION`,`UI_DENY` | `EVT=UI_PAGE_VIEW CAT=UI ACT=VIEW OUT=OK ENT=DiscountPage EntId=New RID=… UID=…` |
| `SEC` | `SEC_LOGIN_OK`,`SEC_LOGIN_FAIL`,`SEC_DENY`,`SEC_ROLE_CHANGE` | `EVT=SEC_LOGIN_FAIL CAT=SEC ACT=LOGIN OUT=DENY UID=user@corp REG=AU-SYD RID=… NOTE=bad_password` |
| `DATA` | `DATA_READ`,`DATA_WRITE`,`DATA_MIGRATE`,`DATA_ERR` | `EVT=DATA_WRITE CAT=DATA ACT=UPDATE OUT=OK ENT=Policy EntId=p_7 CNT=1 DUR=14MS RID=…` |
| `INT` | `INT_CALL_START`,`INT_CALL_END`,`INT_ERR`,`INT_TIMEOUT` | `EVT=INT_CALL_END CAT=INT ACT=EXEC OUT=OK PROV=Stripe STAT=200 DUR=321MS RID=…` |
| `SYS` | `SYS_HEALTH`,`SYS_SCALE_UP`,`SYS_SCALE_DOWN`,`SYS_ERR` | `EVT=SYS_HEALTH CAT=SYS ACT=EXEC OUT=OK HOST=web-7d9c VER=…` |

## Log Levels

| Level | Use |
|---|---|
| `Verbose` | Very fine diagnostics. Off by default in prod. |
| `Debug` | Dev context, cache states, decisions. |
| `Information` | Lifecycle and business events. Default. |
| `Warning` | Degraded behavior, retries, partial failures. |
| `Error` | Operation failed. User-visible or operator action. Include `ex`. |
| `Fatal` | Process compromised. One exit log with environment context. |

## Message Templates

Use structured templates with short keys.

**API end**
```
EVT=API_REQ_END CAT=API ACT={ACT} OUT={OUT} STAT={STAT} DUR={DUR}MS RID={RID} PATH={PATH} ENT={ENT} EntId={EntId} CNT={CNT}
```

**Automation**
```
EVT=AUTO_JOB_{Phase} CAT=AUTO ACT=EXEC OUT={OUT} JOB={JOB} TRY={TRY} DUR={DUR}MS CNT={CNT} RID={RID}
```

**Workflow step**
```
EVT=WF_STEP_{Phase} CAT=WF ACT=STEP OUT={OUT} STEP={STEP} ENT={ENT} EntId={EntId} DUR={DUR}MS RID={RID}
```

**User action**
```
EVT=UI_ACTION CAT=UI ACT={ACT} OUT={OUT} ENT={ENT} EntId={EntId} UID={UID} ORG={ORG} RID={RID} NOTE={NOTE}
```

**Integration call**
```
EVT=INT_CALL_{Phase} CAT=INT ACT=EXEC OUT={OUT} PROV={PROV} STAT={STAT} DUR={DUR}MS RID={RID}
```

**Data write**
```
EVT=DATA_{ACT} CAT=DATA OUT={OUT} ENT={ENT} EntId={EntId} CNT={CNT} DUR={DUR}MS RID={RID}
```

**Security**
```
EVT=SEC_{Kind} CAT=SEC ACT={ACT} OUT={OUT} UID={UID} ROLE={ROLE} RID={RID} NOTE={NOTE}
```

## C# Helpers (Serilog-style)

```csharp
public enum LogCat { API, AUTO, WF, UI, SEC, DATA, INT, SYS }
public enum LogAct { READ, CREATE, UPDATE, DELETE, EXEC, START, END, STEP, VIEW, CLICK, LOGIN, LOGOUT }
public enum LogOut { OK, WARN, ERR, FAIL, DENY, TIMEOUT, RETRY, CANCEL }

public sealed record LogCtx(
    string Rid,
    string? TenantId = null,
    string? OrgId = null,
    string? UserId = null,
    string? Role = null,
    string? Host = null,
    string? Version = null,
    string? Region = null);
```

```csharp
public static class LogFmt
{
    public static IDisposable Push(this LogCtx c)
        => Serilog.Context.LogContext.PushProperties(
            new Serilog.Core.LogEventProperty[] {
                new("RID", new ScalarValue(c.Rid)),
                new("TID", new ScalarValue(c.TenantId)),
                new("ORG", new ScalarValue(c.OrgId)),
                new("UID", new ScalarValue(c.UserId)),
                new("ROLE", new ScalarValue(c.Role)),
                new("HOST", new ScalarValue(c.Host)),
                new("VER", new ScalarValue(c.Version)),
                new("REG", new ScalarValue(c.Region))
            });

    static string Ev(string code) => code;

    public static void UiAction(ILogger log, LogCtx ctx, LogAct act, LogOut outcome,
                                string entity, string? entityId = null, string? note = null, object? extra = null)
    {
        using var _ = ctx.Push();
        log.Information(
            "EVT={EVT} CAT={CAT} ACT={ACT} OUT={OUT} ENT={ENT} EntId={EntId} UID={UID} ORG={ORG} RID={RID} NOTE={NOTE} {@Extra}",
            Ev("UI_ACTION"), LogCat.UI, act, outcome, entity, entityId, ctx.UserId, ctx.OrgId, ctx.Rid, note, extra);
    }

    public static void ApiEnd(ILogger log, LogCtx ctx, LogAct act, LogOut outcome, int status, string path,
                              string? entity = null, string? entityId = null, int? count = null, int? durMs = null,
                              object? extra = null)
    {
        using var _ = ctx.Push();
        log.Information(
            "EVT={EVT} CAT={CAT} ACT={ACT} OUT={OUT} STAT={STAT} DUR={DUR}MS RID={RID} PATH={PATH} ENT={ENT} EntId={EntId} CNT={CNT} {@Extra}",
            Ev("API_REQ_END"), LogCat.API, act, outcome, status, durMs, ctx.Rid, path, entity, entityId, count, extra);
    }

    public static void Err(ILogger log, LogCtx ctx, string evtCode, LogCat cat, LogAct act, Exception ex,
                           string? entity = null, string? entityId = null, string? note = null, object? extra = null)
    {
        using var _ = ctx.Push();
        log.Error(ex,
            "EVT={EVT} CAT={CAT} ACT={ACT} OUT={OUT} ENT={ENT} EntId={EntId} NOTE={NOTE} RID={RID} {@Extra}",
            Ev(evtCode), cat, act, LogOut.ERR, entity, entityId, note, ctx.Rid, extra);
    }
}
```

## Minimal Middleware

```csharp
public class LogCtxMiddleware
{
    private readonly RequestDelegate _next;
    public LogCtxMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        var rid = ctx.TraceIdentifier;
        var lc = new LogCtx(
            Rid: rid,
            TenantId: ctx.User.FindFirst("tid")?.Value,
            OrgId: ctx.User.FindFirst("org")?.Value ?? ctx.Request.Headers["x-org-id"],
            UserId: ctx.User.Identity?.Name,
            Role: ctx.User.FindFirst("role")?.Value,
            Host: Environment.MachineName,
            Version: Environment.GetEnvironmentVariable("BUILD_VERSION"),
            Region: Environment.GetEnvironmentVariable("REGION"));

        ctx.Items["LogCtx"] = lc;
        using (Serilog.Context.LogContext.PushProperty("RID", rid))
        {
            await _next(ctx);
        }
    }
}

public static class HttpContextLogCtxExtensions
{
    public static LogCtx GetLogCtx(this HttpContext ctx)
        => (LogCtx)(ctx.Items["LogCtx"] ?? new LogCtx(Guid.NewGuid().ToString()));
}
```

## DB Storage (Postgres)

```sql
CREATE TABLE IF NOT EXISTS app_log (
  id      bigserial PRIMARY KEY,
  ts      timestamptz NOT NULL DEFAULT now(),
  level   varchar(10) NOT NULL,
  evt     varchar(40) NOT NULL,
  cat     varchar(10) NOT NULL,
  act     varchar(12) NOT NULL,
  "out"   varchar(10) NOT NULL,
  ent     varchar(40),
  ent_id  varchar(80),
  rid     varchar(64) NOT NULL,
  tid     varchar(64),
  uid     varchar(128),
  org     varchar(64),
  dur_ms  int,
  http    varchar(8),
  stat    int,
  path    text,
  note    text,
  props   jsonb NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_app_log_ts    ON app_log (ts DESC);
CREATE INDEX IF NOT EXISTS ix_app_log_evt   ON app_log (evt);
CREATE INDEX IF NOT EXISTS ix_app_log_cat   ON app_log (cat);
CREATE INDEX IF NOT EXISTS ix_app_log_out   ON app_log ("out");
CREATE INDEX IF NOT EXISTS ix_app_log_rid   ON app_log (rid);
CREATE INDEX IF NOT EXISTS ix_app_log_tid   ON app_log (tid);
CREATE INDEX IF NOT EXISTS ix_app_log_uid   ON app_log (uid);
CREATE INDEX IF NOT EXISTS ix_app_log_ent   ON app_log (ent, ent_id);
CREATE INDEX IF NOT EXISTS ix_app_log_props ON app_log USING GIN (props jsonb_path_ops);
```

### EF Core Model

```csharp
public sealed class AppLog
{
    public long Id { get; set; }
    public DateTimeOffset Ts { get; set; }
    public string Level { get; set; } = "";
    public string Evt { get; set; } = "";
    public string Cat { get; set; } = "";
    public string Act { get; set; } = "";
    public string Out { get; set; } = "";
    public string? Ent { get; set; }
    public string? EntId { get; set; }
    public string Rid { get; set; } = "";
    public string? Tid { get; set; }
    public string? Uid { get; set; }
    public string? Org { get; set; }
    public int? DurMs { get; set; }
    public string? Http { get; set; }
    public int? Stat { get; set; }
    public string? Path { get; set; }
    public string? Note { get; set; }
    public string Props { get; set; } = "{}"; // jsonb
}
```

```csharp
public sealed class AppLogConfig : IEntityTypeConfiguration<AppLog>
{
    public void Configure(EntityTypeBuilder<AppLog> b)
    {
        b.ToTable("app_log");
        b.HasKey(x => x.Id);
        b.Property(x => x.Props).HasColumnType("jsonb");
        b.HasIndex(x => x.Ts).HasDatabaseName("ix_app_log_ts");
        b.HasIndex(x => new { x.Cat, x.Evt });
        b.HasIndex(x => x.Rid);
        b.HasIndex(x => new { x.Ent, x.EntId });
    }
}
```

## Example Messages (literal)

Header-only:
```
EVT=UI_ACTION CAT=UI ACT=VIEW OUT=OK ENT=DiscountPage EntId=New RID=1c7d2a6b1c9a4b7f97a8475c1d0f9e22 TID=EvoTss:acme UID=auth0|u_123 ORG=28a8c173-d4b8-43db-9730-5b4caf4ae4ac
EVT=UI_ACTION CAT=UI ACT=CLICK OUT=OK ENT=OrgAssign EntId=28a8c173-d4b8-43db-9730-5b4caf4ae4ac RID=1c7d2a6b1c9a4b7f97a8475c1d0f9e22 TID=EvoTss:acme UID=auth0|u_123 ORG=28a8c173-d4b8-43db-9730-5b4caf4ae4ac NOTE="Assign to NewDiscount"
EVT=API_REQ_END CAT=API ACT=CREATE OUT=OK ENT=Discount EntId=dc_001 HTTP=POST STAT=201 PATH=/api/discounts RID=4b3b1e7b6a4f49b0a3c1f2e8b9d0aa77 TID=EvoTss:acme UID=auth0|u_123 DUR=84MS CNT=1
```

Header + JSON:
```
EVT=INT_CALL_END CAT=INT ACT=EXEC OUT=ERR PROV=Stripe STAT=402 RID=6e0a8f3c2d2140f98c1b77a1d2e3f444 TID=EvoTss:acme DUR=321MS NOTE="card_declined" {"Attempt":1,"Op":"PaymentIntentConfirm"}
EVT=WF_STEP_END CAT=WF ACT=STEP OUT=OK STEP=Price->Hold->Ticket ENT=Quote EntId=q_991 DUR=212MS RID=b12c8e7a9f4d4c2b8a5e1d9c3f0a7e66 {"PROV":"Amadeus","PNR":"ABC123"}
```

## Onboarding Checklist

- [ ] Add `LogCtxMiddleware` and register early in pipeline.
- [ ] Ensure `RID` propagates to background jobs and bus messages.
- [ ] Wrap controllers/pages/services with helpers in `LogFmt`.
- [ ] Configure sinks (Seq/Elasticsearch/Postgres/File) to store properties and keep message as-is.
- [ ] Ship dashboards: API p95, INT failures, tenant split, UI funnel.
- [ ] Alert on `EVT` + `OUT`, not on free text.

## Migration Notes

- Map old message keys to new fields; keep both during transition.
- Start with `CAT`=`API`,`INT`,`UI`. Expand to `AUTO`,`WF`,`SEC`,`DATA`,`SYS` later.
- Add `EVT` codes via constants or a source generator for compile-time safety.
- Keep header order stable. Avoid extra whitespace for grep compatibility.
