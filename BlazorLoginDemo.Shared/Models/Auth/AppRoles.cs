namespace BlazorLoginDemo.Shared.Auth;

public static class AppRoles
{
    public static class GlobalRole
    {
        // Top-tier / cross-tenant
        public const string SuperAdmin = "SuperAdmin";
        public const string Auditor = "Auditor";

        // Generic platform admins by domain (global scope)
        public const string UserAdmin = "UserAdmin";
        public const string PolicyAdmin = "PolicyAdmin";
        public const string FinanceAdmin = "FinanceAdmin";
        public const string FinanceEditor = "FinanceEditor";
        public const string FinanceViewer = "FinanceViewer";
        public const string SecurityAdmin = "SecurityAdmin";
        public const string IntegrationAdmin = "IntegrationAdmin";

        // Sales (global scope)
        public const string SalesRep = "SalesRep";
        public const string SalesManager = "SalesManager";
        public const string SalesAdmin = "SalesAdmin";

        // Support (global scope)
        public const string SupportViewer = "SupportViewer";
        public const string SupportAgent = "SupportAgent";
        public const string SupportFinance = "SupportFinance";
        public const string SupportAdmin = "SupportAdmin";

        // Reporting / export (global scope)
        public const string ReportsViewer = "ReportsViewer";
        public const string DataExporter = "DataExporter";

        // Generic read-only/requestor (global)
        public const string Requestor = "Requestor";
        public const string ReadOnly = "ReadOnly";
    }

    // ORG-SCOPED ROLES (must be tied to a specific tenant/org)
    public static class OrgRole
    {
        public const string Admin = "OrgAdmin";
        public const string UserAdmin = "OrgUserAdmin";
        public const string PolicyAdmin = "OrgPolicyAdmin";
        public const string FinanceAdmin = "OrgFinanceAdmin";
        public const string BookingsManager = "OrgBookingsManager";

        public const string ApproverL1 = "OrgApproverL1";
        public const string ApproverL2 = "OrgApproverL2";
        public const string ApproverL3 = "OrgApproverL3";

        public const string ReportsViewer = "OrgReportsViewer";
        public const string DataExporter = "OrgDataExporter";
        // If you later want OrgFinanceViewer/Editor, add here.
    }
}