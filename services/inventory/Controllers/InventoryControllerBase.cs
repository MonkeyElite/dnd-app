using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Inventory.Controllers;

public abstract class InventoryControllerBase : ControllerBase
{
    protected static decimal NormalizeQuantity(decimal quantity)
        => decimal.Round(quantity, 3, MidpointRounding.AwayFromZero);

    protected bool TryGetRequestingUserId(out Guid userId)
    {
        var subjectValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(subjectValue, out userId);
    }
}
