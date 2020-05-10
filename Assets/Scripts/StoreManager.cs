using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StoreManager {

    /* * * * Keys for expendable items - DO NOT CHANGE * * * */
    public static readonly string ITEM_KEY_EXTRA_LIFE = "store.1up";
    public static readonly string ITEM_KEY_NO_ADS_TEMPORARY = "store.noadstemp";
    public static readonly string ITEM_KEY_RESET_AVERAGE = "store.resetavg";
    private static readonly int numExpendables = 3;

    /* * * * Keys for unlockable items - DO NOT CHANGE * * * */
    public static readonly string ITEM_KEY_HARD_MODE = "store.hardmode";
    public static readonly string ITEM_KEY_COLORS_PAS_FRU = "store.pas/fru";
    public static readonly string ITEM_KEY_COLORS_WAR_COO = "store.war/coo";
    public static readonly string ITEM_KEY_COLORS_MID_WHI = "store.mid/whi";
    public static readonly string ITEM_KEY_COLORS_RGB_CMY = "store.rgb/cmy";
    public static readonly string ITEM_KEY_BRAGGING_RIGHTS = "store.brag";

    private static readonly StoreItem[] STORE_ITEMS = { // TODO: increase prices
        // expendables
        new StoreItem(ITEM_KEY_EXTRA_LIFE, "Extra life", 5, "Revive from the game over screen."),
        new StoreItem(ITEM_KEY_NO_ADS_TEMPORARY, "No ads for 24 hours", 10, "Activate immediately (they stack).", 24),
        new StoreItem(ITEM_KEY_RESET_AVERAGE, "Reset average", 25, "Reset from the Stats menu."),
        // unlockables
        new StoreItem(ITEM_KEY_HARD_MODE, "Hard mode", 40, "Toggle when starting a game. Includes a gold boost."),
        new StoreItem(ITEM_KEY_COLORS_PAS_FRU, "Pastel & Fruit", 50, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_KEY_COLORS_WAR_COO, "Warm & Cool", 50, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_KEY_COLORS_MID_WHI, "Midnight & Whiteout", 50, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_KEY_COLORS_RGB_CMY, "RGB & CMYK", 50, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_KEY_BRAGGING_RIGHTS, "Bragging rights", 500, "Brag anywhere (unlocks a secret).")
    };

    /* * * * Public getters * * * */

    public static int getNumExpendables() {
        return numExpendables;
    }

    public static int getNumItems() {
        return STORE_ITEMS.Length;
    }

    public static StoreItem getItemWithID(int id) {
        return STORE_ITEMS[id];
    }

    /* * * * Buying items * * * */

    ///<summary>Updates gold amount and relevant item count as appropriate. Also updates expiration date if applicable.</summary>
    public static bool buyItem(int id) {
        StoreItem item = STORE_ITEMS[id];
        string key = item.getKey();
        int numBought = DataAndSettingsManager.getNumBoughtForStoreItem(key);
        int gold = DataAndSettingsManager.getGoldAmount();
        if (gold >= item.getCost() && (id < getNumExpendables() || numBought < 1)) {
            DataAndSettingsManager.setNumBoughtForStoreItem(key, numBought + 1);
            DataAndSettingsManager.setGoldAmount(gold -= item.getCost());
            if (item.hasLifespan()) {
                DateTime expiration = DataAndSettingsManager.getExpirationDateForStoreItem(key);
                DateTime now = DateTime.Now;
                TimeSpan lifespan = new TimeSpan(item.getLifespanHours(), 0, 0);
                if (expiration.CompareTo(now) < 0) {
                    // the item has already expired, so set a new expiration date
                    DataAndSettingsManager.setExpirationDateForStoreItem(key, now.Add(lifespan));
                }
                else {
                    // the item hasn't expired yet, so advance the expiration date further
                    DataAndSettingsManager.setExpirationDateForStoreItem(key, expiration.Add(lifespan));
                }
                //Debug.Log("expiration date was " + expiration.ToString());
            }
            return true;
        }
        return false;
    }

    ///<summary>Decrements the item count by 1. For now, actual item functionality should be handled where the item is expended.</summary>
    public static void expendItem(string key) { // TODO: consider handling item functionality in here
        int numBought = DataAndSettingsManager.getNumBoughtForStoreItem(key);
        DataAndSettingsManager.setNumBoughtForStoreItem(key, numBought - 1);
    }

    ///<summary>Sets the counts of items with lifespans according to how many lifespans are left before their expiration dates.</summary>
    public static void updateLifespanItemCounts() {
        foreach (StoreItem item in STORE_ITEMS) {
            string key = item.getKey();
            int numBought = DataAndSettingsManager.getNumBoughtForStoreItem(key);
            if (numBought > 0 && item.hasLifespan()) {
                DateTime expiration = DataAndSettingsManager.getExpirationDateForStoreItem(key);
                DateTime now = DateTime.Now;
                int difference = expiration.Subtract(now).Hours;
                if (difference > 0) {
                    DataAndSettingsManager.setNumBoughtForStoreItem(key, difference / item.getLifespanHours());
                }
                else {
                    DataAndSettingsManager.setNumBoughtForStoreItem(key, 0);
                }
            }
        }
    }

    /* * * * Item-specific methods * * * */

    ///<summary>Returns whether interstitial ads should be shown, based on whether the temporary no-ads item is active.
    /// Should also check whether the user has bought the permanent no-ads IAP.</summary>
    public static bool shouldShowAds() {
        DateTime expiration = DataAndSettingsManager.getExpirationDateForStoreItem(ITEM_KEY_NO_ADS_TEMPORARY);
        DateTime now = DateTime.Now;
        return (expiration.CompareTo(now) < 0);
    }

}

///<summary>A class to organize information about store items.</summary>
public class StoreItem {
    
    private string key;
    private string name;
    private int cost;
    private string description;
    ///<summary>If 0, item does not expire. Otherwise, this is its lifespan in hours.</summary>
    private int lifespanHours;

    public StoreItem(string key, string name, int cost, string description): this(key, name, cost, description, 0) {}
    public StoreItem(string key, string name, int cost, string description, int lifespanHours) {
        this.key = key;
        this.name = name;
        this.cost = cost;
        this.description = description;
        this.lifespanHours = lifespanHours;
    }

    public string getKey() { return this.key; }
    public string getName() { return this.name; }
    public int getCost() { return this.cost; }
    public string getDescription() { return this.description; }
    public bool hasLifespan() { return (this.lifespanHours > 0); }
    ///<summary>May want to call `hasLifespan()` first.</summary>
    public int getLifespanHours() { return this.lifespanHours; }

    ///<summary>Returns the number of this item owned by the user (not counting expended ones).</summary>
    public int getNumBought() {
        return DataAndSettingsManager.getNumBoughtForStoreItem(this.key);
    }

}
