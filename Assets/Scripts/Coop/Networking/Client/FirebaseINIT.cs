using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public static bool firebaseReady;
    
    private FirebaseApp app;
    private FirebaseAuth auth;
    private DatabaseReference databaseReference;

    // Singleton pattern to ensure only one instance exists
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();  // Initialize Firebase services on Awake
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void InitializeFirebase()
    {
        // Check Firebase dependencies
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            // Initialize Firebase services
            app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

            firebaseReady = true;  // Set flag to true once Firebase is initialized

            Debug.Log("Firebase initialized successfully.");
            LoadNextScene();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }

    private void LoadNextScene()
    {
        if (firebaseReady)
        {
            SceneManager.LoadScene("BootStrap"); // Replace with your actual scene name
        }
    }

    // Getters for Firebase services
    public FirebaseAuth GetAuth() => auth;
    public DatabaseReference GetDatabaseReference() => databaseReference;

    // Write data to Firebase Realtime Database
    public void WriteData(string path, string data)
    {
        if (!firebaseReady)
        {
            Debug.LogWarning("Firebase is not ready. Data write aborted.");
            return;
        }

        databaseReference.Child(path).SetValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Data written successfully to " + path);
            }
            else
            {
                Debug.LogError("Error writing data: " + task.Exception);
            }
        });
    }

    // Read data from Firebase Realtime Database
    public void ReadData(string path)
    {
        if (!firebaseReady)
        {
            Debug.LogWarning("Firebase is not ready. Data read aborted.");
            return;
        }

        databaseReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log("Data at " + path + ": " + snapshot.GetRawJsonValue());
                }
                else
                {
                    Debug.Log("No data available at " + path);
                }
            }
            else
            {
                Debug.LogError("Error reading data: " + task.Exception);
            }
        });
    }

    // You can also add more utility methods as needed (e.g., for handling authentication)
}
