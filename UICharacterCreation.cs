﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class UICharacterCreation : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] InputField inputCharacterName;
    [SerializeField] Dropdown dropdownClass;
    [SerializeField, FormerlySerializedAs("dropdownCurrent")] Text currentClass;
    [SerializeField] Button createButton;
    [SerializeField] Button cancelButton;

    // cache
    NetworkManagerMMO manager;

    void Awake() {
        // NetworkManager.singleton is null for some reason
        manager = FindObjectOfType<NetworkManagerMMO>();

        // button onclicks
        createButton.onClick.SetListener(() => {
            var msg = new CharacterCreateMsg{
                name = inputCharacterName.text,
                classIndex = dropdownClass.value
            };
            manager.client.Send(CharacterCreateMsg.MsgId, msg);
        });
        cancelButton.onClick.SetListener(() => {
            inputCharacterName.text = "";
            Hide();
            FindObjectOfType<UICharacterSelection>().Show();
        });
    }

    void Update() {
        // only update if visible
        if (!panel.activeSelf) return;

        // hide if disconnected
        if (!NetworkClient.active) Hide();

        // copy player classes to class selection
        dropdownClass.options.Clear();
        foreach (var p in manager.GetPlayerClasses())
            dropdownClass.options.Add(new Dropdown.OptionData(p.name));
        
        // we also have to refresh the current text, otherwise it's still
        // 'Option A'
        int idx = dropdownClass.value;
        if (idx != -1) currentClass.text = dropdownClass.options[idx].text;
    }

    public void Hide() { panel.SetActive(false); }
    public void Show() { panel.SetActive(true); }
}
