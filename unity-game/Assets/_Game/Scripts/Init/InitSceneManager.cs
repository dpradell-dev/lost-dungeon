using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitSceneManager : MonoBehaviour
{
    private IAuthenticationService authenticationService;

    private void Start()
    {
        authenticationService = OpenfortController.Instance;
        if (authenticationService != null)
        {
            authenticationService.OnSdkInitialized += LoadLoginScene;
        }
    }

    private void OnDestroy() {
        if (authenticationService != null)
        {
            authenticationService.OnSdkInitialized -= LoadLoginScene;
        }
    }

    private void LoadLoginScene()
    {
        SceneManager.LoadScene("Login");
    }
}
