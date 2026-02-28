using static Cinturon360.Shared.Contracts.Organizations.OrganizationUnifiedDto;

namespace Cinturon360.Web.ViewModels.Orgs;

public static class ListOrgsMapper
{
    public static ListOrgsVm ToVm(
        ListOrganizationItemsAggregate aggregate,
        string rid)
    {
        var items =
            aggregate.OrganizationItems
                .Select(p => new OrgListItemVm
                {
                    Id = p.Id,
                    OrgName = p.OrgName,
                    IsActive = p.IsActive,
                    OrgType = p.OrgType,
                    OrgSector = p.OrganizationSector,
                    TaxRegistrationNumber = p.TaxRegistrationNumber ?? string.Empty,
                    AddressLine1 = p.AddressLine1 ?? string.Empty,
                    City = p.City ?? string.Empty,
                    MailingAddressLine1 = p.MailingAddressLine1 ?? string.Empty,
                    MailingCity = p.MailingCity ?? string.Empty,
                    ContactPersonLastName = p.ContactPersonLastName ?? string.Empty,
                    ContactPersonPhone = p.ContactPersonPhone ?? string.Empty,
                    ContactPersonEmail = p.ContactPersonEmail ?? string.Empty,
                    BillingPersonLastName = p.BillingPersonLastName ?? string.Empty,
                    BillingPersonPhone = p.BillingPersonPhone ?? string.Empty,
                    BillingPersonEmail = p.BillingPersonEmail ?? string.Empty,
                    AdminPersonLastName = p.AdminPersonLastName ?? string.Empty,
                    AdminPersonPhone = p.AdminPersonPhone ?? string.Empty,
                    AdminPersonEmail = p.AdminPersonEmail ?? string.Empty
                })
                .ToList();

        return ListOrgsVm.Ready(rid, items);
    }
}