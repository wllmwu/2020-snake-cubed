using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour {

    /* * * * Names of expendable items * * * */
    public static readonly string ITEM_NAME_EXTRA_LIFE = "Extra life";
    //public static readonly string ITEM_NAME_NO_ADS_TEMPORARY = "No ads - 24 hours";
    public static readonly string ITEM_NAME_RESET_AVERAGE = "Reset average";

    /* * * * Names of unlockable items * * * */
    public static readonly string ITEM_NAME_HARD_MODE = "Hard mode";
    public static readonly string ITEM_NAME_COLORS_PAS_FRU = "Pastel & Fruit";
    public static readonly string ITEM_NAME_COLORS_WAR_COO = "Warm & Cool";
    public static readonly string ITEM_NAME_COLORS_MID_WHI = "Midnight & Whiteout";
    public static readonly string ITEM_NAME_COLORS_RGB_CMY = "RGB & CMYK";
    public static readonly string ITEM_NAME_BRAGGING_RIGHTS = "Bragging rights";

    private static readonly StoreItem[] STORE_ITEMS = {
        // expendables
        new StoreItem(ITEM_NAME_EXTRA_LIFE, 4, "Revive from the game over screen."),
        //new StoreItem(ITEM_NAME_NO_ADS_TEMPORARY, 12, "24 hours of peace! (Activates immediately.)"),
        new StoreItem(ITEM_NAME_RESET_AVERAGE, 25, "Reset from the Stats menu."),
        // unlockables
        new StoreItem(ITEM_NAME_HARD_MODE, 50, "Toggle when starting a game."),
        new StoreItem(ITEM_NAME_COLORS_PAS_FRU, 60, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_NAME_COLORS_WAR_COO, 60, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_NAME_COLORS_MID_WHI, 60, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_NAME_COLORS_RGB_CMY, 60, "Find these new color schemes in Settings."),
        new StoreItem(ITEM_NAME_BRAGGING_RIGHTS, 500, "Brag anywhere (unlocks a secret).")
    };
    private static readonly int numExpendables = 2;

    /* * * * Public methods * * * */

    public static int getNumExpendables() {
        return numExpendables;
    }

    public static int getNumItems() {
        return STORE_ITEMS.Length;
    }

    public static StoreItem getItemWithID(int id) {
        return STORE_ITEMS[id];
    }

    public static bool buyItem(int id) {
        StoreItem item = STORE_ITEMS[id];
        string name = item.getName();
        int numBought = DataAndSettingsManager.getNumBoughtForStoreItem(name);
        int gold = DataAndSettingsManager.getGoldAmount();
        if (gold >= item.getCost() && (id < getNumExpendables() || numBought < 1)) {
            DataAndSettingsManager.setNumBoughtForStoreItem(name, numBought + 1);
            DataAndSettingsManager.setGoldAmount(gold -= item.getCost());
            return true;
        }
        return false;
    }

    public static void expendItem(string name) {
        int numBought = DataAndSettingsManager.getNumBoughtForStoreItem(name);
        DataAndSettingsManager.setNumBoughtForStoreItem(name, numBought - 1);
    }

}

///<summary>A class to organize information about store items.</summary>
public class StoreItem {
    
    private string name;
    private int cost;
    private string description;

    public StoreItem(string name, int cost, string description) {
        this.name = name;
        this.cost = cost;
        this.description = description;
    }

    public string getName() { return this.name; }
    public int getCost() { return this.cost; }
    public string getDescription() { return this.description; }

    public int getNumBought() {
        return DataAndSettingsManager.getNumBoughtForStoreItem(this.name);
    }

}
