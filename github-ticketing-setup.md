# GitHub Ticketing — Configuration Guide

> This document covers everything needed to configure the GitHub Issues integration
> used by the Cinturon360 platform for support ticket management.
> Keep this document updated as the integration evolves.

---

## 1. GitHub Repository Setup

### 1.1 Create a private repository

Create a **private** GitHub repository that will serve as the ticketing backend.
Suggested name: `cinturon360-support-tickets`

The repository does not need any code — it is used exclusively for Issues.

### 1.2 Create a GitHub Personal Access Token (Classic)

1. Go to **GitHub → Settings → Developer Settings → Personal Access Tokens → Tokens (classic)**.
2. Generate a new token with the following scopes:
   - `repo` (full control of private repositories — required for creating/reading issues)
3. Set a sensible expiry (e.g. 1 year). Rotate it before expiry.
4. Store the token in the platform's secret vault / environment config under:
   ```
   GitHub__TicketingPat
   ```
   Or if using user secrets:
   ```json
   { "GitHub": { "TicketingPat": "ghp_yourtoken" } }
   ```

### 1.3 Store the repository reference in app config

```json
{
  "GitHub": {
    "TicketingPat": "ghp_...",
    "TicketingOwner": "your-github-org-or-username",
    "TicketingRepo": "cinturon360-support-tickets"
  }
}
```

---

## 2. GitHub Issue Label Scheme

Labels are used to classify tickets and drive visibility logic inside the platform.
The platform will **auto-create missing labels** on first use. Alternatively, use the
shell script in §2.3 to pre-populate them all.

### 2.1 Category Labels (cat1 / cat2)

Each ticket gets one `cat1:*` label (primary category) and optionally one `cat2:*` label
(sub-category). The `cat2` labels are sub-categories of `cat1`.

| cat1 | cat2 options |
|------|-------------|
| `cat1:booking` | `cat2:flight`, `cat2:hotel`, `cat2:car`, `cat2:rail`, `cat2:cruise`, `cat2:tour`, `cat2:insurance`, `cat2:other` |
| `cat1:payment` | `cat2:invoice`, `cat2:topup`, `cat2:refund`, `cat2:dispute` |
| `cat1:account` | `cat2:login`, `cat2:mfa`, `cat2:profile`, `cat2:password`, `cat2:sso` |
| `cat1:policy` | `cat2:approval`, `cat2:rule`, `cat2:assignment` |
| `cat1:reporting` | `cat2:export`, `cat2:dashboard`, `cat2:data` |
| `cat1:technical` | `cat2:bug`, `cat2:performance`, `cat2:integration`, `cat2:api` |
| `cat1:admin` | `cat2:user-management`, `cat2:org-management`, `cat2:billing-config` |
| `cat1:general` | `cat2:feedback`, `cat2:feature-request`, `cat2:other` |

### 2.2 Context Labels

These drive routing, visibility, and assignment logic:

| Label | Purpose |
|-------|---------|
| `org-level:client` | Raised by a Client org user |
| `org-level:tmc` | Raised by a TMC org user |
| `org-level:vendor` | Raised by a Vendor org user |
| `org-level:gps-support` | Raised by or escalated to GPS Support |
| `queue:client-support` | In the Client support queue (default for client tickets) |
| `queue:tmc-support` | In the TMC support queue |
| `queue:vendor-support` | In the Vendor support queue |
| `queue:gps-support` | Escalated to GPS Support team |
| `escalated` | Ticket has been escalated at least once |
| `de-escalated` | Ticket was sent back to the level below |
| `visibility:public` | Notes on this ticket are visible to the raiser |
| `visibility:internal` | Notes are internal only (not shown to raiser) |
| `assigned-to:{username}` | The GitHub username this ticket is assigned to within the platform |

> **Note on visibility:** GitHub Issues does not natively support public/internal note visibility.
> The platform should use a naming convention for comments: prefix internal comments with
> `[INTERNAL]` and filter them out when showing the ticket to the raiser in the UI.

### 2.3 Shell Script — Pre-populate All Labels

Run this once against your ticketing repository after creating it. Set `OWNER`, `REPO`, and `TOKEN`.

