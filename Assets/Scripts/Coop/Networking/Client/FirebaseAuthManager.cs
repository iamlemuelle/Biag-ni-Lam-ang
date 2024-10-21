using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.SceneManagement;
using TMPro;

public class FirebaseAuthManager : MonoBehaviour
{
    // Firebase variable
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    // UI Elements for Login
    [Space]
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public Button loginButton;

    // UI Elements for Registration
    [Space]
    [Header("Registration")]
    public TMP_InputField nameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;
    public Button signUpButton;

    // Connect Button for Anonymous Authentication
    [Space]
    [Header("Connection")]
    public Button connectButton;

    public const string PlayerNameKey = "PlayerName";  // Key for Player Name in PlayerPrefs

    private void Awake()
    {
        // Initialize Unity Authentication Services
        InitializeUnityAuthentication();

        // Check Firebase dependencies
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private async void InitializeUnityAuthentication()
    {
        bool initialized = await AuthenticationWrapper.InitializeAuthentication();
        if (!initialized)
        {
            Debug.LogError("Failed to initialize Unity Authentication.");
        }
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

        // Add listeners to buttons
        connectButton.onClick.AddListener(Connect);
        signUpButton.onClick.AddListener(Register);
        loginButton.onClick.AddListener(Login);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    // Connect as an anonymous user
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

    // Register with Firebase and Unity Authentication
    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (name == "")
        {
            Debug.LogError("User Name is empty");
            yield break;
        }
        if (email == "")
        {
            Debug.LogError("Email field is empty");
            yield break;
        }
        if (password != confirmPassword)
        {
            Debug.LogError("Password does not match");
            yield break;
        }

        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            HandleFirebaseError(registerTask.Exception, "Registration Failed");
        }
        else
        {
            user = registerTask.Result.User;
            UserProfile userProfile = new UserProfile { DisplayName = name };
            var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

            yield return new WaitUntil(() => updateProfileTask.IsCompleted);

            if (updateProfileTask.Exception != null)
            {
                user.DeleteAsync();
                HandleFirebaseError(updateProfileTask.Exception, "Profile Update Failed");
            }
            else
            {
                Debug.Log("Registration Successful. Welcome " + user.DisplayName);
                PlayerPrefs.SetString(PlayerNameKey, name);
                PlayerPrefs.Save(); // Ensure it is written to disk

                // Authenticate with Unity Services after successful registration
                AuthenticateWithUnityServices();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    // Login with Firebase and Unity Authentication
    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            HandleFirebaseError(loginTask.Exception, "Login Failed");
        }
        else
        {
            user = loginTask.Result.User; 
            Debug.LogFormat("{0} You Are Successfully Logged In", user.DisplayName);
            PlayerPrefs.SetString(PlayerNameKey, user.DisplayName);
            PlayerPrefs.Save(); // Ensure it is written to disk

            AuthenticateWithUnityServices();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Authenticate with Unity services after Firebase sign-in or sign-up
    private async void AuthenticateWithUnityServices()
    {
        await AuthenticationWrapper.PrepareAuthentication();
        if (AuthenticationWrapper.AuthState == AuthState.Authenticated)
        {
            Debug.Log("Successfully authenticated with Unity Services.");
        }
        else
        {
            Debug.LogError("Failed to authenticate with Unity Services.");
        }
    }

    // Handle Firebase authentication errors
    private void HandleFirebaseError(AggregateException exception, string errorPrefix)
    {
        FirebaseException firebaseException = exception.GetBaseException() as FirebaseException;
        AuthError authError = (AuthError)firebaseException.ErrorCode;
        string errorMessage = errorPrefix + ": ";

        switch (authError)
        {
            case AuthError.InvalidEmail:
                errorMessage += "Invalid Email";
                break;
            case AuthError.WrongPassword:
                errorMessage += "Wrong Password";
                break;
            case AuthError.MissingEmail:
                errorMessage += "Email is missing";
                break;
            case AuthError.MissingPassword:
                errorMessage += "Password is missing";
                break;
            default:
                errorMessage += "Unknown error occurred";
                break;
        }

        Debug.LogError(errorMessage);
    }
}
