using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMinimap : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] float zoomMin = 5.0f;
    [SerializeField] float zoomMax = 50.0f;
    [SerializeField] float zoomStepSize = 5.0f;
    [SerializeField] Text levelName;
    [SerializeField] Button buttonPlus;
    [SerializeField] Button buttonMinus;
    [SerializeField] Camera minimapCamera;

    void Start() {
        buttonPlus.onClick.SetListener(() => {
            minimapCamera.orthographicSize = Mathf.Max(minimapCamera.orthographicSize - zoomStepSize, zoomMin);
        });
        buttonMinus.onClick.SetListener(() => {
            minimapCamera.orthographicSize = Mathf.Min(minimapCamera.orthographicSize + zoomStepSize, zoomMax);
        });
    }

    void Update() {
        var player = Utils.ClientLocalPlayer();
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        levelName.text = SceneManager.GetActiveScene().name;
    }
}
