namespace BlazorLoginDemo.Shared.Auth;

public static class AppPolicies
{
    public static class PlatformPolicy
    {
        public const string AdminArea = "Platform.AdminArea";
        public const string SupportArea = "Platform.SupportArea";
        public const string FinanceWrite = "Platform.FinanceWrite";
        public const string FinanceRead = "Platform.FinanceRead";
        public const string SalesArea = "Platform.SalesArea";
        public const string ReportsRead = "Platform.ReportsRead";
        public const string DataExport = "Platform.DataExport";
    }

    public static class TmcPolicy
    {
        public const string AdminArea = "Tmc.AdminArea";
        public const string FinanceWrite = "Tmc.FinanceWrite";
        public const string FinanceRead = "Tmc.FinanceRead";
        public const string BookingsOps = "Tmc.BookingsOps";
        public const string ReportsRead = "Tmc.ReportsRead";
        public const string DataExport = "Tmc.DataExport";
    }

    public static class ClientPolicy
    {
        public const string AdminArea = "Client.AdminArea";
        public const string FinanceWrite = "Client.FinanceWrite";
        public const string FinanceRead = "Client.FinanceRead";
        public const string ApproverL1Plus = "Client.ApproverL1Plus";
        public const string ApproverL2Plus = "Client.ApproverL2Plus";
        public const string ApproverL3Only = "Client.ApproverL3Only";
        public const string ReportsRead = "Client.ReportsRead";
        public const string DataExport = "Client.DataExport";
        public const string Requestor = "Client.Requestor";
    }
}
