using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIFade : Singleton<UIFade>
{
    [SerializeField] private Image fadeScreen;
    [SerializeField] private GameObject fadeText; // The text to activate/deactivate
    [SerializeField] private float fadeSpeed = 1f;

    private IEnumerator fadeRoutine;

    // Modified FadeToBlack to make text activation optional
    public void FadeToBlack(bool activateText = false) 
    {
        if (fadeRoutine != null) 
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = FadeRoutine(1, activateText);
        StartCoroutine(fadeRoutine);
    }

    public void FadeToClear() 
    {
        if (fadeRoutine != null) 
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = FadeRoutine(0, false);
        StartCoroutine(fadeRoutine);
    }

    private IEnumerator FadeRoutine(float targetAlpha, bool activateText) 
    {
        while (!Mathf.Approximately(fadeScreen.color.a, targetAlpha)) 
        {
            float alpha = Mathf.MoveTowards(fadeScreen.color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, alpha);
            yield return null;
        }

        // Activate the text after the fade if requested
        SetTextActive(activateText);
    }

    // New method to control text activation
    public void SetTextActive(bool isActive) 
    {
        if (fadeText != null) 
        {
            fadeText.SetActive(isActive);
        }
    }

    public void MainMenu() 
    {
        SceneManager.LoadSceneAsync("Menu");
    }
}
