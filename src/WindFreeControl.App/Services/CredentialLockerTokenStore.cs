using WindFreeControl.Core.Abstractions;
using Windows.Security.Credentials;

namespace WindFreeControl.App.Services;

public sealed class CredentialLockerTokenStore : ITokenStore
{
    private const string Resource = "WindFreeControl";
    private const string UserName = "SmartThings";

    private readonly PasswordVault _vault = new();

    public string? Load()
    {
        var credential = Find();
        if (credential is null)
        {
            return null;
        }

        credential.RetrievePassword();
        return credential.Password;
    }

    public void Save(string token)
    {
        Clear();
        _vault.Add(new PasswordCredential(Resource, UserName, token));
    }

    public void Clear()
    {
        if (Find() is { } credential)
        {
            _vault.Remove(credential);
        }
    }

    private PasswordCredential? Find()
    {
        // FindAllByResource throws instead of returning an empty list when nothing is stored.
        try
        {
            return _vault.FindAllByResource(Resource).FirstOrDefault();
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            return null;
        }
    }
}
