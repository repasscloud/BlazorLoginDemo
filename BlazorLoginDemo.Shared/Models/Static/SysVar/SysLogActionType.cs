namespace BlazorLoginDemo.Shared.Models.Static.SysVar;

public enum SysLogActionType : int
{
    // 0–9 base
    Unknown     = 0,

    // 10–49 CRUD
    Create      = 10,
    Read        = 20,
    Update      = 30,
    Delete      = 40,

    // 50–99 Execution & UI
    Exec        = 50,
    Start       = 60,
    End         = 70,
    Step        = 80,
    View        = 90,
    Click       = 100,

    // 110–179 Workflow decisions
    Submit      = 110,
    Approve     = 120,
    Reject      = 130,
    Assign      = 140,
    Unassign    = 150,

    // 180–239 Data movement/validation
    Import      = 180,
    Export      = 190,
    Validate    = 200,
    Sync        = 210,
    Reconcile   = 220,

    // 240–279 Control flow
    Cancel      = 240,
    Retry       = 250,
    Pause       = 260,
    Resume      = 270,

    // 280–319 Queueing
    Enqueue     = 280,
    Dequeue     = 290,

    // 320–359 Auth
    Login       = 320,
    Logout      = 330
}
