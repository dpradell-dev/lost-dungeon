using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class OpenfortController: MonoBehaviour
{
    public static OpenfortController Instance { get; private set; }
    
    // TODO private OpenfortSDK _openfort;
    // TODO private string _oauthAccessToken;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // TODO: NEW AUTHENTICATION FLOW
    public async UniTask AuthenticateWithOAuth(string idToken)
    {
        /*
        Debug.Log("PlayFab session ticket: " + idToken);

        var authOptions = new Shield.ShieldAuthOptions();
        
        try
        {
            _openfort = new OpenfortSDK(OFStaticData.PublishableKey);
            var authResponse = await _openfort.AuthenticateWithOAuth(OAuthProvider.Playfab, idToken, TokenType.IdToken);
            _oauthAccessToken = authResponse.Token;

            Debug.Log(_oauthAccessToken);

            authOptions = new Shield.OpenfortAuthOptions
            {
                authProvider = Shield.ShieldAuthProvider.Openfort,
                openfortOAuthToken = _oauthAccessToken
            };

            await _openfort.ConfigureEmbeddedSigner(4337, authOptions);
        }
        catch (MissingRecoveryPassword)
        {
            await _openfort.ConfigureEmbeddedSignerRecovery(4337, authOptions, "secret");
        }
        */
    }
}