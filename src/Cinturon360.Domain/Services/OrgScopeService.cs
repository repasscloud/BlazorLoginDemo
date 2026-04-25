using Cinturon360.Domain.Entities.Organization;
using Cinturon360.Domain.Enums.Security;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Domain.Services;

/// <summary>
/// Evaluates whether a user's org-role assignment grants access to a target org,
/// based on the scope rule and the org hierarchy.
/// Rules:
///   - Self: user may only act within their own home org
///   - SelfAndDescendants: user may act within home org + any child org below it
///   - Access never flows upward (Client cannot see TMC data)
///   - Access never flows sideways (Client A cannot see Client B)
/// </summary>
public static class OrgScopeService
{
    /// <summary>
    /// Determines if a user with the given homeOrgId and scopeMode can access targetOrgId,
    /// given the flat list of orgs representing the known hierarchy.
    /// </summary>
    public static bool CanAccess(
        string homeOrgId,
        ScopeMode scopeMode,
        string targetOrgId,
        IReadOnlyList<Organisation> allOrgs)
    {
        if (homeOrgId == targetOrgId)
            return true;

        if (scopeMode == ScopeMode.Self)
            return false;

        // SelfAndDescendants: check if targetOrgId is a descendant of homeOrgId
        return IsDescendant(homeOrgId, targetOrgId, allOrgs);
    }

    /// <summary>
    /// Returns all org IDs accessible by a user with the given homeOrgId and scopeMode.
    /// </summary>
    public static IReadOnlyList<string> GetAccessibleOrgIds(
        string homeOrgId,
        ScopeMode scopeMode,
        IReadOnlyList<Organisation> allOrgs)
    {
        if (scopeMode == ScopeMode.Self)
            return [homeOrgId];

        var result = new List<string> { homeOrgId };
        CollectDescendants(homeOrgId, allOrgs, result);
        return result;
    }

    private static bool IsDescendant(
        string ancestorOrgId,
        string targetOrgId,
        IReadOnlyList<Organisation> allOrgs)
    {
        var target = allOrgs.FirstOrDefault(o => o.Id == targetOrgId);
        if (target?.ParentOrgId == null)
            return false;
        if (target.ParentOrgId == ancestorOrgId)
            return true;
        return IsDescendant(ancestorOrgId, target.ParentOrgId, allOrgs);
    }

    private static void CollectDescendants(
        string parentOrgId,
        IReadOnlyList<Organisation> allOrgs,
        List<string> result)
    {
        var children = allOrgs.Where(o => o.ParentOrgId == parentOrgId);
        foreach (var child in children)
        {
            result.Add(child.Id);
            CollectDescendants(child.Id, allOrgs, result);
        }
    }
}
