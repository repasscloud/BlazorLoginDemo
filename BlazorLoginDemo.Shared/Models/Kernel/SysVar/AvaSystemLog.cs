using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorLoginDemo.Shared.Models.Static.SysVar;

namespace BlazorLoginDemo.Shared.Models.Kernel.SysVar
{
    public enum SysLogLevel : int { Verbose=10, Debug=20, Information=30, Warning=40, Error=50, Fatal=60 }
    public enum SysLogOutcome : int { OK=0, WARN=1, ERR=2, FAIL=3, DENY=4, TIMEOUT=5, RETRY=6, CANCEL=7 }

    public class AvaSystemLog
    {
        [Key] public long Id { get; set; }

        // Always UTC
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        // Level / codes
        [Required] public SysLogLevel Level { get; set; } = SysLogLevel.Information;

        [Required, MaxLength(80)]
        public string Evt { get; set; } = string.Empty;            // e.g. API_REQ_END, UI_ACTION

        [Required] public SysLogCatType Cat { get; set; }          // category enum
        [Required] public SysLogActionType Act { get; set; }       // action enum
        [Required] public SysLogOutcome Out { get; set; } = SysLogOutcome.OK;

        // Domain target
        [MaxLength(80)]  public string? Ent { get; set; }           // e.g. Discount, Quote
        [MaxLength(80)]  public string? EntId { get; set; }

        // Correlation / tenancy
        [Required, MaxLength(64)]
        public string Rid { get; set; } = string.Empty;             // request/correlation id

        [MaxLength(64)]  public string? Tid { get; set; }           // tenant
        [MaxLength(128)] public string? Uid { get; set; }           // user
        [MaxLength(64)]  public string? Org { get; set; }           // org

        // Timing / http
        public int? DurMs { get; set; }
        [MaxLength(8)]   public string? Http { get; set; }          // GET/POST
        public int? Stat { get; set; }                              // status code
        [MaxLength(512)] public string? Path { get; set; }

        // Free text
        [MaxLength(512)] public string? Note { get; set; }
        [MaxLength(2048)] public string Message { get; set; } = string.Empty; // optional narrative

        // Computed compact header for quick greps (not stored unless you want to)
        [NotMapped]
        public string Header =>
            $"EVT={Evt} CAT={Cat} ACT={Act} OUT={Out} " +
            $"{(Ent is { Length: >0 } ? $"ENT={Ent} " : "")}" +
            $"{(EntId is { Length: >0 } ? $"EntId={EntId} " : "")}" +
            $"RID={Rid}" +
            $"{(string.IsNullOrWhiteSpace(Tid) ? "" : $" TID={Tid}")}" +
            $"{(string.IsNullOrWhiteSpace(Uid) ? "" : $" UID={Uid}")}" +
            $"{(string.IsNullOrWhiteSpace(Org) ? "" : $" ORG={Org}")}" +
            $"{(DurMs is null ? "" : $" DUR={DurMs}MS")}" +
            $"{(string.IsNullOrWhiteSpace(Http) ? "" : $" HTTP={Http}")}" +
            $"{(Stat is null ? "" : $" STAT={Stat}")}" +
            $"{(string.IsNullOrWhiteSpace(Path) ? "" : $" PATH={Path}")}" +
            $"{(string.IsNullOrWhiteSpace(Note) ? "" : $" NOTE=\"{Note}\"")}";
    }
}
