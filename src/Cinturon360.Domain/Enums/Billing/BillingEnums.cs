namespace Cinturon360.Domain.Enums.Billing;

public enum LicenseType
{
    Free        = 1,
    Starter     = 2,
    Business    = 3,
    Enterprise  = 4
}

public enum BillingCycle
{
    Monthly  = 1,
    Annual   = 2
}

public enum InvoiceStatus
{
    Draft      = 1,
    Sent       = 2,
    Paid       = 3,
    Void       = 4,
    Overdue    = 5,
    Disputed   = 6
}

public enum PaymentStatus
{
    Pending    = 1,
    Processing = 2,
    Succeeded  = 3,
    Failed     = 4,
    Refunded   = 5
}

public enum PaymentMethod
{
    Card           = 1,
    BankTransfer   = 2,
    PrepaidBalance = 3,
    Crypto         = 4
}
