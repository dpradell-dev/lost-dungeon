using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nethereum.Web3;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Evm;
using WalletConnectUnity.Modal;
using WalletConnectUnity.Modal.Sample;

public class WalletConnectController : MonoBehaviour
{
    public event UnityAction<SessionStruct?> OnConnected;
    public event UnityAction<string> OnConnectionError;
    public event UnityAction OnDisconnected;
    
    #region UNITY_LIFECYCLE

    private void Start()
    {
        SubscribeToEvents();
    }
    
    private async void SubscribeToEvents()
    {
        WalletConnectModal.Ready += (sender, args) =>
        {
            if (args.SessionResumed)
            {
                // Session has been resumed, proceed to the game
                Debug.Log("Session resumed.");
            }
            else
            {
                // WalletConnectModal events. This happens before the wallet is connected.
                WalletConnectModal.ConnectionError += ConnectionError_Handler;

                // WalletConnect.Instance events. This happens when the wallet is connected.
                // Invoked after wallet connected
                //WalletConnect.Instance.SessionConnected += OnSessionConnected_Handler;
                // Invoked after wallet disconnected
                WalletConnect.Instance.SessionDisconnected += OnSessionDisconnected_Handler;
            
                // We don't do anything here but we want to have it for logs.
                WalletConnect.Instance.ActiveSessionChanged += (s, @struct) =>
                {
                    if (string.IsNullOrEmpty(@struct.Topic))
                        return;
                    
                    Debug.Log($"[WalletConnectModalSample] Session connected. Topic: {@struct.Topic}");
                    OnSessionConnected_Handler(s, @struct);
                };
            }
        };
    }

    private void OnDisable()
    {
        // No need to unsubscribe as this class persists throughout whole game.
        /*
        WalletConnectModal.ConnectionError -= ConnectionError_Handler;
        WalletConnect.Instance.SessionConnected -= OnSessionConnected_Handler;
        WalletConnect.Instance.SessionDisconnected -= OnSessionDisconnected_Handler;
        */
    }
    #endregion
    
