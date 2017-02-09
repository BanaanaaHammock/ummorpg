// Simple character selection list. The charcter prefabs are known, so we could
// easily show 3D models, stats, etc. too.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

public class UICharacterSelection : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform content;
    // available characters (set after receiving the message from the server)
    [HideInInspector] public CharactersAvailableMsg characters;
    [SerializeField] Button createButton;
    [SerializeField] Button quitButton;

    // cache
    NetworkManagerMMO manager;

    void Awake() {
        // NetworkManager.singleton is null for some reason
        manager = FindObjectOfType<NetworkManagerMMO>();

        // button onclicks
        createButton.onClick.SetListener(() => {
            Hide();
            FindObjectOfType<UICharacterCreation>().Show();
        });
        quitButton.onClick.SetListener(() => { Application.Quit(); });
    }

    void Update() {
        // only update if visible
        if (!panel.activeSelf) return;

        // hide if disconnected or if a local player is in the game world
        if (!NetworkClient.active || Utils.ClientLocalPlayer() != null) Hide();

        // instantiate/destroy enough slots
        UIUtils.BalancePrefabs(slotPrefab, characters.characterNames.Length, content);

        // refresh all
        var prefabs = ((NetworkManagerMMO)NetworkManager.singleton).GetPlayerClasses();
        for (int i = 0; i < characters.characterNames.Length; ++i) {
            var prefab = prefabs.Find(p => p.name == characters.characterClasses[i]);
            var entry = content.GetChild(i);

            var txt = entry.GetChild(0).GetComponent<Text>();
            txt.text = characters.characterNames[i];

            var imgClass = entry.GetChild(1).GetComponent<Image>();
            imgClass.sprite = prefab.GetComponent<Player>().classIcon;
            
            var buttonSelect = entry.GetChild(2).GetComponent<Button>();
            int icopy = i; // needed for lambdas, otherwise i is Count
            buttonSelect.onClick.SetListener(() => {
                // use ClientScene.AddPlayer with a parameter, which calls
                // OnServerAddPlayer on the server.
                var msg = new CharacterSelectMsg{index=icopy};
                ClientScene.AddPlayer(manager.client.connection, 0, msg);
            });
            
            var buttonDelete = entry.GetChild(3).GetComponent<Button>();
            buttonDelete.onClick.SetListener(() => {
                // send delete message
                var msg = new CharacterDeleteMsg{index=icopy};
                manager.client.Send(CharacterDeleteMsg.MsgId, msg);
            });
        }
        createButton.interactable = characters.characterNames.Length < manager.charLimit;
    }

    public void Hide() { panel.SetActive(false); }
    public void Show() { panel.SetActive(true); }
}
