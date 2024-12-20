 
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    public const string PlayerNameKey = "PlayerName";  // Add this line
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button signUpButton;
    [SerializeField] private Button loginButton;

    private void Start()
    {
        Debug.Log("Start called");
        
        // Check if UI elements are assigned
        if (connectButton == null) Debug.LogError("connectButton is not assigned!");
        if (signUpButton == null) Debug.LogError("signUpButton is not assigned!");
        if (loginButton == null) Debug.LogError("loginButton is not assigned!");
        if (usernameField == null) Debug.LogError("usernameField is not assigned!");
        if (passwordField == null) Debug.LogError("passwordField is not assigned!");

        // Add listeners to the buttons
        connectButton.onClick.AddListener(Connect);
        signUpButton.onClick.AddListener(SignUp);
        loginButton.onClick.AddListener(Login);
    }


    // Anonymous connect
public async void Connect()
{
    bool initialized = await AuthenticationWrapper.InitializeAuthentication();
    if (initialized && AuthenticationWrapper.AuthState == AuthState.Authenticated)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    else
    {
        Debug.LogError("Anonymous authentication failed or services not initialized.");
    }
}



    // Sign up new user
   public async void SignUp()
{
    // Ensure services are initialized
    bool initialized = await AuthenticationWrapper.InitializeAuthentication();
    
    if (initialized && IsValidInput())
    {
        // Now you can safely call the sign-up method
        await AuthenticationWrapper.SignUpWithUsernamePasswordAsync(usernameField.text, passwordField.text);
        if (AuthenticationWrapper.AuthState == AuthState.Authenticated)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            Debug.LogError("Sign-up failed.");
        }
    }
    else
    {
        Debug.LogError("Authentication services not initialized or invalid input.");
    }
}


    // Login existing user
    public async void Login()
    {
        if (IsValidInput())
        {
            await AuthenticationWrapper.SignInWithUsernamePasswordAsync(usernameField.text, passwordField.text);
            if (AuthenticationWrapper.AuthState == AuthState.Authenticated)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                Debug.LogError("Login failed.");
            }
        }
    }

    // Validate username and password input
    private bool IsValidInput()
    {
        return usernameField.text.Length >= 3 && usernameField.text.Length <= 20 &&
               passwordField.text.Length >= 8 && passwordField.text.Length <= 30;
    }
}

