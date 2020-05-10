using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class DataAndSettingsManager {

    // player data keys - DO NOT CHANGE
    private static readonly string KEY_HIGHSCORE = "stats.highscore";
    private static readonly string KEY_AVERAGE_SCORE = "stats.average";
    private static readonly string KEY_GAMES_PLAYED = "stats.games";
    private static readonly string KEY_GOLD_AMOUNT = "stats.gold";

    // settings keys - DO NOT CHANGE
    private static readonly string KEY_HARD_MODE = "settings.hardmode";
    private static readonly string KEY_COLORBLIND_MODE = "settings.colorblind";
    private static readonly string KEY_COLOR_SCHEME_ID = "settings.colorscheme";
    /*private static readonly string PATH_CUSTOM_COLOR_SCHEME_FILE = "playercustomcs.s3d";*/
    private static readonly string KEY_SMOOTH_MOVEMENT = "settings.smooth";
    private static readonly string KEY_MUSIC = "settings.music";
    private static readonly string KEY_SOUND_EFFECTS = "settings.sounds";

    private static readonly string SAVE_PATH = "/save.dat"; // DO NOT CHANGE
    private static Dictionary<string, int> intData = new Dictionary<string, int>();
    private static Dictionary<string, float> floatData = new Dictionary<string, float>();
    private static Dictionary<string, bool> boolData = new Dictionary<string, bool>();
    private static Dictionary<string, string> stringData = new Dictionary<string, string>();
    ///<summary>Flag for whether data has been read from disk.</summary>
    private static bool didLoad;
    ///<summary>Flag for whether any data has been changed since the last write to disk.</summary>
    private static bool changedSinceLastWrite;

    public delegate void SetColorblindMode(bool isOn);
    public static event SetColorblindMode OnToggleColorblindMode;

    /* * * * Public getters and setters * * * */

    public static int getHighscore() { return retrieveInt(KEY_HIGHSCORE, 0); }
    public static void setHighscore(int highscore) { saveInt(KEY_HIGHSCORE, highscore); }

    public static float getAverageScore() { return retrieveFloat(KEY_AVERAGE_SCORE, 0f); }
    public static void setAverageScore(float average) { saveFloat(KEY_AVERAGE_SCORE, average); }

    public static int getGamesPlayed() { return retrieveInt(KEY_GAMES_PLAYED, 0); }
    public static void setGamesPlayed(int games) { saveInt(KEY_GAMES_PLAYED, games); }

    public static int getGoldAmount() { return retrieveInt(KEY_GOLD_AMOUNT, 0); }
    public static void setGoldAmount(int gold) { saveInt(KEY_GOLD_AMOUNT, gold); }

    public static bool getHardModeState() { return retrieveBool(KEY_HARD_MODE, false); }
    public static void setHardModeState(bool isOn) { saveBool(KEY_HARD_MODE, isOn); }

    public static bool getColorblindModeState() { return retrieveBool(KEY_COLORBLIND_MODE, false); }
    public static void setColorblindModeState(bool isOn) {
        saveBool(KEY_COLORBLIND_MODE, isOn);
        triggerColorblindModeDelegate(isOn);
    }

    public static int getColorSchemeID() { return retrieveInt(KEY_COLOR_SCHEME_ID, 0); }
    public static void setColorSchemeID(int id) { saveInt(KEY_COLOR_SCHEME_ID, id); }

    public static bool getSmoothMovementState() { return retrieveBool(KEY_SMOOTH_MOVEMENT, true); }
    public static void setSmoothMovementState(bool isOn) { saveBool(KEY_SMOOTH_MOVEMENT, isOn); }

    public static bool getMusicEnabledState() { return retrieveBool(KEY_MUSIC, true); }
    public static void setMusicEnabledState(bool isOn) { saveBool(KEY_MUSIC, isOn); }

    public static bool getSoundsEnabledState() { return retrieveBool(KEY_SOUND_EFFECTS, true); }
    public static void setSoundsEnabledState(bool isOn) { saveBool(KEY_SOUND_EFFECTS, isOn); }

    public static int getNumBoughtForStoreItem(string itemKey) { return retrieveInt(itemKey, 0); }
    public static void setNumBoughtForStoreItem(string itemKey, int numBought) { saveInt(itemKey, numBought); }

    public static DateTime getExpirationDateForStoreItem(string itemKey) {
        return retrieveDate(itemKey + ".expire"); // DO NOT CHANGE the key
    }
    public static void setExpirationDateForStoreItem(string itemKey, DateTime date) {
        saveDate(itemKey + ".expire", date); // DO NOT CHANGE the key
    }

    /* * * * Public methods * * * */

    ///<summary>Sets the average score and number of games played to 0.</summary>
    public static void resetAverageScore() {
        setAverageScore(0f);
        setGamesPlayed(0);
    }

    ///<summary>Reads game data from disk storage and loads it into memory, if this hasn't been done already.</summary>
    public static void loadData() {
        if (!didLoad) {
            string path = Application.persistentDataPath + SAVE_PATH;
            if (File.Exists(path)) {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);
                SaveData load = formatter.Deserialize(stream) as SaveData; // if the cast fails, then load is null
                if (load != null) {
                    intData = load.getIntData();
                    floatData = load.getFloatData();
                    boolData = load.getBoolData();
                    stringData = load.getStringData();
                }
                stream.Close();
                changedSinceLastWrite = false; // any previous data was overwritten
            }
        }
        didLoad = true;
    }

    ///<summary>Writes current game data to disk storage, if changes were made since the last time data was written.</summary>
    public static void writeData() {
        if (changedSinceLastWrite) {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = Application.persistentDataPath + SAVE_PATH;
            FileStream stream = new FileStream(path, FileMode.Create);
            SaveData save = new SaveData(intData, floatData, boolData, stringData, 0);
            formatter.Serialize(stream, save);
            stream.Close();
        }
        changedSinceLastWrite = false;
    }

    // UNUSED
    /*public static void savePlayerCustomColorScheme() {
        ColorScheme customScheme = ColorSchemesManager.getPlayerCustomScheme();
        if (customScheme != null) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Path.Combine(Application.persistentDataPath, PATH_CUSTOM_COLOR_SCHEME_FILE));
            bf.Serialize(file, customScheme);
            file.Close();
        }
    }

    public static ColorScheme retrievePlayerCustomColorScheme() {
        if (File.Exists(Path.Combine(Application.persistentDataPath, PATH_CUSTOM_COLOR_SCHEME_FILE))) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Path.Combine(Application.persistentDataPath, PATH_CUSTOM_COLOR_SCHEME_FILE), FileMode.Open);
            ColorScheme customScheme = (ColorScheme)bf.Deserialize(file);
            file.Close();
            return customScheme;
        }
        else {
            return new ColorScheme(
                "Custom",
                new Color(0f, 0f, 0f, 178/255f),
                Color.white,
                Color.red,
                Color.green,
                Color.blue
            );
        }
    }*/

    /* * * * Delegate callers * * * */

    public static void updateColorblindModeListeners() {
        triggerColorblindModeDelegate(getColorblindModeState());
    }

    ///<summary>Triggers the `OnToggleColorblindMode` event and passes in `isOn`.</summary>
    private static void triggerColorblindModeDelegate(bool isOn) {
        if (OnToggleColorblindMode != null) {
            OnToggleColorblindMode(isOn);
        }
    }

    /* * * * Private save/retrieve functions * * * */

    private static int retrieveInt(string key, int defaultValue) {
        int value;
        if (intData.TryGetValue(key, out value)) { // value will contain the corresponding value if true, or the default int value
            return value;
        }
        return defaultValue;
    }
    private static void saveInt(string key, int value) {
        intData[key] = value;
        changedSinceLastWrite = true;
    }

    private static float retrieveFloat(string key, float defaultValue) {
        float value;
        if (floatData.TryGetValue(key, out value)) { // value will contain the corresponding value if true, or the default float value
            return value;
        }
        return defaultValue;
    }
    private static void saveFloat(string key, float value) {
        floatData[key] = value;
        changedSinceLastWrite = true;
    }

    private static bool retrieveBool(string key, bool defaultValue) {
        bool value;
        if (boolData.TryGetValue(key, out value)) { // value will contain the corresponding value if true, or the default bool value
            return value;
        }
        return defaultValue;
    }
    private static void saveBool(string key, bool value) {
        boolData[key] = value;
        changedSinceLastWrite = true;
    }

    private static DateTime retrieveDate(string key) {
        string time = retrieveString(key, "");
        DateTime result;
        DateTime.TryParse(time, out result); // result is equal to DateTime.MinValue if the conversion fails
        return result;
    }
    private static void saveDate(string key, DateTime value) {
        saveString(key, value.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    private static string retrieveString(string key, string defaultValue) {
        string value;
        if (stringData.TryGetValue(key, out value)) { // value will contain the corresponding value if true, or the default string value
            return value;
        }
        return defaultValue;
    }
    private static void saveString(string key, string value) {
        stringData[key] = value;
        changedSinceLastWrite = true;
    }

}

///<summary>Wrapper class for game data, to be serialized with BinaryFormatter.</summary>
[System.Serializable]
class SaveData {

    // WARNING: do not change this class definition unless you make the BinaryFormatter deserialization handle the difference
    [SerializeField]
    private Dictionary<string, int> intData;
    [SerializeField]
    private Dictionary<string, float> floatData;
    [SerializeField]
    private Dictionary<string, bool> boolData;
    [SerializeField]
    private Dictionary<string, string> stringData;
    [SerializeField]
    private int number;

    public SaveData(Dictionary<string, int> ints, Dictionary<string, float> floats, Dictionary<string, bool> bools,
        Dictionary<string, string> strings, int number) {
        this.intData = ints;
        this.floatData = floats;
        this.boolData = bools;
        this.stringData = strings;
        this.number = number;
    }

    public Dictionary<string, int> getIntData() { return this.intData; }
    public Dictionary<string, float> getFloatData() { return this.floatData; }
    public Dictionary<string, bool> getBoolData() { return this.boolData; }
    public Dictionary<string, string> getStringData() { return this.stringData; }

}
