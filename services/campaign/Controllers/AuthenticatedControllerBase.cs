using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Campaign.Controllers;

public abstract class AuthenticatedControllerBase : ControllerBase
{
    protected bool TryGetRequestingUserId(out Guid userId)
    {
        var subjectValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(subjectValue, out userId);
    }
}
