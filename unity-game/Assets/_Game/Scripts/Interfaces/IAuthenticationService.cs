using System;
using Cysharp.Threading.Tasks;

public interface IAuthenticationService
{
    event Action OnSdkInitialized;
    UniTask InitializeSdkAsync();
    UniTask AuthenticateWithThirdPartyProvider(string accessToken);
}
