using DndApp.Identity.Data.Entities;

namespace DndApp.Identity.Security;

public interface IJwtTokenService
{
    string CreateAccessToken(User user);
}
