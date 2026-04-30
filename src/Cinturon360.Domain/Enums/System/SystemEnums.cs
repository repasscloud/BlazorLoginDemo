namespace Cinturon360.Domain.Enums.System;

public enum JobStatus
{
    Queued     = 1,
    Running    = 2,
    Succeeded  = 3,
    Failed     = 4,
    Cancelled  = 5,
    Retrying   = 6
}

public enum JobType
{
    SendEmail             = 1,
    GeneratePdf           = 2,
    SyncAirports          = 3,
    SendApprovalNotice    = 4,
    ProcessPayment        = 5,
    ArchiveBooking        = 6,
    CleanupExpiredSessions = 7,
    ReportGeneration      = 8,
    SyncExchangeRates     = 9
}

public enum DocumentType
{
    BookingConfirmation = 1,
    Invoice             = 2,
    ItineraryPdf        = 3,
    PolicyDocument      = 4,
    TravelReport        = 5,
    Other               = 6
}