    public async void Connect()
    {
        try
        {
            if (WalletConnect.Instance.SignClient.PendingSessionRequests.Length == 0)
            {
                // Normal connection.
                Debug.Log("No pending session requests. Connecting...");
                var dappConnectOptions = new WalletConnectModalOptions
                {
                    ConnectOptions = BuildConnectOptions()
                };

                WalletConnectModal.Open(dappConnectOptions);
            }
            else
            {
                Debug.Log("SignClient has some pending requests");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Debug.LogWarning("BUG: Pending session requests error inside SignClient. Disposing requests...");

            OnConnectionError?.Invoke("Known bug error.");
        }
    }
    
    public async void Disconnect()
    {
        await WalletConnect.Instance.DisconnectAsync();
    }
    
    public async UniTask<string> Sign(string message, string address)
    {
        var result = await PersonalSignAsync(message, address);
        return result;
    }
    
    public async UniTask<string> AcceptAccountOwnership(string contractAddress, string newOwnerAddress)
    {
        try
        {
            // Contract details
            string contractABI = "[{\"inputs\":[],\"name\":\"acceptOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";

            // Initialize Nethereum
            var web3 = new Web3();
            var contract = web3.Eth.GetContract(contractABI, contractAddress);

            // Get the function from the contract
            var acceptOwnershipFunction = contract.GetFunction("acceptOwnership");
            var encodedData = acceptOwnershipFunction.GetData();
            
            var desiredChainId = GameConstants.GameChainId; // BEAM network chain ID
            
            // Get ActiveSession namespace
            //var currentNamespace = WalletConnect.Instance.ActiveSession.Namespaces.FirstOrDefault();
            var currentChainId = GetChainId(); // Implement this method to get the current chain ID
            var currentFullChainId = ChainConstants.Namespaces.Evm + ":" + currentChainId;
            
            if (currentChainId != desiredChainId)
            {
                Debug.LogWarning($"Wrong network. Please switch your wallet to the correct network. Chain ID should be {desiredChainId}");
                var switched = await SwitchToBeamNetwork(currentFullChainId);

                if (!switched)
                {
                    Debug.LogError("Failed switching to BEAM network.");
                    var added = await AddBeamNetwork(currentFullChainId);

                    if (!added)
                    {
                        var message = "Failed to switch to BEAM network and add BEAM network";
                        Debug.Log(message);
                        return null;
                    }
                    // This means we added BEAM correctly, we can go ahead.
                }
                // This means we switched to BEAM correctly, we can go ahead.
            }

            // Prepare the transaction
            var txParams = new Transaction()
            {
                from = newOwnerAddress,
                to = contractAddress,
                data = encodedData,
                value = "0x0",
                gas = "0xFDE8", // Hex value for 65,000 gas limit
            };

            var ethSendTransaction = new EthSendTransaction(txParams);
            // The fullChainId might need to be adjusted based on the network specifics
            var fullChainId = ChainConstants.Namespaces.Evm + ":" + desiredChainId; // BEAM!
            
            await UniTask.Delay(1500);
            
            /* TODO not using it
            // Let's update the session with Beam network
            var updated = await AddBeamNetworkToSession();
            if (!updated) return null;
            */
            
            // Send the transaction
            //var signClient = WalletConnect.Instance.SignClient;
            var txHash = await WalletConnect.Instance.RequestAsync<EthSendTransaction, string>(ethSendTransaction);
            
            // Handle the transaction hash (e.g., display it, log it, etc.)
            Debug.Log("Transaction Hash: " + txHash);
            return txHash;

        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred: {e.Message}");
            return null; // Or handle the failure case appropriately
        }
    }
    
    public UniTask<string> GetConnectedAddressAsync()
    {
        return UniTask.Run(GetConnectedAddress);
    }
    
    
    public UniTask<int?> GetChainIdAsync()
    {
        return UniTask.Run(GetChainId);
    }

    #region EVENT_HANDLERS
    private void OnSessionConnected_Handler(object sender, SessionStruct? session)
    {
        Debug.Log("WC SESSION CONNECTED");
        OnConnected?.Invoke(session);
    }
    
    private void OnSessionDisconnected_Handler(object sender, EventArgs eventArgs)
    {
        Debug.LogWarning("WC SESSION DISCONNECTED");
        OnDisconnected?.Invoke();
    }
    
    private void ConnectionError_Handler(object sender, EventArgs eventArgs)
    {
        Debug.Log("WC SESSION CONNECTION ERROR");
        // No need for real disconnection as we're not connected yet.
        OnConnectionError?.Invoke($"Connection error reason: {eventArgs}");
    }
    #endregion

    #region PRIVATE_METHODS
    private async UniTask<string> PersonalSignAsync(string message, string address)
    {
        var data = new PersonalSign(message, address);

        try
        {
            var result = await WalletConnect.Instance.RequestAsync<PersonalSign, string>(data);
            return result;
        }
        catch (WalletConnectException e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    private string GetConnectedAddress()
    {
        var currentAddress = WalletConnect.Instance.ActiveSession.CurrentAddress(ChainConstants.Namespaces.Evm);
        return currentAddress.Address;
    }
    
    private int? GetChainId()
    {
        var defaultChain = WalletConnect.Instance.ActiveSession.Namespaces.Keys.FirstOrDefault();
    
        if (string.IsNullOrWhiteSpace(defaultChain))
            return null;

        var defaultNamespace = WalletConnect.Instance.ActiveSession.Namespaces[defaultChain];
    
        if (defaultNamespace.Chains.Length == 0)
            return null;

        // Assuming we need the last chain if there are multiple chains
        var fullChain = defaultNamespace.Chains.LastOrDefault();

        if (string.IsNullOrWhiteSpace(fullChain))
            return null;

        var chainParts = fullChain.Split(':');

        // Check if the split operation gives at least 2 parts
        if (chainParts.Length < 2)
            return null;

        if (int.TryParse(chainParts[1], out int chainId))
        {
            return chainId;
        }

        return null;
    }
    
    private async UniTask<bool> AddBeamNetwork(string currentFullChainId)
    {
        try
        {

            var beamChain = new EthereumChain()
            {
                chainIdHex = GameConstants.GameChainIdHex,
                name = "Beam",
                rpcUrls = new [] {"https://build.onbeam.com/rpc"},
                nativeCurrency = new Currency("Beam", "BEAM", 18),
                blockExplorerUrls = new [] {"https://subnets.avax.network/beam"}
            };
            
            var addChainRequest = new WalletAddEthereumChain(beamChain);
            
            var signClient = WalletConnect.Instance.SignClient;
            // Request to switch the Ethereum chain
            //TODO big change
            var result = await WalletConnect.Instance.RequestAsync<WalletAddEthereumChain, object>(addChainRequest);

            // Interpret a null response as successful operation
            // https://docs.metamask.io/wallet/reference/wallet_addethereumchain/
            return result == null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding Ethereum chain: {e.Message}");
            return false;
        }
    }
    
    private async UniTask<bool> SwitchToBeamNetwork(string currentFullChainId)
    {
        try
        {
            var switchChainRequest = new WalletSwitchEthereumChain(GameConstants.GameChainId.ToString());
            
            var signClient = WalletConnect.Instance.SignClient;
            // Request to switch the Ethereum chain
            //TODO big change!
            var result = await WalletConnect.Instance.RequestAsync<WalletSwitchEthereumChain, object>(switchChainRequest);

            // Interpret a null response as successful operation
            // https://docs.metamask.io/wallet/reference/wallet_switchethereumchain/
            return result == null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error switching Ethereum chain: {e.Message}");
            return false;
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private ConnectOptions BuildConnectOptions()
    {
        // Using optional namespaces. Wallet will approve only chains it supports.
        var optionalNamespaces = new Dictionary<string, ProposedNamespace>();
        
        var methods = new string[]
        {
            "eth_sendTransaction",
            "eth_signTransaction",
            "eth_sign",
            "personal_sign",
            "eth_signTypedData",
            "wallet_switchEthereumChain",
            "wallet_addEthereumChain"
        };

        var events = new string[]
        {
            "chainChanged", "accountsChanged"
        };
        
        optionalNamespaces.Add(ChainConstants.Namespaces.Evm, new ProposedNamespace()
        {
            Chains = new []{"eip155:4337"},
            Events = events,
            Methods = methods
        });

        return new ConnectOptions
        {
            OptionalNamespaces = optionalNamespaces
        };
    }
    #endregion
}
