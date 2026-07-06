bool isCampaignWriteRole(String? role) {
  if (role == null) {
    return false;
  }

  switch (role.trim().toLowerCase()) {
    case 'owner':
    case 'admin':
    case 'treasurer':
      return true;
    default:
      return false;
  }
}

bool canManageCampaignSettings(String? role) {
  if (role == null) {
    return false;
  }

  switch (role.trim().toLowerCase()) {
    case 'owner':
    case 'admin':
      return true;
    default:
      return false;
  }
}

bool canManageInvites(String? role) => canManageCampaignSettings(role);
