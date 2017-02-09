using UnityEngine;
using UnityEngine.UI;

public class UIHealthMana : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Slider hpBar;
    [SerializeField] Text hpStatus;
    [SerializeField] Slider mpBar;
    [SerializeField] Text mpStatus;
    void Update() {
        var player = Utils.ClientLocalPlayer();
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        hpBar.value = player.HpPercent();
        hpStatus.text = player.hp + " / " + player.hpMax;

        mpBar.value = player.MpPercent();
        mpStatus.text = player.mp + " / " + player.mpMax;
        
    }
}
