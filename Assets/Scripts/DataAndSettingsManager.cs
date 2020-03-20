using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class DataAndSettingsManager : MonoBehaviour {

    /* * * * Keys for player data * * * */

    ///<summary>The key for the highscore, stored by PlayerPrefs.</summary>
    private static readonly string KEY_HIGHSCORE = "highscore";
    ///<summary>The key for the average score, stored by PlayerPrefs.</summary>
    private static readonly string KEY_AVERAGE_SCORE = "average";
    ///<summary>The key for the number of games stored, by PlayerPrefs.</summary>
    private static readonly string KEY_GAMES_PLAYED = "games";
    ///<summary>The key for the amount of gold, stored by PlayerPrefs.</summary>
    private static readonly string KEY_GOLD_AMOUNT = "gold";

    /* * * * Keys for player settings * * * */

    ///<summary>The key for hard mode state, stored by PlayerPrefs.</summary>
    private static readonly string KEY_HARD_MODE = "hardmode";
    ///<summary>The key for colorblind mode persistent on/off state, stored by PlayerPrefs.</summary>
    private static readonly string KEY_COLORBLIND_MODE = "colorblind";
    ///<summary>The key for the current color scheme ID, stored by PlayerPrefs.</summary>
    private static readonly string KEY_COLOR_SCHEME_ID = "colorscheme";
    /*private static readonly string PATH_CUSTOM_COLOR_SCHEME_FILE = "playercustomcs.s3d";*/
    ///<summary>The key for smooth movement persistent on/off state, stored by PlayerPrefs.</summary>
    private static readonly string KEY_SMOOTH_MOVEMENT = "smooth";
    ///<summary>The key for sound effects persistent on/off state, stored by PlayerPrefs.</summary>
    private static readonly string KEY_SOUND_EFFECTS = "sounds";
    ///<summary>The key for music persistent on/off state, stored by PlayerPrefs.</summary>
    private static readonly string KEY_MUSIC = "music";

    public delegate void SetColorblindMode(bool isOn);
    public static event SetColorblindMode OnToggleColorblindMode;

    /* * * * Lifecycle methods * * * */

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }

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

    public static int getNumBoughtForStoreItem(string name) { return retrieveInt(name, 0); }
    public static void setNumBoughtForStoreItem(string name, int value) { saveInt(name, value); }

    /* * * * Public methods * * * */

    public static void resetAverageScore() {
        setAverageScore(0f);
        setGamesPlayed(0);
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

    private static void triggerColorblindModeDelegate(bool isOn) {
        if (OnToggleColorblindMode != null) {
            OnToggleColorblindMode(isOn);
        }
    }

    /* * * * Save/retrieve functions * * * */

    private static int retrieveInt(string key, int defaultValue) {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    private static void saveInt(string key, int value) {
        PlayerPrefs.SetInt(key, value);
    }

    private static float retrieveFloat(string key, float defaultValue) {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }
    private static void saveFloat(string key, float value) {
        PlayerPrefs.SetFloat(key, value);
    }

    private static bool retrieveBool(string key, bool defaultValue) {
        return PlayerPrefs.GetInt(key, (defaultValue ? 1 : 0)) != 0;
    }
    private static void saveBool(string key, bool value) {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

}