```bash
#!/usr/bin/env bash
# populate-github-labels.sh
# Usage: OWNER=your-org REPO=cinturon360-support-tickets TOKEN=ghp_... bash populate-github-labels.sh

set -euo pipefail

OWNER="${OWNER:?Set OWNER}"
REPO="${REPO:?Set REPO}"
TOKEN="${TOKEN:?Set TOKEN}"
API="https://api.github.com/repos/${OWNER}/${REPO}/labels"

create_label() {
  local name="$1" color="$2" description="$3"
  curl -sf -X POST "${API}" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Accept: application/vnd.github+json" \
    -H "Content-Type: application/json" \
    -d "{\"name\":\"${name}\",\"color\":\"${color}\",\"description\":\"${description}\"}" \
    > /dev/null && echo "Created: ${name}" || echo "Skipped (exists?): ${name}"
}

# ── cat1 categories ──────────────────────────────────────────────────────────
create_label "cat1:booking"    "0075ca" "Primary category: Booking"
create_label "cat1:payment"    "e4e669" "Primary category: Payment"
create_label "cat1:account"    "d93f0b" "Primary category: Account"
create_label "cat1:policy"     "0e8a16" "Primary category: Policy"
create_label "cat1:reporting"  "5319e7" "Primary category: Reporting"
create_label "cat1:technical"  "b60205" "Primary category: Technical"
create_label "cat1:admin"      "1d76db" "Primary category: Admin"
create_label "cat1:general"    "c2e0c6" "Primary category: General"

# ── cat2 sub-categories ──────────────────────────────────────────────────────
create_label "cat2:flight"         "bfd4f2" "Booking sub-type: Flight"
create_label "cat2:hotel"          "bfd4f2" "Booking sub-type: Hotel"
create_label "cat2:car"            "bfd4f2" "Booking sub-type: Car"
create_label "cat2:rail"           "bfd4f2" "Booking sub-type: Rail"
create_label "cat2:cruise"         "bfd4f2" "Booking sub-type: Cruise"
create_label "cat2:tour"           "bfd4f2" "Booking sub-type: Tour"
create_label "cat2:insurance"      "bfd4f2" "Booking sub-type: Insurance"
create_label "cat2:invoice"        "fbca04" "Payment sub-type: Invoice"
create_label "cat2:topup"          "fbca04" "Payment sub-type: Top-up"
create_label "cat2:refund"         "fbca04" "Payment sub-type: Refund"
create_label "cat2:dispute"        "fbca04" "Payment sub-type: Dispute"
create_label "cat2:login"          "e99695" "Account sub-type: Login"
create_label "cat2:mfa"            "e99695" "Account sub-type: MFA"
create_label "cat2:profile"        "e99695" "Account sub-type: Profile"
create_label "cat2:password"       "e99695" "Account sub-type: Password"
create_label "cat2:sso"            "e99695" "Account sub-type: SSO"
create_label "cat2:approval"       "c5def5" "Policy sub-type: Approval"
create_label "cat2:rule"           "c5def5" "Policy sub-type: Rule"
create_label "cat2:assignment"     "c5def5" "Policy sub-type: Assignment"
create_label "cat2:export"         "d4c5f9" "Reporting sub-type: Export"
create_label "cat2:dashboard"      "d4c5f9" "Reporting sub-type: Dashboard"
create_label "cat2:data"           "d4c5f9" "Reporting sub-type: Data"
create_label "cat2:bug"            "ee0701" "Technical sub-type: Bug"
create_label "cat2:performance"    "ee0701" "Technical sub-type: Performance"
create_label "cat2:integration"    "ee0701" "Technical sub-type: Integration"
create_label "cat2:api"            "ee0701" "Technical sub-type: API"
create_label "cat2:user-management"  "1d76db" "Admin sub-type: User Management"
create_label "cat2:org-management"   "1d76db" "Admin sub-type: Org Management"
create_label "cat2:billing-config"   "1d76db" "Admin sub-type: Billing Config"
create_label "cat2:feedback"       "c2e0c6" "General: Feedback"
create_label "cat2:feature-request" "c2e0c6" "General: Feature Request"
create_label "cat2:other"          "ffffff" "Other / uncategorised"

# ── org-level labels ─────────────────────────────────────────────────────────
create_label "org-level:client"      "006b75" "Raised by a Client org user"
create_label "org-level:tmc"         "0052cc" "Raised by a TMC org user"
create_label "org-level:vendor"      "5319e7" "Raised by a Vendor org user"
create_label "org-level:gps-support" "b60205" "GPS Support internal"

# ── queue labels ─────────────────────────────────────────────────────────────
create_label "queue:client-support"  "006b75" "In the Client support queue"
create_label "queue:tmc-support"     "0052cc" "In the TMC support queue"
create_label "queue:vendor-support"  "5319e7" "In the Vendor support queue"
create_label "queue:gps-support"     "b60205" "In the GPS Support queue"

# ── escalation / visibility ──────────────────────────────────────────────────
create_label "escalated"           "e4e669" "Ticket has been escalated"
create_label "de-escalated"        "fef2c0" "Ticket was sent back down"
create_label "visibility:public"   "0e8a16" "Notes visible to ticket raiser"
create_label "visibility:internal" "b60205" "Internal notes only"

echo "Done."
```

