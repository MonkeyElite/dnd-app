namespace DndApp.Identity.Security;

public interface IInviteCodeHasher
{
    string Normalize(string inviteCode);

    string Hash(string inviteCode);
}
