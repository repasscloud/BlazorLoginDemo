# 🔐 Authorization Policies & Roles Reference

This document defines all **authorization policies** and the **roles** that can satisfy them.  
Policies are grouped by domain for clarity.

---

## 👥 User Access & Membership

| **Policy Name**          | **Allowed Roles**                               | **Purpose / Definition** |
|---------------------------|------------------------------------------------|---------------------------|
| **RequireMemberOrAbove** | `Member`, `Manager`, `Admin`                     | Ensures only authenticated users with **Member+** status can access. |
| **ManagersOnly**          | `Manager`, `Admin`                              | Restricts features to managerial or administrative users. |
| **AdminsOnly**            | `Admin`                                         | Highest level of “general admin” users. |
| **CanManageUsers**        | `SuperAdmin`, `OrgAdmin`, `UserAdmin`           | Full control of user lifecycle (create, update, disable, assign roles). |
| **CanEnableDisableUser**  | `SuperAdmin`, `OrgAdmin`, `UserAdmin`           | Explicit permission to enable/disable user accounts. |
| **CanManageGroups**       | `SuperAdmin`, `SupportAdmin`                    | Create, edit, or delete user groups and memberships. |

---

## 📜 Policy Management

| **Policy Name**     | **Allowed Roles**                         | **Purpose / Definition** |
|----------------------|------------------------------------------|---------------------------|
| **CanEditPolicies** | `SuperAdmin`, `OrgAdmin`, `PolicyAdmin`   | Allows editing or redefining security policies and permissions. |

---

## 💰 Financial Access

| **Policy Name**        | **Allowed Roles**                                                                      | **Purpose / Definition** |
|-------------------------|---------------------------------------------------------------------------------------|---------------------------|
| **CanEditFinancials**  | `SuperAdmin`, `OrgAdmin`, `FinanceAdmin`, `FinanceEditor`                             | Modify financial data (transactions, reporting adjustments, etc.). |
| **FinanceRead**        | `SuperAdmin`, `OrgAdmin`, `FinanceAdmin`, `FinanceEditor`, `FinanceViewer`, `SupportFinance` | Grants **read-only** or higher access to financial records. |

---

## 🛠 Support Area

| **Policy Name** | **Allowed Roles**                                                | **Purpose / Definition** |
|------------------|-----------------------------------------------------------------|---------------------------|
| **SupportArea** | `SupportViewer`, `SupportAgent`, `SupportFinance`, `SupportAdmin` | Access to support dashboards, tickets, and customer tools. |

---

## ✅ Approval Workflows

| **Policy Name**        | **Allowed Roles**                                             | **Purpose / Definition** |
|-------------------------|--------------------------------------------------------------|---------------------------|
| **ApproverL1OrAbove**  | `ApproverL1`, `ApproverL2`, `ApproverL3`, `OrgAdmin`, `SuperAdmin` | Approval workflows requiring **Level 1 approver or higher**. |
| **ApproverL2OrAbove**  | `ApproverL2`, `ApproverL3`, `OrgAdmin`, `SuperAdmin`          | Approval workflows requiring **Level 2 approver or higher**. |
| **ApproverL3OrAbove**  | `ApproverL3`, `OrgAdmin`, `SuperAdmin`                        | Approval workflows requiring **Level 3 approver or higher**. |

---

# 👥 Roles Reference

| **Role**          | **Definition** |
|--------------------|----------------|
| **Member**         | Standard authenticated user with baseline access. |
| **Manager**        | Elevated user with authority over a subset of users, reports, or features. |
| **Admin**          | Full administrative access to platform-level management. |
| **SuperAdmin**     | **Highest authority**. Unrestricted access across the system. |
| **OrgAdmin**       | Organization-level admin. Manages users, policies, and financials within their org. |
| **UserAdmin**      | Specialized admin for managing **user accounts only**. |
| **PolicyAdmin**    | Can manage and edit system policies (security/authorization). |
| **FinanceAdmin**   | Full control of financial data, reporting, and reconciliation. |
| **FinanceEditor**  | Can edit financial records but with limited scope compared to FinanceAdmin. |
| **FinanceViewer**  | **Read-only** access to financial records. |
| **SupportViewer**  | View-only access to support cases and dashboards. |
| **SupportAgent**   | Handles customer support requests (ticket resolution). |
| **SupportFinance** | Support role with access to financial-related support cases. |
| **SupportAdmin**   | Manages all support operations and configurations. |
| **ApproverL1**     | Level 1 approval authority in workflows. |
| **ApproverL2**     | Level 2 approval authority (higher than L1). |
| **ApproverL3**     | Level 3 approval authority (highest approver role). |

---