---

## 3. Ticket Lifecycle & Queue Routing

### 3.1 Default queue on creation

| Raised by | Default queue label | Visible to |
|-----------|--------------------|-----------:|
| Client user | `queue:client-support` | Client user + TMC above |
| TMC user | `queue:tmc-support` | TMC user + Vendor above |
| Vendor user | `queue:vendor-support` | Vendor user + GPS Support |

> Whether a ticket goes to the **same org** queue or the **level above** immediately
> is controlled by the category's `DefaultEscalation` setting (configured per `cat1`
> in the app admin). You will configure this later via the platform admin UI.

### 3.2 Escalation

- Any user with the `Support` role in an org can escalate a ticket to the org above.
- Escalation changes the `queue:*` label and adds the `escalated` label.
- A **system comment** is added to the issue:
  ```
  [SYSTEM] Ticket escalated from {from_org} ({from_level}) to {to_org} ({to_level})
  by {user_display_name} on {datetime UTC}.
  ```

### 3.3 De-escalation (sending back)

- Any user with the `Support` role can send a ticket back down one level.
- The `queue:*` label is changed back to the level below and `de-escalated` is added.
- A **system comment** is added:
  ```
  [SYSTEM] Ticket de-escalated from {from_level} back to {to_level}
  by {user_display_name} on {datetime UTC}. Note: {optional_note}
  ```

### 3.4 Closing tickets

- Once a ticket is marked **Closed** on GitHub, the platform treats it as closed.
- The platform UI does **not** provide a re-open button — re-open must happen directly
  in the GitHub Issues UI by a Support user with GitHub repo access.

---

## 4. Ticket Body Format (Markdown/HTML via TinyMCE)

The platform's ticket creation form uses **TinyMCE (free tier)** as the editor.
The submitted HTML is stored in the GitHub issue body as-is (GitHub renders HTML in issues).
Alternatively, the platform can convert HTML → Markdown before submission using a
lightweight HTML-to-Markdown converter so the issue body renders cleanly in the GitHub UI.

**Recommended approach:** Convert to Markdown on submit.
Use the `HtmlToMarkdown` NuGet package or a similar lightweight converter.

### 4.1 Issue body template

```markdown
## Summary
{user-provided text}

## Details
{user-provided text}

---
**Raised by:** {display_name} ({user_id})
**Org:** {org_name} ({org_id}) — {org_level}
**Category:** {cat1} / {cat2}
**Created:** {datetime UTC}
```

### 4.2 File attachments

GitHub Issues does not support programmatic file attachment via the API for private repos
(only image uploads via the web UI are supported natively). The recommended approach:

1. Upload the attachment to the platform's S3/R2 storage bucket (scoped to the ticket).
2. Include the pre-signed URL or a platform-authenticated download link in the issue body.
3. Attachments are managed/expired via the platform, not GitHub.

---

## 5. Assignment via Labels

The `assigned-to:{github-username}` label is used to track assignment within the
platform UI. The GitHub "Assignee" field may also be set simultaneously for native
GitHub issue tracking.

**To change assignment:** remove the old `assigned-to:*` label, add the new one.
The platform should validate that the new assignee has a Support role in the relevant org.

---

## 6. GPS Support Permission

GPS Support is a **platform-level** special permission (not an org-level role).
It must be added to a user's account by:
- A superuser account, OR
- Another user who already holds the `GpsSupport` permission.

This permission will be implemented as a distinct `Permission` value in the platform:
`Permission.GpsSupport`

GPS Support users can see all tickets at all levels, including `queue:vendor-support`.

---

## 7. Platform Integration — App Config Checklist

| Config key | Description |
|------------|-------------|
| `GitHub:TicketingPat` | GitHub PAT with `repo` scope |
| `GitHub:TicketingOwner` | GitHub org or username owning the repo |
| `GitHub:TicketingRepo` | Repository name (private) |
| `GitHub:TicketingAutoCreateLabels` | `true` = auto-create labels if missing; `false` = fail loudly |

---

## 8. Open Questions / Future Work

- [ ] Decide: HTML → Markdown conversion in platform, or store raw HTML in issue body?
- [ ] Define `DefaultEscalation` setting per `cat1` in the admin UI (immediate escalation vs same-org first).
- [ ] TinyMCE free tier plugin set (confirm if attachment plugin is available for free).
- [ ] Webhook: do we need a GitHub → platform webhook for comment sync back to platform DB? (Useful if support staff comment directly in GitHub.)
- [ ] Rate limiting: GitHub API is 5000 req/hr for authenticated requests — monitor at scale.
- [ ] GPS Support permission implementation in the platform's permission system.
