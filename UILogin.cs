// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UILogin : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Text status;
    [SerializeField] InputField inputName;
    [SerializeField] InputField inputPass;
    [SerializeField] Dropdown dropdownServer;
    [SerializeField] Text currentServer;
    [SerializeField] Button btnLogin;
    [SerializeField] Button btnRegister;
    [SerializeField, TextArea(1, 30)] string registerMessage = "First time? Just log in and we will\ncreate an account automatically.";
    [SerializeField] Button btnHost;
    [SerializeField] Button btnDedicated;
    [SerializeField] Button btnCancel;
    [SerializeField] Button btnQuit;

    // cache
    NetworkManagerMMO manager;

    void Awake() {
        // NetworkManager.singleton is null for some reason
        manager = FindObjectOfType<NetworkManagerMMO>();

        // button onclicks
        btnQuit.onClick.SetListener(() => { Application.Quit(); });
        btnLogin.onClick.SetListener(() => { manager.StartClient(); });
        btnRegister.onClick.SetListener(() => { FindObjectOfType<UIPopup>().Show(registerMessage); });
        btnHost.onClick.SetListener(() => { manager.StartHost(); });
        btnCancel.onClick.SetListener(() => { manager.StopClient(); });
        btnDedicated.onClick.SetListener(() => { manager.StartServer(); });
    }

    void Start() {
        // load last server by name in case order changes some day.
        if (PlayerPrefs.HasKey("LastServer")) {
            string last = PlayerPrefs.GetString("LastServer", "");
            dropdownServer.value =  manager.serverList.FindIndex(s => s.name == last);
        }
    }

    void OnDestroy() {
        // save last server by name in case order changes some day
        PlayerPrefs.SetString("LastServer", currentServer.text);
    }

    void Update() {
        // only update while visible
        if (!panel.activeSelf) return;
        
        // status
        status.text = manager.IsConnecting() ? "Connecting..." : "";

        // button states
        btnLogin.interactable = !manager.IsConnecting();
        btnHost.interactable = !manager.IsConnecting();
        btnCancel.gameObject.SetActive(manager.IsConnecting());
        btnDedicated.interactable = !manager.IsConnecting();

        // inputs
        manager.id = inputName.text;
        manager.pw = inputPass.text;

        // copy servers to dropdown; copy current one to networkmanager ip/port.
        dropdownServer.interactable = !manager.IsConnecting();
        dropdownServer.options.Clear();
        foreach (var server in manager.serverList)
            dropdownServer.options.Add(new Dropdown.OptionData(server.name));
        
        int idx = dropdownServer.value;
        if (idx != -1) {
            // we also have to refresh the current text, otherwise it's still
            // 'Option A'
            currentServer.text = dropdownServer.options[idx].text;

            // set selected ip + port in networkmanager
            manager.networkAddress = manager.serverList[idx].ip;
            manager.networkPort = manager.serverList[idx].port;
        }
    }

    public void Show() { panel.SetActive(true); }
    public void Hide() { panel.SetActive(false); }
}
