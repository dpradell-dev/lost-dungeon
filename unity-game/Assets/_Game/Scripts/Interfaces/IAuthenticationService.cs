using System;
using Cysharp.Threading.Tasks;

public interface IAuthenticationService
{
    event Action OnSdkInitialized;
    UniTask InitializeSdkAsync();
    void AuthenticateWithThirdPartyProvider(string accessToken);
}
