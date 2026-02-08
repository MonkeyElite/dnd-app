using DndShop.Contracts;

namespace DndApp.Identity.Authorization;

public static class RoleRules
{
    public static bool TryNormalizeRole(string? role, out string normalizedRole)
    {
        if (Enum.TryParse<Role>(role?.Trim(), ignoreCase: true, out var parsed))
        {
            normalizedRole = parsed.ToString();
            return true;
        }

        normalizedRole = string.Empty;
        return false;
    }

    public static bool CanManageCampaignMembers(string role)
        => role is nameof(Role.Owner) or nameof(Role.Admin);

    public static bool CanManageInvites(string role)
        => CanManageCampaignMembers(role);

    public static bool CanAssignInviteRole(string role)
        => role is nameof(Role.Member)
            or nameof(Role.Treasurer)
            or nameof(Role.ReadOnly)
            or nameof(Role.Admin);
}
