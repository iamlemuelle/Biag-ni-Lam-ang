using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public string currentLanguage; // Default language
    public List<string> supportedLanguages = new List<string> { "tag", "il" };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist through scenes
            LoadLanguage(); // Load the saved language
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadLanguage();
    }

    // Method to switch the current language and save it in PlayerPrefs
    public void SetLanguage(string language)
    {
        if (supportedLanguages.Contains(language))
        {
            currentLanguage = language;
            PlayerPrefs.SetString("Language", currentLanguage); // Save to PlayerPrefs
            PlayerPrefs.Save(); // Ensure changes are saved
        }
        else
        {
            Debug.LogWarning("Unsupported language: " + language);
        }
    }

    // Load the saved language from PlayerPrefs
    private void LoadLanguage()
    {
        if (PlayerPrefs.HasKey("Language"))
        {
            currentLanguage = PlayerPrefs.GetString("Language");
        }
        else
        {
            PlayerPrefs.SetString("Language", currentLanguage); // Save default if not set
        }
    }
}
