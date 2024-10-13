using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public string currentLanguage = "tag"; // Default language
    public List<string> supportedLanguages = new List<string> { "tag", "il" }; // Add more languages if needed

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist through scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to switch the current language
    public void SetLanguage(string language)
    {
        if (supportedLanguages.Contains(language))
        {
            currentLanguage = language;
        }
        else
        {
            Debug.LogWarning("Unsupported language: " + language);
        }
    }
}
