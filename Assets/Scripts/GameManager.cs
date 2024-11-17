using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public string PlayerID { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SetPlayerID(string id) {
        if (!string.IsNullOrEmpty(id)) {
            PlayerID = id;
            Debug.Log("Player ID set to: " + PlayerID);
        } else {
            Debug.LogError("Player ID is null or empty");
        }
    }

    public string GetPlayerID() {
        return PlayerID;
    }
}
