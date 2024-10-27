using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;

public class FirebaseINIT : MonoBehaviour
{
    public static bool firebaseReady;

    async void Awake()
    {
        CheckIfReady();
    }

    void Update()
    {
        if (firebaseReady)
        {
            SceneManager.LoadScene("BootStrap"); // Adjust the name based on your actual login scene
        }
    }

    public static void CheckIfReady()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                Debug.Log("Firebase is ready for use.");
                
                // Set firebaseReady to true if you want to proceed with scene loading in Update()
                firebaseReady = true;
            }
            else
            {
                UnityEngine.Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }
}
