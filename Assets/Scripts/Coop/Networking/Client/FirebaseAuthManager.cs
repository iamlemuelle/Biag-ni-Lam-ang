using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.SceneManagement;
using TMPro;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    private DatabaseReference databaseReference; // Reference to the database

    [Space]
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public Button loginButton;
    public TMP_Text loginFeedbackText;

    [Space]
    [Header("Registration")]
    public TMP_InputField nameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;
    public Button signUpButton;
    public TMP_Text registrationFeedbackText;

    private void Awake()
    {
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

    void InitializeFirebase()
    {
        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;

        // Initialize Firebase Realtime Database
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Set up authentication state change listener
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        signUpButton.onClick.AddListener(() => RegisterAsync());
        loginButton.onClick.AddListener(() => LoginAsync());
    }

    public async Task<FirebaseUser> Login(string email, string password)
    {
        try
        {
            AuthResult authResult = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = authResult.User;

            // Start Unity Authentication here, after Firebase login
            await AuthenticationWrapper.SignInWithUsernamePasswordAsync(email, password);

            StartCoroutine(LoadUserData(user.UserId)); // Load user data after successful login
            return user;
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Login failed: {e.Message}");
            return null;  
        }
    }

    public async Task<FirebaseUser> SignUp(string email, string password)
    {
        try
        {
            AuthResult authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = authResult.User;

            // Start Unity Authentication after successful Firebase sign-up
            await AuthenticationWrapper.SignUpWithUsernamePasswordAsync(email, password);

            await SaveUserProfile(user.UserId, nameRegisterField.text);

            return user;
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Sign up failed: {e.Message}");
            return null;  
        }
    }

    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            user = auth.CurrentUser;
            if (user != null)
            {
                Debug.Log("Signed in " + user.UserId);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                Debug.Log("User is signed out.");
            }
        }
    }

    public async void RegisterAsync()
{
    // Sign out the current user before registering a new one
    auth.SignOut();

    string name = nameRegisterField.text;
    string email = emailRegisterField.text;
    string password = passwordRegisterField.text;
    string confirmPassword = confirmPasswordRegisterField.text;

    if (string.IsNullOrEmpty(name))
    {
        StartCoroutine(ShowRegistrationFeedback("Name field is empty"));
        return;
    }

    if (string.IsNullOrEmpty(email))
    {
        StartCoroutine(ShowRegistrationFeedback("Email field is empty"));
        return;
    }

    if (password != confirmPassword)
    {
        StartCoroutine(ShowRegistrationFeedback("Passwords do not match"));
        return;
    }

    // Check if AuthenticationWrapper is initialized
    bool initialized = await AuthenticationWrapper.InitializeAuthentication();
    if (!initialized)
    {
        Debug.LogError("Authentication services not initialized.");
        return;
    }

    try
    {
        // Sign-up process
        user = await SignUp(email, password);
        if (user != null)
        {
            PlayerPrefs.SetString("PlayerNameKey", name);
            PlayerPrefs.Save();
            Debug.Log("Registration Successful. Welcome " + name);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            Debug.LogError("Sign-up failed.");
        }
    }
    catch (FirebaseException ex)
    {
        HandleFirebaseError(ex, "Registration Failed");
    }
}



    public async void LoginAsync()
    {
        string email = emailLoginField.text;
        string password = passwordLoginField.text;

        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowLoginFeedback("Email field is empty"));
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            StartCoroutine(ShowLoginFeedback("Password field is empty"));
            return;
        }

        bool initialized = await AuthenticationWrapper.InitializeAuthentication();
        if (!initialized)
        {
            Debug.LogError("Authentication services not initialized.");
            return;
        }

        try
        {
            // Start login process
            await AuthenticationWrapper.SignInWithUsernamePasswordAsync(email, password);
            if (AuthenticationWrapper.AuthState == AuthState.Authenticated)
            {
                PlayerPrefs.SetString("PlayerNameKey", email);
                PlayerPrefs.Save();
                Debug.LogFormat("{0} You Are Successfully Logged In", email);
                StartCoroutine(ShowLoginFeedback("Login successful!"));
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                Debug.LogError("Login failed.");
            }
        }
        catch (FirebaseException ex)
        {
            StartCoroutine(ShowLoginFeedback("Wrong Credentials"));
        }
    }

    // Coroutine to show and hide login feedback text
    private IEnumerator ShowLoginFeedback(string message)
    {
        loginFeedbackText.text = message;
        loginFeedbackText.gameObject.SetActive(true); // Show the feedback text

        yield return new WaitForSeconds(4); // Wait for 2 seconds

        loginFeedbackText.gameObject.SetActive(false); // Hide the feedback text
    }

     private IEnumerator ShowRegistrationFeedback(string message)
    {
        registrationFeedbackText.text = message;
        registrationFeedbackText.gameObject.SetActive(true); // Show the feedback text

        yield return new WaitForSeconds(4); // Wait for 4 seconds

        registrationFeedbackText.gameObject.SetActive(false); // Hide the feedback text
    }


    public IEnumerator LoadUserData(string userId)
    {
        // Wait until the Firebase tasks are completed
        var task = FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(userId)
            .GetValueAsync();

        // Wait for the task to complete
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError("Failed to load user data: " + task.Exception);
        }
        else if (task.IsCompleted)
        {
            DataSnapshot snapshot = task.Result;
            // Process the loaded data here
            Debug.Log("User data loaded successfully.");
            // Example: Access user data from snapshot
            var userData = snapshot.Value as Dictionary<string, object>;
            // Do something with userData
        }
    }

    private async Task SaveUserProfile(string userId, string name)
    {
        // Save user profile information in the Firebase database
        var userProfile = new Dictionary<string, object>
        {
            { "displayName", name }
            // Add other user fields as necessary
        };

        await databaseReference.Child("users").Child(userId).SetValueAsync(userProfile);
    }

    private void HandleFirebaseError(FirebaseException exception, string errorPrefix)
    {
        string errorMessage = errorPrefix + ": ";
        AuthError authError = (AuthError)exception.ErrorCode;
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
