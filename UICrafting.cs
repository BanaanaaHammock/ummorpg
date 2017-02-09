using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UICrafting : MonoBehaviour {
    [SerializeField] KeyCode hotKey = KeyCode.T;
    [SerializeField] GameObject panel;
    [SerializeField] GameObject ingredientSlotPrefab;
    [SerializeField] Transform ingredientContent;
    [SerializeField] UIDragAndDropable result;
    [SerializeField] Button buttonCraft;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only update the panel if it's active
        if (panel.activeSelf) {
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(ingredientSlotPrefab, player.craftingIndices.Count, ingredientContent);

            // refresh all
            for (int i = 0; i < player.craftingIndices.Count; ++i) {
                var entry = ingredientContent.GetChild(i).GetChild(0); // slot entry
                entry.name = i.ToString(); // for drag and drop
                int itemIdx = player.craftingIndices[i];

                if (0 <= itemIdx && itemIdx < player.inventory.Count &&
                    player.inventory[itemIdx].valid) {
                    var item = player.inventory[itemIdx];

                    // set state
                    entry.GetComponent<UIShowToolTip>().enabled = true;
                    entry.GetComponent<UIDragAndDropable>().dragable = true;
                    // note: entries should be dropable at all times

                    // image
                    entry.GetComponent<Image>().color = Color.white;
                    entry.GetComponent<Image>().sprite = item.image;
                    entry.GetComponent<UIShowToolTip>().text = item.Tooltip();

                    // amount overlay: not needed while it's always one item
                    //entry.GetChild(0).gameObject.SetActive(item.amount > 1);
                    //if (item.amount > 1) entry.GetComponentInChildren<Text>().text = item.amount.ToString();
                } else {
                    // reset the index if it's not valid anymore
                    player.craftingIndices[i] = -1;

                    // remove listeners
                    entry.GetComponent<Button>().onClick.RemoveAllListeners();

                    // set state
                    entry.GetComponent<UIShowToolTip>().enabled = false;
                    entry.GetComponent<UIDragAndDropable>().dragable = false;

                    // image
                    entry.GetComponent<Image>().color = Color.clear;
                    entry.GetComponent<Image>().sprite = null;

                    // amount overlay: not needed while it's always one item
                    //entry.GetChild(0).gameObject.SetActive(false);
                }
            }

            // result slot: find a matching recipe (if any)
            // -> build list of item templates
            var validIndices = player.craftingIndices.Where(
                idx => 0 <= idx && idx < player.inventory.Count && 
                player.inventory[idx].valid
            );

            var items = validIndices.Select(idx => player.inventory[idx].template).ToList();
            var recipe = RecipeTemplate.dict.Values.ToList().Find(r => r.CanCraftWith(items)); // good enough for now
            if (recipe != null) {
                // set state
                result.GetComponent<UIShowToolTip>().enabled = true;

                // image
                result.GetComponent<Image>().color = Color.white;
                result.GetComponent<Image>().sprite = recipe.result.image;
                result.GetComponent<UIShowToolTip>().text = new Item(recipe.result).Tooltip();
            } else {
                // remove listeners
                result.GetComponent<Button>().onClick.RemoveAllListeners();

                // set state
                result.GetComponent<UIShowToolTip>().enabled = false;

                // image
                result.GetComponent<Image>().color = Color.clear;
                result.GetComponent<Image>().sprite = null;
            }

            // craft button
            buttonCraft.interactable = recipe != null && player.InventoryCanAddAmount(recipe.result, 1);
            buttonCraft.onClick.SetListener(() => {
                player.CmdCraft(validIndices.ToArray());
            });
        }
    }
}
