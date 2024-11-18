using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;
    private static string _username; // to hold the username or email


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
            _username = username;
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
            _username = username;
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
    public static async Task UpdatePasswordAsync(string currentPassword, string newPassword)
    {

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
    public static async Task UpdatePasswordInUnityAuth(string currentPassword, string newPassword)
    {
        // Ensure the user is authenticated
        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogError("User must be authenticated to update the password.");
            return;
        }

        try
        {
            // Re-authenticate the user with current password
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(_username, currentPassword);
            
            // Now update the password
            await UpdatePasswordAsync(currentPassword, newPassword);
            
            // Optionally, sign in again with the new password
            await SignInWithUsernamePasswordAsync(_username, newPassword);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update password in Unity Authentication: {ex.Message}");
        }
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
