using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEntrance : MonoBehaviour
{
    [SerializeField] private string transitionName;

    // the Instance that you can see is from the class singleton so that we can easily access the class without initializing the class.
    private void Start() {
        if (transitionName == SceneManagement.Instance.SceneTransitionName) {
            PlayerController.Instance.transform.position = this.transform.position;
            CameraController.Instance.SetPlayerCameraFollow(); // after the player entered the scene 

            UIFade.Instance.FadeToClear();
        }
    }
}
