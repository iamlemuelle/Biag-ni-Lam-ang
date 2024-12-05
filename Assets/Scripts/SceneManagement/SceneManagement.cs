using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : Singleton<SceneManagement>
{
    public string SceneTransitionName { get; private set; }

    public void SetTransitionName(string sceneTransitionName) {
        this.SceneTransitionName = sceneTransitionName;
    }

    public void LoadScene()
{
    // Check if the key exists in PlayerPrefs
    if (PlayerPrefs.HasKey("CurrentScene"))
    {
        // Load the saved scene index
        int sceneIndex = PlayerPrefs.GetInt("CurrentScene");
        SceneManager.LoadSceneAsync(sceneIndex);

        // After loading the scene, load the player position
        StartCoroutine(LoadPlayerPosition(sceneIndex));
    }
    else
    {
        // Fallback to the default scene index (5)
        int defaultSceneIndex = 5;
        SceneManager.LoadSceneAsync(defaultSceneIndex);

        // Load the player position for the default scene
        StartCoroutine(LoadPlayerPosition(defaultSceneIndex));
    }
}


    public void NewGame() {
        // Delete all PlayerPrefs data
        PlayerPrefs.DeleteAll();
        
        // Load the starting scene for a new game
        SceneManager.LoadSceneAsync("Town"); // Scene index 4 is the starting area
    }

    private IEnumerator LoadPlayerPosition(int sceneIndex) {
        // Wait until the scene is loaded
        yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex == sceneIndex);

        // Load player's position
        float playerPosX = PlayerPrefs.GetFloat("PlayerPosX", 0);
        float playerPosY = PlayerPrefs.GetFloat("PlayerPosY", 0);
        float playerPosZ = PlayerPrefs.GetFloat("PlayerPosZ", 0);
        
        // Find the player object and set its position
        GameObject player = GameObject.Find("Player"); // Replace with the actual name of your player object
        if (player != null) {
            player.transform.position = new Vector3(playerPosX, playerPosY, playerPosZ);
        }
    }

    public void ExitGame() {
        Application.Quit();
    }
}
