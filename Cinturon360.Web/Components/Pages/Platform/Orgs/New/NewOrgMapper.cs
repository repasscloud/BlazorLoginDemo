using Cinturon360.Shared.Contracts.Organizations;
using Cinturon360.Web.Drafts.Platform.Org;

namespace Cinturon360.Web.Drafts.Platform.Orgs.New;

internal static class NewOrgMapper
{
    public static CreateOrganizationRequest ToRequest(
        NewOrgDraft draft)
    {
        if (draft is null)
        {
            throw new ArgumentNullException(nameof(draft));
        }

        return new CreateOrganizationRequest
        {
            OrganizationName = draft.OrgName,
            OrganizationType = draft.OrgType,
            ParentOrganizationId = draft.ParentOrgId,
            IsActive = draft.IsActive,
            DefaultCurrencyCode = draft.DefaultCurrencyCode,
            OrganizationClassification = draft.OrgClass,
            OrganizationSector = draft.OrgSector,
            SlaTier = draft.ServiceLevelAgreementTier,
            TimeZoneId = draft.TimeZoneId,
            TaxationType = draft.TaxRegistrationType,
            TaxId = draft.TaxRegistrationNumber,
            AddressLine1 = draft.AddressLine1,
            AddressLine2 = draft.AddressLine2,
            City = draft.City,
            State = draft.State,
            PostalCode = draft.PostalCode,
            Country = draft.Country,
            MailingAddressLine1 = draft.MailingAddressLine1,
            MailingAddressLine2 = draft.MailingAddressLine2,
            MailingCity = draft.MailingCity,
            MailingState = draft.MailingState,
            MailingPostalCode = draft.MailingPostalCode,
            MailingCountry = draft.MailingCountry,
            ContactPersonFirstName = draft.ContactPersonFirstName,
            ContactPersonLastName = draft.ContactPersonLastName,
            ContactPersonCountryCode = draft.ContactPersonCountryCode,
            ContactPersonPhoneNumber = draft.ContactPersonPhone,
            ContactPersonPhoneNumberIsMobile = draft.ContactPersonPhoneReceiveSMS,
            ContactPersonPhoneNumberIsWhatsApp = draft.ContactPersonPhoneReceiveWhatsApp,
            ContactPersonEmail = draft.ContactPersonEmail,
            ContactPersonJobTitle = draft.ContactPersonJobTitle,
            BillingPersonFirstName = draft.BillingPersonFirstName,
            BillingPersonLastName = draft.BillingPersonLastName,
            BillingPersonCountryCode = draft.BillingPersonCountryCode,
            BillingPersonPhoneNumber = draft.BillingPersonPhone,
            BillingPersonPhoneNumberIsMobile = draft.BillingPersonPhoneReceiveSMS,
            BillingPersonPhoneNumberIsWhatsApp = draft.BillingPersonPhoneReceiveWhatsApp,
            BillingPersonEmail = draft.BillingPersonEmail,
            BillingPersonJobTitle = draft.BillingPersonJobTitle,
            AdminPersonFirstName = draft.AdminPersonFirstName,
            AdminPersonLastName = draft.AdminPersonLastName,
            AdminPersonCountryCode = draft.AdminPersonCountryCode,
            AdminPersonPhoneNumber = draft.AdminPersonPhone,
            AdminPersonPhoneNumberIsMobile = draft.AdminPersonPhoneReceiveSMS,
            AdminPersonPhoneNumberIsWhatsApp = draft.AdminPersonPhoneReceiveWhatsApp,
            AdminPersonEmail = draft.AdminPersonEmail,
            AdminPersonJobTitle = draft.AdminPersonJobTitle
        };        
    }
}
