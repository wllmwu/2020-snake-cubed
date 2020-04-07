using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour {

    /* * * * Names of expendable items * * * */
    public static readonly string ITEM_KEY_EXTRA_LIFE = "store.1up";
    public static readonly string ITEM_KEY_NO_ADS_TEMPORARY = "store.noadstemp";
    public static readonly string ITEM_KEY_RESET_AVERAGE = "store.resetavg";

    /* * * * Names of unlockable items * * * */
    public static readonly string ITEM_KEY_HARD_MODE = "store.hardmode";
    public static readonly string ITEM_KEY_COLORS_PAS_FRU = "store.pas/fru";
    public static readonly string ITEM_KEY_COLORS_WAR_COO = "store.war/coo";
    public static readonly string ITEM_KEY_COLORS_MID_WHI = "store.mid/whi";
    public static readonly string ITEM_KEY_COLORS_RGB_CMY = "store.rgb/cmy";
    public static readonly string ITEM_KEY_BRAGGING_RIGHTS = "store.brag";

    private static readonly StoreItem[] STORE_ITEMS = {
        // expendables
        new StoreItem(ITEM_KEY_EXTRA_LIFE, "Extra life", 5, "Revive from the game over screen."),
        new StoreItem(ITEM_KEY_NO_ADS_TEMPORARY, "No ads for 24 hours", 10, "Activate immediately (they stack).", 24),
        new StoreItem(ITEM_KEY_RESET_AVERAGE, "Reset average", 25, "Reset from the Stats menu."),
        // unlockables
        new StoreItem(ITEM_KEY_HARD_MODE, "Hard mode", 40, "Toggle when starting a game."),
        new StoreItem(ITEM_KEY_COLORS_PAS_FRU, "Pastel & Fruit", 50, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_KEY_COLORS_WAR_COO, "Warm & Cool", 50, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_KEY_COLORS_MID_WHI, "Midnight & Whiteout", 50, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_KEY_COLORS_RGB_CMY, "RGB & CMYK", 50, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_KEY_BRAGGING_RIGHTS, "Bragging rights", 500, "Brag anywhere (unlocks a secret).")
    };
    private static readonly int numExpendables = 3;

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

    public static void expendItem(string key) {
        int numBought = DataAndSettingsManager.getNumBoughtForStoreItem(key);
        DataAndSettingsManager.setNumBoughtForStoreItem(key, numBought - 1);
    }

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

}

///<summary>A class to organize information about store items.</summary>
public class StoreItem {
    
    private string key;
    private string name;
    private int cost;
    private string description;
    private int lifespanHours;
    /*private bool hasALifespan;
    private TimeSpan lifespan;
    /*private bool shouldTrackPurchaseDate;
    private bool shouldTrackExpirationDate;*/

    public StoreItem(string key, string name, int cost, string description): this(key, name, cost, description, 0) {}
    public StoreItem(string key, string name, int cost, string description, int lifespanHours) {
        this.key = key;
        this.name = name;
        this.cost = cost;
        this.description = description;
        this.lifespanHours = lifespanHours;
    }

    /*public StoreItem(string name, int cost, string description, bool shouldTrackPurchaseDate, bool shouldTrackExpirationDate) {
        this.name = name;
        this.cost = cost;
        this.description = description;
        this.shouldTrackPurchaseDate = shouldTrackPurchaseDate;
        this.shouldTrackExpirationDate = shouldTrackExpirationDate;
    }*/

    public string getKey() { return this.key; }
    public string getName() { return this.name; }
    public int getCost() { return this.cost; }
    public string getDescription() { return this.description; }
    public bool hasLifespan() { return (this.lifespanHours > 0); }
    public int getLifespanHours() { return this.lifespanHours; }
    /*public bool shouldTrackItemPurchaseDate() { return this.shouldTrackPurchaseDate; }
    public bool shouldTrackItemExpirationDate() { return this.shouldTrackExpirationDate; }*/

    public int getNumBought() {
        return DataAndSettingsManager.getNumBoughtForStoreItem(this.key);
    }

}
