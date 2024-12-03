using UnityEngine;
using UnityEngine.UI;

public class QuestLog : MonoBehaviour
{
    [Header("Tagalog Quest Buttons")]
    [SerializeField] private Button[] tagalogButtons; // Buttons for Tagalog quests

    [Header("Ilocano Quest Buttons")]
    [SerializeField] private Button[] ilocanoButtons; // Buttons for Ilocano quests

    void Start()
    {
        // Initialize buttons and update display based on the current language from PlayerPrefs
        InitializeQuestLog();
    }

    public void InitializeQuestLog()
    {
        // Initialize buttons without changing their text
        InitializeLanguageButtons(tagalogButtons);
        InitializeLanguageButtons(ilocanoButtons);

        // Show buttons for the currently selected language based on PlayerPrefs
        string currentLanguage = PlayerPrefs.GetString("Language", "tag"); // Default to "tag" if not set
        UpdateLanguageDisplay(currentLanguage);
    }

    private void InitializeLanguageButtons(Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true); // Ensure the button is active

            // Optionally, add custom actions for the buttons if needed
            buttons[i].onClick.RemoveAllListeners(); // Clear existing listeners
            buttons[i].onClick.AddListener(() => Debug.Log($"Button {i} clicked"));
        }
    }

    public void UpdateQuestLog(QuestManager questManager)
    {
        // Update button visibility for completed quests in both languages
        UpdateLanguageButtons(tagalogButtons, questManager);
        UpdateLanguageButtons(ilocanoButtons, questManager);
    }

    private void UpdateLanguageButtons(Button[] buttons, QuestManager questManager)
    {
        for (int i = 0; i < buttons.Length && i < questManager.questCompleted.Length; i++)
        {
            if (questManager.questCompleted[i])
            {
                Debug.Log($"Hiding button for completed quest {i}.");
                buttons[i].gameObject.SetActive(false); // Hide button for completed quests
            }
        }
    }

    public void UpdateLanguageDisplay(string language)
    {
        Debug.Log($"Updating language display to: {language}"); // Log the language change

        bool showTagalogButtons = language.Equals("tag", System.StringComparison.OrdinalIgnoreCase);
        bool showIlocanoButtons = language.Equals("il", System.StringComparison.OrdinalIgnoreCase);

        // Toggle the visibility of the language buttons
        foreach (var button in tagalogButtons)
        {
            button.gameObject.SetActive(showTagalogButtons); // Show Tagalog buttons
        }

        foreach (var button in ilocanoButtons)
        {
            button.gameObject.SetActive(showIlocanoButtons); // Show Ilocano buttons
        }
    }
}
