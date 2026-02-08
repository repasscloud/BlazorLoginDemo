using Cinturon360.Shared.Models.Kernel.Travel;

namespace Cinturon360.Shared.Helpers;

public static class IDGeneratorHelper
{
    // avoids 0,1,2,5,8,B,I,L,O,S,Z for better readability
    public static readonly string Alphabet = "34679ACDEFGHJKMNPQRTUVWXY";

    private sealed record IdSpec(
        string? FixedPrefix,
        int Length,
        bool RequiresExternalPrefix = false
    );

    private static readonly IReadOnlyDictionary<IdGenType, IdSpec> Specs =
        new Dictionary<IdGenType, IdSpec>
        {
            // Core / platform
            { IdGenType.Default,       new IdSpec("xxx_", 10) },

            { IdGenType.Organization, new IdSpec("org_", 21) },
            { IdGenType.Domain,       new IdSpec("dom_", 18) },
            { IdGenType.License,      new IdSpec("lic_", 14) },
            { IdGenType.Job,          new IdSpec("job_", 16) },
            { IdGenType.Log,          new IdSpec("log_", 18) },
            { IdGenType.Error,        new IdSpec("err_", 10) },

            // Financial / policy
            { IdGenType.ExchangeRate,    new IdSpec("fxs_", 16) },
            { IdGenType.ExpensePolicy,   new IdSpec("epol_", 18) },
            { IdGenType.TravelPolicy,    new IdSpec("tpol_", 18) },
            { IdGenType.EphemeralPolicy, new IdSpec("etp_", 14) },
            { IdGenType.Discount,        new IdSpec("dsc_", 12) },
            { IdGenType.LateFee,         new IdSpec("lfe_", 12) },

            // Travel artefacts
            { IdGenType.FlightView,      new IdSpec("fvo_", 14) },
            { IdGenType.FlightSearchReq, new IdSpec("fosr_", 20) },
            { IdGenType.FlightSearch,    new IdSpec("fos_", 20) },
            { IdGenType.TravelQuoteUser, new IdSpec("tqu_", 16) },
            { IdGenType.UserSysPref,     new IdSpec("usp_", 16) },
            { IdGenType.RailOperator,    new IdSpec("rop_", 16) },

            // Reference data
            { IdGenType.LoyaltyAccount, new IdSpec("loy_", 16) },
            { IdGenType.Dto,            new IdSpec("dto_", 18) },
            { IdGenType.Tmc,            new IdSpec("tmc_", 12) },
            { IdGenType.Vendor,         new IdSpec("ven_", 12) },
            { IdGenType.User,           new IdSpec("usr_", 16) },
            { IdGenType.GitHubOAuth,    new IdSpec("gho_", 24) },

            // Master data
            { IdGenType.AircraftMaker, new IdSpec("acm_", 12) },
            { IdGenType.Employee,      new IdSpec("emp_", 12) },
            { IdGenType.Sales,         new IdSpec("sal_", 14) },
            { IdGenType.Storage,       new IdSpec("sto_", 20) },
            { IdGenType.Airline,       new IdSpec("air_", 8) },
            { IdGenType.Airport,       new IdSpec("apt_", 6) },

            // Quote (special)
            { IdGenType.Quote, new IdSpec(null, 12, RequiresExternalPrefix: true) },

            // Discount Code (special)
            { IdGenType.DiscountCode, new IdSpec($"disc_{DateTime.UtcNow:yyyyMMdd}_", 8) },
            
            // Employee Private Key (special)
            { IdGenType.EmployeePrivateKey, new IdSpec("emp_pk_", 16) }
        };

    public static string GenerateId(
        IdGenType type = IdGenType.Default,
        string? prefix = null)
    {
        if (!Specs.TryGetValue(type, out var spec))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported IdGenType.");

        if (spec.RequiresExternalPrefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException(
                    "Prefix is required for this IdGenType.",
                    nameof(prefix));

            return $"{prefix}{NanoidDotNet.Nanoid.Generate(Alphabet, spec.Length)}";
        }

        var id = NanoidDotNet.Nanoid.Generate(Alphabet, spec.Length);
        return spec.FixedPrefix is null
            ? id
            : $"{spec.FixedPrefix}{id}";
    }
}


public enum IdGenType : short
{
    // Generic / fallback
    Default,

    // Core identity
    Organization,          // org_
    Domain,                // dom_
    License,               // lic_
    User,                  // usr_
    Employee,              // emp_

    // Commercial / tenancy
    Vendor,                // ven_
    Tmc,                   // tmc_
    Sales,                 // sal_
    Discount,              // dsc_
    LateFee,               // lfe_

    // Operational / system
    Job,                   // job_
    Log,                   // log_
    Error,                 // err_
    Storage,               // sto_
    GitHubOAuth,           // gho_

    // Financial / policy
    ExchangeRate,          // fxs_
    ExpensePolicy,         // epol_
    TravelPolicy,          // tpol_
    EphemeralPolicy,       // etp_

    // Travel domain
    FlightView,            // fvo_
    FlightSearchReq,       // fosr_
    FlightSearch,          // fos_
    TravelQuoteUser,       // tqu_
    LoyaltyAccount,        // loy_
    UserSysPref,           // usp_
    RailOperator,          // rop_

    // Reference / master data
    Dto,                   // dto_
    AircraftMaker,         // acm_
    Airline,               // air_
    Airport,               // apt_

    // Special (caller-supplied prefix)
    Quote,                 // flt_, acc_, txi_, trn_, car_, bus_, sim_, act_, mix_
    DiscountCode,          // disc_
    EmployeePrivateKey    // empkey_
}

public static class TravelQuoteId
{
    public const int TotalLength = 20;

    // Keep it short so prefix + nanoid == 20.
    // e.g. "tqf_" (4) + 16 chars = 20.
    private const int PrefixLength = 4;
    private const int SuffixLength = TotalLength - PrefixLength;

    public static string Generate(TravelQuoteType type)
    {
        var prefix = type switch
        {
            TravelQuoteType.flight        => "tqf_",
            TravelQuoteType.accomodation  => "tqa_",
            TravelQuoteType.taxi          => "tqt_",
            TravelQuoteType.train         => "tqr_",
            TravelQuoteType.hirecar       => "tqh_",
            TravelQuoteType.bus           => "tqb_",
            TravelQuoteType.simcard       => "tqs_",
            TravelQuoteType.activity      => "tqy_",
            TravelQuoteType.mixed         => "tqm_",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot generate Id for Unknown.")
        };

        // Nanoid default is 21 chars, so you MUST pass size here to stay under MaxLength(16).
        return prefix + NanoidDotNet.Nanoid.Generate(IDGeneratorHelper.Alphabet, size: SuffixLength);
    }
}
