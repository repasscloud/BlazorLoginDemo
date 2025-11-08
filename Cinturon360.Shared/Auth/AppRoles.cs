namespace Cinturon360.Shared.Auth;

public static class AppRoles
{
    public const string Sudo = "Sudo";

    public static class Platform
    {
        public const string SuperAdmin = "Platform.SuperAdmin";
        public const string SuperUser = "Platform.SuperUser";
        public const string Admin = "Platform.Admin";

        public const string UserAdmin = "Platform.UserAdmin";
        public const string OrgAdmin = "Platform.OrgAdmin";
        public const string PolicyAdmin = "Platform.PolicyAdmin";
        public const string SecurityAdmin = "Platform.SecurityAdmin";
        public const string IntegrationAdmin = "Platform.IntegrationAdmin";

        public static class Finance
        {
            public const string Admin = "Platform.Finance.Admin";
            public const string Editor = "Platform.Finance.Editor";
            public const string Viewer = "Platform.Finance.Viewer";
        }

        public static class Support
        {
            public const string Admin = "Platform.Support.Admin";
            public const string Agent = "Platform.Support.Agent";
            public const string Viewer = "Platform.Support.Viewer";
            public const string Finance = "Platform.Support.Finance";
        }

        public static class Sales
        {
            public const string Rep = "Platform.Sales.Rep";
            public const string Manager = "Platform.Sales.Manager";
            public const string Admin = "Platform.Sales.Admin";
        }

        public const string ReportsViewer = "Platform.ReportsViewer";
        public const string DataExporter = "Platform.DataExporter";
        public const string Auditor = "Platform.Auditor";
        public const string ReadOnly = "Platform.ReadOnly";
    }

    public static class Tmc
    {
        public const string Admin = "Tmc.Admin";
        public const string UserAdmin = "Tmc.UserAdmin";
        public const string PolicyAdmin = "Tmc.PolicyAdmin";
        public const string SecurityAdmin = "Tmc.SecurityAdmin";
        public const string IntegrationAdmin = "Tmc.IntegrationAdmin";

        public static class Finance
        {
            public const string Admin = "Tmc.Finance.Admin";
            public const string Editor = "Tmc.Finance.Editor";
            public const string Viewer = "Tmc.Finance.Viewer";
        }

        public const string BookingsManager = "Tmc.BookingsManager";
        public const string TravelAgent = "Tmc.TravelAgent";
        public const string ReportsViewer = "Tmc.ReportsViewer";
        public const string DataExporter = "Tmc.DataExporter";
        public const string Auditor = "Tmc.Auditor";
        public const string ReadOnly = "Tmc.ReadOnly";
    }

    public static class Client
    {
        public const string Admin = "Client.Admin";
        public const string UserAdmin = "Client.UserAdmin";
        public const string PolicyAdmin = "Client.PolicyAdmin";
        public const string SecurityAdmin = "Client.SecurityAdmin";
        public const string IntegrationAdmin = "Client.IntegrationAdmin";

        public static class Finance
        {
            public const string Admin = "Client.Finance.Admin";
            public const string Editor = "Client.Finance.Editor";
            public const string Viewer = "Client.Finance.Viewer";
        }

        public static class Approver
        {
            public const string L1 = "Client.Approver.L1";
            public const string L2 = "Client.Approver.L2";
            public const string L3 = "Client.Approver.L3";
        }

        public const string ReportsViewer = "Client.ReportsViewer";
        public const string DataExporter = "Client.DataExporter";
        public const string Auditor = "Client.Auditor";
        public const string ReadOnly = "Client.ReadOnly";
        public const string Requestor = "Client.Requestor";
    }
}
