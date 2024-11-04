using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    // Initialize Unity Services
    public static async Task<bool> InitializeAuthentication()
    {
        try
        {
            Debug.Log("Initializing Unity Services...");
            await UnityServices.InitializeAsync();  // Ensure this completes successfully

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Player signed in with ID: {AuthenticationService.Instance.PlayerId}");
                AuthState = AuthState.Authenticated;
            };

            Debug.Log("Unity Services initialized.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
            return false;
        }
    }

    // Call this method before any sign-in or sign-up
    public static async Task<bool> PrepareAuthentication()
    {
        return await InitializeAuthentication();
    }

    // Sign-up a new player with username and password
    public static async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        if (AuthState != AuthState.Authenticated)
        {
            await PrepareAuthentication(); // Ensure services are initialized
        }

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("Sign-up is successful.");
            AuthState = AuthState.Authenticated;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Sign-up failed: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Sign-up failed: {ex.Message}");
        }
    }

    public static async Task<AuthState> DoAuth(int maxRetries = 5)
    {
        // Comment or remove this if you don't want to sign in anonymously
        // await SignInAnonymouslyAsync(maxRetries);

        // Instead, let the Firebase login handle the authentication.
        return AuthState;
    }


    // Sign-in an existing player with username and password
    public static async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        if (AuthState != AuthState.Authenticated)
        {
            await PrepareAuthentication(); // Ensure services are initialized
        }

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("Sign-in is successful.");
            AuthState = AuthState.Authenticated;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Sign-in failed: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Sign-in failed: {ex.Message}");
        }
    }

    // Upgrade an anonymous account to username/password
    public static async Task AddUsernamePasswordAsync(string username, string password)
    {
        if (AuthState != AuthState.Authenticated)
        {
            await PrepareAuthentication(); // Ensure services are initialized
        }

        try
        {
            await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
            Debug.Log("Username and password added successfully.");
            AuthState = AuthState.Authenticated;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Failed to add username and password: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Failed to add username and password: {ex.Message}");
        }
    }
    public static async Task UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogError("User must be authenticated to update the password.");
            return;
        }

        try
        {
            // Call the UpdatePasswordAsync method with current and new password
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
            Debug.Log("Password updated successfully in Unity Authentication.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Failed to update password: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Failed to update password: {ex.Message}");
        }
    }

    // AuthenticationWrapper.cs
    public static void UpdatePasswordInUnityAuth(string email, string newPassword)
    {
        // Call the asynchronous method and handle its completion
        UpdatePasswordAsync(email, newPassword).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Password updated successfully in Unity Authentication.");
            }
            else
            {
                Debug.LogError("Failed to update password in Unity Authentication: " + task.Exception);
            }
        });
    }

    // Anonymous sign-in
    public static async Task<AuthState> SignInAnonymouslyAsync(int maxRetries = 5)
    {
        if (AuthState == AuthState.Authenticated)
            return AuthState;

        AuthState = AuthState.Authenticating;

        int retries = 0;
        while (retries < maxRetries && AuthState == AuthState.Authenticating)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("Anonymous sign-in successful.");
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError(ex.Message);
                AuthState = AuthState.Error;
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError(ex.Message);
                AuthState = AuthState.Error;
            }

            retries++;
            await Task.Delay(1000);
        }

        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Anonymous sign-in failed after {retries} attempts.");
            AuthState = AuthState.TimeOut;
        }

        return AuthState;
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}
