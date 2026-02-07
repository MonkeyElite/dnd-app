using System.Security.Cryptography;

namespace DndApp.Identity.Security;

public sealed class InviteCodeGenerator : IInviteCodeGenerator
{
    private static readonly char[] Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public string Generate()
    {
        Span<char> rawChars = stackalloc char[12];
        for (var i = 0; i < rawChars.Length; i++)
        {
            rawChars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return string.Create(14, rawChars.ToArray(), static (buffer, state) =>
        {
            buffer[0] = state[0];
            buffer[1] = state[1];
            buffer[2] = state[2];
            buffer[3] = state[3];
            buffer[4] = '-';
            buffer[5] = state[4];
            buffer[6] = state[5];
            buffer[7] = state[6];
            buffer[8] = state[7];
            buffer[9] = '-';
            buffer[10] = state[8];
            buffer[11] = state[9];
            buffer[12] = state[10];
            buffer[13] = state[11];
        });
    }
}
