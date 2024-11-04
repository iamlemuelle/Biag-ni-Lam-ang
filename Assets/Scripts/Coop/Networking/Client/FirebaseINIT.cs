using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class FirebaseINIT : MonoBehaviour
{
    private static FirebaseINIT instance;
    public static bool firebaseReady;
    private FirebaseApp app;
    private DatabaseReference databaseReference; // Reference to the database

    // Singleton pattern to ensure only one instance exists
    public static FirebaseINIT Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("FirebaseINIT");
                instance = go.AddComponent<FirebaseINIT>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Initialize Firebase app and database
                app = FirebaseApp.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                firebaseReady = true;

                Debug.Log("Firebase is ready to use.");
                LoadNextScene();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void LoadNextScene()
    {
        if (firebaseReady)
        {
            SceneManager.LoadScene("BootStrap"); // Replace with your actual scene name
        }
    }

    // Method to write data to the database
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

    // Method to read data from the database
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
}
