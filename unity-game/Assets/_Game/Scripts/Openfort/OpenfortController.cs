using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Openfort.OpenfortSDK.Model;
using Openfort.OpenfortSDK;

public class OpenfortController: MonoBehaviour, IAuthenticationService
{
    public static IAuthenticationService Instance { get; private set; }
    public event Action OnSdkInitialized;
    
    private OpenfortSDK openfortSDK;
    private string accessToken;

    [Header("Developer Keys")]
    [SerializeField] private string PublishableKey = "pk_test_b978024c-d0b0-5e28-8e37-1eb7b970dcdd";
    [SerializeField] private string ShieldApiKey = "b4216b9d-a69f-479f-82e8-1162072aa0bd";
    [SerializeField] private string ShieldEncryptionShare = "AlxvufDZf43HwjqA2pvhAmBBnNulJzmYsv6EjZuavcD9";
    
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
    
    private async void Start()
    {
        await InitializeSdkAsync();
    }

    public async UniTask InitializeSdkAsync()
    {
        try
        {
            Debug.Log("Initializing Openfort SDK...");
            openfortSDK = await OpenfortSDK.Init(PublishableKey, ShieldApiKey, ShieldEncryptionShare);

            Debug.Log("Openfort SDK initialized successfully");
            OnSdkInitialized?.Invoke();
        }
        catch (Exception e)
        {
            Debug.Log("Failed to initialize Openfort SDK: " + e.Message);
        }
    }

    public async UniTask AuthenticateWithThirdPartyProvider(string accessToken)
    {
        
        // Create third-party oauth request
        try
        {
            ThirdPartyOAuthRequest thirdPartyOAuthRequest = new ThirdPartyOAuthRequest(ThirdPartyOAuthProvider.Playfab, accessToken, TokenType.IdToken);
            Debug.Log("Authenticating...");
            await openfortSDK.AuthenticateWithThirdPartyProvider(thirdPartyOAuthRequest);
            Debug.Log("Authenticated successfully");
        }
        catch (Exception e)
        {
            Debug.Log("Failed to log in: " + e.Message);
        }

        await UniTask.Delay(1000);
        await ConfigureEmbeddedSigner();
    }

    private async UniTask ConfigureEmbeddedSigner()
    {
        try 
        {
            Debug.Log("Getting access token...");
            accessToken = await openfortSDK.GetAccessToken();
            Debug.Log("Access token retrieved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.Log("Failed to get access token: " + e.Message);
            throw;
        }

        try
        { 
            int chainId = 11155111; // Soneium Minato chain ID

            ShieldAuthentication shieldAuth = new ShieldAuthentication(ShieldAuthType.Openfort, accessToken);
            EmbeddedSignerRequest signerRequest = new EmbeddedSignerRequest(chainId, shieldAuth);

            Debug.Log("Configuring embedded signer...");
            await openfortSDK.ConfigureEmbeddedSigner(signerRequest);
            Debug.Log("Embedded signer configured successfully");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.Log("Failed to configure embedded signer: " + e.Message);
            throw;
        }
    }
}