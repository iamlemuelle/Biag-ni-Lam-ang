using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase.Database;

public class FirebaseSettings : MonoBehaviour
{
    private Firebase.FirebaseApp app;
    private DatabaseReference databaseReference; // Reference to the database

    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp
                app = Firebase.FirebaseApp.DefaultInstance;

                // Initialize the database reference
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                Debug.Log("Firebase is ready to use.");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    // Method to write data to the database
    public void WriteData(string path, string data)
    {
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
