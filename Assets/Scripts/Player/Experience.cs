using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience : MonoBehaviour
{
    public static Experience Instance;
    public delegate void ExperienceChangeHandler(int amount);
    public event ExperienceChangeHandler OnExperienceChange;

    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        } 
        else 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void AddExperience(int amount) {
        Debug.Log("Experience Added: " + amount);
        OnExperienceChange?.Invoke(amount);
    }
}
