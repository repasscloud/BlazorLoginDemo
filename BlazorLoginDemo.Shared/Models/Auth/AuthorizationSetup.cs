// using Microsoft.AspNetCore.Authorization;
// using Microsoft.Extensions.DependencyInjection;
// using static BlazorLoginDemo.Shared.Auth.AppRoles;
// using static BlazorLoginDemo.Shared.Auth.AppPolicies;

// namespace BlazorLoginDemo.Shared.Auth;

// public static class AuthorizationSetup
// {
//     public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
//     {
//         services.AddHttpContextAccessor();
//         services.AddSingleton<IAuthorizationHandler, OrgRoleHandler>();

//         services.AddAuthorizationCore(options =>
//         {
//             // ── GLOBAL POLICIES ──────────────────────────────────────────────────────
//             options.AddPolicy(GlobalPolicy.AdminsOnly,
//                 p => p.RequireRole(GlobalRole.SuperAdmin, GlobalRole.UserAdmin, GlobalRole.PolicyAdmin, GlobalRole.FinanceAdmin));

//             // Keep your legacy “member/manager/admin” if still used:
//             options.AddPolicy(GlobalPolicy.RequireMemberOrAbove,
//                 p => p.RequireRole("Member", "Manager", "Admin"));

//             options.AddPolicy(GlobalPolicy.ManagersOnly,
//                 p => p.RequireRole("Manager", "Admin"));

//             options.AddPolicy(GlobalPolicy.CanManageUsers,
//                 p => p.RequireRole(GlobalRole.SuperAdmin, GlobalRole.UserAdmin));

//             options.AddPolicy(GlobalPolicy.CanEditPolicies,
//                 p => p.RequireRole(GlobalRole.SuperAdmin, GlobalRole.PolicyAdmin));

//             options.AddPolicy(GlobalPolicy.CanEditFinancials,
//                 p => p.RequireRole(GlobalRole.SuperAdmin, GlobalRole.FinanceAdmin, GlobalRole.FinanceEditor));

//             options.AddPolicy(GlobalPolicy.FinanceRead,
//                 p => p.RequireRole(GlobalRole.SuperAdmin, GlobalRole.FinanceAdmin, GlobalRole.FinanceEditor, GlobalRole.FinanceViewer, GlobalRole.SupportFinance));

//             options.AddPolicy(GlobalPolicy.CanEnableDisableUser,
//                 p => p.RequireRole(GlobalRole.SuperAdmin, GlobalRole.UserAdmin));

//             options.AddPolicy(GlobalPolicy.SupportArea,
//                 p => p.RequireRole(GlobalRole.SupportViewer, GlobalRole.SupportAgent, GlobalRole.SupportFinance, GlobalRole.SupportAdmin, GlobalRole.SuperAdmin));

//             options.AddPolicy(GlobalPolicy.CanManageGroups,
//                 p => p.RequireRole(GlobalRole.SuperAdmin, GlobalRole.SupportAdmin));

//             // Sales / Licensing
//             options.AddPolicy(GlobalPolicy.SalesArea,
//                 p => p.RequireRole(GlobalRole.SalesRep, GlobalRole.SalesManager, GlobalRole.SalesAdmin, GlobalRole.SuperAdmin));

//             options.AddPolicy(GlobalPolicy.CanCreateCustomers,
//                 p => p.RequireRole(GlobalRole.SalesRep, GlobalRole.SalesManager, GlobalRole.SalesAdmin, GlobalRole.SuperAdmin));

//             options.AddPolicy(GlobalPolicy.LicenseRead,
//                 p => p.RequireRole(GlobalRole.SalesRep, GlobalRole.SalesManager, GlobalRole.SalesAdmin, GlobalRole.FinanceViewer, GlobalRole.SupportFinance, GlobalRole.SuperAdmin));

//             options.AddPolicy(GlobalPolicy.CanManageLicenses,
//                 p => p.RequireRole(GlobalRole.SalesManager, GlobalRole.SalesAdmin, GlobalRole.SuperAdmin));

//             options.AddPolicy(GlobalPolicy.CanAmendLicenses,
//                 p => p.RequireRole(GlobalRole.SalesManager, GlobalRole.SalesAdmin, GlobalRole.SuperAdmin));

//             options.AddPolicy(GlobalPolicy.CanApproveDiscounts,
//                 p => p.RequireRole(GlobalRole.SalesManager, GlobalRole.SalesAdmin, GlobalRole.SuperAdmin));

//             // ── ORG-SCOPED POLICIES (tenant matching via OrgRoleRequirement) ────────
//             options.AddPolicy(OrgPolicy.Admin,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.Admin)));

//             options.AddPolicy(OrgPolicy.UserAdmin,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.UserAdmin, OrgRole.Admin)));

//             options.AddPolicy(OrgPolicy.PolicyAdmin,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.PolicyAdmin, OrgRole.Admin)));

//             options.AddPolicy(OrgPolicy.FinanceAdmin,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.FinanceAdmin, OrgRole.Admin)));

//             options.AddPolicy(OrgPolicy.BookingsManager,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.BookingsManager, OrgRole.Admin)));

//             options.AddPolicy(OrgPolicy.ReportsViewer,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.ReportsViewer, OrgRole.Admin)));

//             options.AddPolicy(OrgPolicy.DataExporter,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.DataExporter, OrgRole.Admin)));

//             // Approvals ladder per org
//             options.AddPolicy(OrgPolicy.ApproverL1OrAbove,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.ApproverL1, OrgRole.ApproverL2, OrgRole.ApproverL3, OrgRole.Admin)));

//             options.AddPolicy(OrgPolicy.ApproverL2OrAbove,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.ApproverL2, OrgRole.ApproverL3, OrgRole.Admin)));

//             options.AddPolicy(OrgPolicy.ApproverL3OrAbove,
//                 p => p.Requirements.Add(new OrgRoleRequirement(OrgRole.ApproverL3, OrgRole.Admin)));
//         });

//         return services;
//     }
// }
