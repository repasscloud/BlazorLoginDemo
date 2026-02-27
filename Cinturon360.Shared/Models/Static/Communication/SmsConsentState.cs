namespace Cinturon360.Shared.Models.Static.Communication;
public enum SmsConsentState
{
    None = 0,  // default value indicating no consent status, or consent status is unknown/not provided
    Granted = 1,  // user granted consent and evidence of consent was received (e.g. Twilio delivery receipt indicating message was delivered)
    PendingEvidence = 2,  // e.g. awaiting Twilio delivery receipt or similar proof of consent for regulatory purposes or if TMC enabled
    Revoked = 3,  // user revoked consent, or evidence of consent was rejected/failed (e.g. Twilio delivery failure indicating number is invalid or carrier rejected)
    Expired = 4  // consent expired due to time (e.g. 1 year) or regulatory requirements, or TMC disabled
}