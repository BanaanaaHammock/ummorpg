using UnityEngine;
using UnityEngine.UI;

public class UIExperienceBar : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Slider bar;
    [SerializeField] Text status;
    [SerializeField]
    Text Levelonly;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        bar.value = player.ExpPercent();
        status.text = " (" + (player.ExpPercent() * 100f).ToString("F2") + "%)";
        Levelonly.text = "Lv " + player.level;
    }
}
