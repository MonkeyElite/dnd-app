using System.Security.Cryptography;
using System.Text;
using DndApp.Identity.Options;
using Microsoft.Extensions.Options;

namespace DndApp.Identity.Security;

public sealed class InviteCodeHasher : IInviteCodeHasher
{
    private readonly byte[] _pepperBytes;

    public InviteCodeHasher(IOptions<AuthOptions> options)
    {
        var pepper = options.Value.InvitePepper;
        if (string.IsNullOrWhiteSpace(pepper))
        {
            throw new InvalidOperationException("Auth:InvitePepper must be configured.");
        }

        _pepperBytes = Encoding.UTF8.GetBytes(pepper);
    }

    public string Normalize(string inviteCode)
        => (inviteCode ?? string.Empty).Trim().ToUpperInvariant();

    public string Hash(string inviteCode)
    {
        var normalized = Normalize(inviteCode);
        using var hmac = new HMACSHA256(_pepperBytes);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hashBytes);
    }
}
