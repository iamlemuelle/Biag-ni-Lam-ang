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

            // Load user data after successful login
            StartCoroutine(LoadUserData(user.UserId)); // Start the coroutine here

            // Pass the data to Unity's auth
            await AuthenticationWrapper.SignInWithUsernamePasswordAsync(email, password);

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

            // After successful Firebase sign-up, register in your authentication wrapper
            await AuthenticationWrapper.SignUpWithUsernamePasswordAsync(email, password);

            // Optionally, store additional user info in Firebase
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
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
            registrationFeedbackText.text = "Name field is empty";
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            registrationFeedbackText.text = "Email field is empty";
            return;
        }

        if (password != confirmPassword)
        {
            registrationFeedbackText.text = "Passwords do not match";
            return;
        }

        try
        {
            var userCredential = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = userCredential.User;  
            UserProfile userProfile = new UserProfile { DisplayName = name };
            await user.UpdateUserProfileAsync(userProfile);
            Debug.Log("Registration Successful. Welcome " + user.DisplayName);
            registrationFeedbackText.text = "Registration successful!";
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
            loginFeedbackText.text = "Email field is empty";
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            loginFeedbackText.text = "Password field is empty";
            return;
        }

        try
        {
            var userCredential = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = userCredential.User; 
            Debug.LogFormat("{0} You Are Successfully Logged In", user.DisplayName);
            loginFeedbackText.text = "Login successful!";
            
            // Start loading user data after login
            StartCoroutine(LoadUserData(user.UserId));

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        catch (FirebaseException ex)
        {
            HandleFirebaseError(ex, "Login Failed");
        }
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
