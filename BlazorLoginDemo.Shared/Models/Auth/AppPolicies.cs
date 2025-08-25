namespace BlazorLoginDemo.Shared.Auth;

public static class AppPolicies
{
    public static class GlobalPolicy
    {
        public const string RequireMemberOrAbove = "Global.RequireMemberOrAbove"; // if you still use Member/Manager/Admin
        public const string ManagersOnly = "Global.ManagersOnly";
        public const string AdminsOnly = "Global.AdminsOnly";

        public const string CanManageUsers = "Global.CanManageUsers";
        public const string CanEditPolicies = "Global.CanEditPolicies";
        public const string CanEditFinancials = "Global.CanEditFinancials";
        public const string FinanceRead = "Global.FinanceRead";
        public const string CanEnableDisableUser = "Global.CanEnableDisableUser";

        public const string SupportArea = "Global.SupportArea";
        public const string CanManageGroups = "Global.CanManageGroups";

        public const string SalesArea = "Global.SalesArea";
        public const string CanCreateCustomers = "Global.CanCreateCustomers";
        public const string LicenseRead = "Global.LicenseRead";
        public const string CanManageLicenses = "Global.CanManageLicenses";
        public const string CanAmendLicenses = "Global.CanAmendLicenses";
        public const string CanApproveDiscounts = "Global.CanApproveDiscounts";
    }

    public static class OrgPolicy
    {
        // These are tenant-scoped and require matching orgId
        public const string Admin = "Org.Admin";
        public const string UserAdmin = "Org.UserAdmin";
        public const string PolicyAdmin = "Org.PolicyAdmin";
        public const string FinanceAdmin = "Org.FinanceAdmin";
        public const string BookingsManager = "Org.BookingsManager";

        public const string ApproverL1OrAbove = "Org.ApproverL1OrAbove";
        public const string ApproverL2OrAbove = "Org.ApproverL2OrAbove";
        public const string ApproverL3OrAbove = "Org.ApproverL3OrAbove";

        public const string ReportsViewer = "Org.ReportsViewer";
        public const string DataExporter = "Org.DataExporter";
    }
}