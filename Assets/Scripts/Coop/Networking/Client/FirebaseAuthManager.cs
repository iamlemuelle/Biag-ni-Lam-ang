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
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Unity.Services.Core;
using Unity.Services.Authentication;


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

    [Space]
    [Header("Forgot Password")]
    public TMP_InputField emailResetField;
    public TMP_InputField codeInputField;
    public TMP_InputField currentPasswordField;
    public TMP_InputField newPasswordField;
    public Button sendResetButton;
    public Button verifyCodeButton;
    public Button resetPasswordButton;
    public TMP_Text resetFeedbackText;
    

    private string generatedResetCode; // Temporary storage for the reset code

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            Debug.Log($"Dependency Status: {dependencyStatus}");
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

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync(); // Initialize Unity Services
            Debug.Log("Unity Services initialized successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Unity Services: " + e.Message);
        }
    }
    void InitializeFirebase()
    {
        // Initialize Firebase Auth and Database
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        
        // Assign button listeners
        signUpButton.onClick.AddListener(() => RegisterAsync());
        loginButton.onClick.AddListener(() => LoginAsync());
        sendResetButton.onClick.AddListener(SendVerificationCode);
        verifyCodeButton.onClick.AddListener(VerifyResetCode);
        resetPasswordButton.onClick.AddListener(ResetPassword);

        // Enable debug logging for Firebase
        Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;
    }
    public void SendVerificationCode()
    {
        string email = emailResetField.text;  // Get the email from the input field

        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowFeedback(resetFeedbackText, "Please enter an email address."));
            return;
        }

        if (!IsValidEmail(email))
        {
            StartCoroutine(ShowFeedback(resetFeedbackText, "Invalid email format."));
            return;
        }

        try
        {
            // Generate a simple verification code (e.g., a 6-digit number)
            generatedResetCode = UnityEngine.Random.Range(100000, 999999).ToString();
            Debug.Log($"Generated code: {generatedResetCode}");

            // Send the code via email using SMTP
            SendEmail(email, generatedResetCode);
            StartCoroutine(ShowFeedback(resetFeedbackText, "A verification code has been sent to your email."));
        }
        catch (Exception e)
        {
            Debug.LogError("Error while sending the verification code: " + e.Message);
            StartCoroutine(ShowFeedback(resetFeedbackText, "Failed to send verification code."));
        }
    }


    private void SendEmail(string recipientEmail, string code)
    {
        try
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("baekjiheonie8@gmail.com", "mpzy qwzj nfgs qjwc "),  // Replace with your email and app-specific password
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            mail.From = new MailAddress("baekjiheonie8@gmail.com");  // Replace with your email
            mail.To.Add(new MailAddress(recipientEmail));
            mail.Subject = "Your Verification Code";
            mail.Body = $"Your verification code is: {code}";

            // Bypass SSL certificate validation (use cautiously in production)
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            SmtpServer.Send(mail);
            Debug.Log("Email sent successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send email: " + e.Message);
            throw;  // Rethrow the exception to handle in the calling method
        }
    }

    public void VerifyResetCode()
    {
        string inputCode = codeInputField.text;  // Get the code from the input field

        if (string.IsNullOrEmpty(inputCode))
        {
            StartCoroutine(ShowFeedback(resetFeedbackText, "Please enter the code sent to your email."));
            return;
        }

        if (inputCode == generatedResetCode)
        {
            Debug.Log("Verification code matched.");
            StartCoroutine(ShowFeedback(resetFeedbackText, "Code verified. You can now reset your password."));

            // Disable the send reset button and code input field
            sendResetButton.gameObject.SetActive(false); // Hide the send reset button
            codeInputField.gameObject.SetActive(false); // Hide the code input field

            // Enable the new password input field
            newPasswordField.interactable = true; // Show the new password input field
            currentPasswordField.gameObject.SetActive(true);
            resetPasswordButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Invalid verification code.");
            StartCoroutine(ShowFeedback(resetFeedbackText, "Invalid verification code. Please try again."));
        }
    }
    public void ResetPassword()
    {
        string newPassword = newPasswordField.text;

        if (string.IsNullOrEmpty(newPassword))
        {
            StartCoroutine(ShowFeedback(resetFeedbackText, "Please enter a new password."));
            return;
        }

        // Get the email for the user that is trying to reset their password
        string email = emailResetField.text;  // Use the email entered earlier

        // Update the password in Unity's authentication system directly
        UpdatePasswordInUnityAuth(email, newPassword); // Ensure you're passing both email and newPassword

        // Provide feedback to the user
        StartCoroutine(ShowFeedback(resetFeedbackText, "Password updated successfully in Unity authentication."));
    }


     public async Task UpdatePasswordInUnityAuth(string currentPassword, string newPassword)
    {
        try
        {
            // Ensure Unity Services are initialized (Initialize is called in Start())
            await UnityServices.InitializeAsync(); // Initialize again if necessary

            // Now proceed with the password update
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
            Debug.Log("Password updated successfully in Unity Authentication.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to update password in Unity Authentication: " + e.Message);
        }
    }


    private IEnumerator ShowFeedback(TMP_Text feedbackText, string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        yield return new WaitForSeconds(4);

        feedbackText.gameObject.SetActive(false);
    }

    public async Task<FirebaseUser> Login(string email, string password)
    {
        try
        {
            // AuthResult authResult = await auth.SignInWithEmailAndPasswordAsync(email, password);
            // user = authResult.User;

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
            StartCoroutine(ShowRegistrationFeedback("Attempting to create a new user with email: " + email));
            AuthResult authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = authResult.User;

            StartCoroutine(ShowRegistrationFeedback("Firebase user created successfully."));
            await AuthenticationWrapper.SignUpWithUsernamePasswordAsync(email, password);

            await SaveUserProfile(user.UserId, nameRegisterField.text);
            return user;
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Signup failed: {e.Message} - {e.StackTrace}");
            StartCoroutine(ShowRegistrationFeedback($"Signup failed: {e.Message}"));
            return null;
        }
    }
    public async void RegisterAsync()
    {
        string name = nameRegisterField.text;
        string email = emailRegisterField.text;
        string password = passwordRegisterField.text;
        string confirmPassword = confirmPasswordRegisterField.text;

        if (string.IsNullOrEmpty(name))
        {
            StartCoroutine(ShowRegistrationFeedback("Walang laman ang field ng username"));
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowRegistrationFeedback("Walang laman ang field ng email"));
            return;
        }

        if (password != confirmPassword)
        {
            StartCoroutine(ShowRegistrationFeedback("Hindi magkatugma ang password"));
            return;
        }

        // Check internet connectivity before proceeding
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            StartCoroutine(ShowRegistrationFeedback("No internet connection."));
            return;
        }

        bool initialized = await AuthenticationWrapper.InitializeAuthentication();
        if (!initialized)
        {
            StartCoroutine(ShowRegistrationFeedback("Authentication services not initialized."));
            return;
        }

        try
        {
            Debug.Log("Starting the signup process.");
            user = await SignUp(email, password);
            if (user != null)
            {
                string playerID = user.UserId; // Unique UID from Firebase

                Debug.Log("Signup successful! PlayerID: " + playerID);

                // Save the PlayerID in the GameManager
                GameManager.Instance.SetPlayerID(playerID);
                PlayerPrefs.SetString("PlayerNameKey", name);
                PlayerPrefs.Save();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                StartCoroutine(ShowRegistrationFeedback("Invalid username or email address"));
            }
        }
        catch (FirebaseException ex)
        {
            HandleFirebaseError(ex, "Registration Failed");
        }
    }


    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    public async void LoginAsync()
    {
        string email = emailLoginField.text;
        string password = passwordLoginField.text;

        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowLoginFeedback("Walang laman ang field ng email"));
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            StartCoroutine(ShowLoginFeedback("Walang laman ang field ng password"));
            return;
        }

        bool initialized = await AuthenticationWrapper.InitializeAuthentication();
        if (!initialized)
        {
            Debug.LogError("Authentication services not initialized.");
            StartCoroutine(ShowLoginFeedback("Hindi wasto ang email o password "));
            return;
        }

        try
        {
            // Start login process
            AuthResult authResult = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = authResult.User;
            await AuthenticationWrapper.SignInWithUsernamePasswordAsync(email, password);
            if (AuthenticationWrapper.AuthState == AuthState.Authenticated)
            {
                Debug.Log("Login successful! PlayerID: " + user.UserId);
                // Save the PlayerID in the GameManager
                GameManager.Instance.SetPlayerID(user.UserId);
                PlayerPrefs.SetString("PlayerNameKey", email);
                PlayerPrefs.Save();
                Debug.LogFormat("{0} You Are Successfully Logged In", email);
                StartCoroutine(ShowLoginFeedback("Login successful!"));
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                StartCoroutine(ShowLoginFeedback("Invalid Username or Password"));
            }
        }
        catch (FirebaseException ex)
        {
            StartCoroutine(ShowLoginFeedback("Invalid Username or Password"));
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
        var task = databaseReference.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCompleted && !task.IsFaulted)
        {
            DataSnapshot snapshot = task.Result;
            var userData = snapshot.Value as Dictionary<string, object>;
            if (userData != null)
            {
                Debug.Log("Login successful! PlayerID: " + userId);

                // Save the PlayerID in the GameManager
                GameManager.Instance.SetPlayerID(userId);
            }
        }
        else
        {
            Debug.LogError("Failed to load user data.");
        }
    }

    private async Task SaveUserProfile(string userId, string name)
    {
        var userProfile = new Dictionary<string, object>
        {
            { "displayName", name },
            { "createdAt", DateTime.UtcNow.ToString("o") } // Save creation timestamp
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