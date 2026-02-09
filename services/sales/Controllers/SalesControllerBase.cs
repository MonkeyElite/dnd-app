using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Sales.Controllers;

public abstract class SalesControllerBase : ControllerBase
{
    protected static decimal NormalizeQuantity(decimal quantity)
        => decimal.Round(quantity, 3, MidpointRounding.AwayFromZero);

    protected static long NormalizeMinor(decimal amountMinor)
        => decimal.ToInt64(decimal.Round(amountMinor, 0, MidpointRounding.AwayFromZero));

    protected bool TryGetRequestingUserId(out Guid userId)
    {
        var subjectValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(subjectValue, out userId);
    }

    protected Guid GetCorrelationId()
    {
        var correlationHeaderValue = Request.Headers["X-Correlation-Id"].FirstOrDefault();
        return Guid.TryParse(correlationHeaderValue, out var correlationId)
            ? correlationId
            : Guid.NewGuid();
    }
}
