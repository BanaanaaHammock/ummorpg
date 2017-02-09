// Saves the item info in a ScriptableObject that can be used ingame by
// referencing it from a MonoBehaviour. It only stores an item's static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some items may not be referenced by any
// entity ingame (e.g. when a special event item isn't dropped anymore after the
// event). But all items should still be loadable from the database, even if
// they are not referenced by anyone anymore. So we have to use Resources.Load.
// (before we added them to the dict in OnEnable, but that's only called for
//  those that are referenced in the game. All others will be ignored be Unity.)
//
// An Item can be created by right clicking the Resources folder and selecting
// Create -> uMMORPG Item. Existing items can be found in the Resources folder.
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public enum rarityLevels { None, Normal, Rare, Epic, Legendary };

[CreateAssetMenu(fileName="New Item", menuName="uMMORPG Item", order=999)]
public class ItemTemplate : ScriptableObject {
    [Header("Base Stats")]
    public string category;
    public int maxStack;
    public long buyPrice;
    public long sellPrice;
    public int minLevel; // level required to use/equip the item
    public bool sellable;
    public bool tradable;
    public bool destroyable;
    [TextArea(1, 30)] public string tooltip;
    public Sprite image;
    public Material ArmorTexture;
    public Material LegArmorTexture;
    public Material BootsArmorTexture;
    [Header("Usage Boosts")]
    public bool usageDestroy;
    public int usageHp;
    public int usageMp;
    public int usageExp;

    [Header("Equipment Boosts")]
    public int equipHpBonus;
    public int equipMpBonus;
    public int equipDamageBonus;
    public int equipDefenseBonus;
    [Range(0, 1)] public float equipBlockBonus; // % chance to block attacks
    [Range(0, 1)] public float equipCritBonus; // % chance

    public GameObject model; // Prefab
    public rarityLevels rarity;

    public static string[] itemRarityColors = new string[] { "", "#e3e3e3", "#2eba38", "#6b106b", "#ecbf6c" };

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    static Dictionary<string, ItemTemplate> cache = null;
    public static Dictionary<string, ItemTemplate> dict {
        get {
            // load if not loaded yet
            return cache ?? (cache = Resources.LoadAll<ItemTemplate>("").ToDictionary(
                item => item.name, item => item)
            );
        }
    }
    
    // inspector validation ////////////////////////////////////////////////////
    void OnValidate() {
        // make sure that the sell price <= buy price to avoid exploitation
        // (people should never buy an item for 1 gold and sell it for 2 gold)
        sellPrice = Math.Min(sellPrice, buyPrice);
    }
}
