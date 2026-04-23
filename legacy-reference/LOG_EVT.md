# Generic/base
UI_ACTION                     (CAT=Ui,           ACT=View/Click)
API_REQ_END                   (CAT=Api,          ACT=End)
INT_CALL_END                  (CAT=Integration,  ACT=Exec)
DATA_WRITE                    (CAT=Data,         ACT=Create/Update)
AUTO_JOB_END                  (CAT=Automation,   ACT=Exec)
WF_STEP_END                   (CAT=Workflow,     ACT=Step)
SEC_LOGIN_FAIL                (CAT=Auth,         ACT=Login)
SYS_HEALTH                    (CAT=Sys,          ACT=Exec)

# Travel Policy
TRAVEL_POLICY_CREATE          (CAT=Data,         ACT=Create)
TRAVEL_POLICY_NOT_FOUND       (CAT=Data,         ACT=Read)

# Flight offers / Amadeus
FLIGHT_OFFERS_REQ_START       (CAT=Api,          ACT=Start)
AMADEUS_FLIGHT_OFFERS_CALL    (CAT=Integration,  ACT=Exec)
FLIGHT_OFFERS_SAVE_RESULTS    (CAT=Storage,      ACT=Update)
AMADEUS_OAUTH_TOKEN_FAIL      (CAT=Integration,  ACT=Exec)
AMADEUS_API_ERROR             (CAT=Integration,  ACT=Exec)

# Discounts
DISCOUNT_PAGE_VIEW            (CAT=Ui,           ACT=View)
DISCOUNT_CREATE               (CAT=Data,         ACT=Create)
DISCOUNT_CREATE_OK            (CAT=Data,         ACT=Create)
DISCOUNT_CREATE_EXCEPTION     (CAT=Data,         ACT=Create)
DISCOUNT_CREATE_FAIL          (CAT=Data,         ACT=Create)
DISCOUNT_CODE_EXISTS          (CAT=Data,         ACT=Validate)

# Organization
ORG_CREATE                    (CAT=Data,         ACT=Create)
ORG_CREATE_END                (CAT=Data,         ACT=End)
ORG_READ_BY_ID                (CAT=Data,         ACT=Read)
ORG_TAX_ID_VALIDATE           (CAT=Tax,          ACT=Validate)
ORG_DEFAULT_POLICY_MISSING    (CAT=Data,         ACT=Validate)

# Superseded (kept for history; prefer the replacements above)
AMADEUS_FLIGHT_OFFERS_REQUEST → use FLIGHT_OFFERS_REQ_START
