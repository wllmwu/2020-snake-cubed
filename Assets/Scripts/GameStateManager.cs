using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { // make sure these match with CanvasStateChanger
    SettingPosition,
    WaitingToStart,
    TutorialRunning,
    GameRunning,
    GamePaused,
    GameOver
}

public class GameStateManager : MonoBehaviour {

    public delegate void StateChangeResponder(GameState newState);
    public static event StateChangeResponder OnGameStateChange;

    private static GameState currentState;
    private static GameState previousState;

    private static GameStarter gameStarter;
    private static GameRunner gameRunner;
    private static GameEnder gameEnder;

    void Start() {
        gameStarter = GetComponent<GameStarter>();
        gameRunner = GetComponent<GameRunner>();
        gameEnder = GetComponent<GameEnder>();
        ColorSchemesManager.setColorScheme(DataAndSettingsManager.getColorSchemeID());
        gameEnder.loadAds();
        onInitialize();
    }

    void OnApplicationPause(bool pauseStatus) {
        // this method is called when the app is soft-closed on iOS and Android
        if (pauseStatus) {
            DataAndSettingsManager.writeData();
        }
    }

    void OnDestroy() {
        DataAndSettingsManager.writeData();
    }

    /* * * * Public getters * * * */

    public static GameState getCurrentState() { return currentState; }
    public static GameState getPreviousState() { return previousState; }

    ///<summary>Returns the total score so far in the current game.</summary>
    public static int getScore() { return gameRunner.getScore(); }
    ///<summary>Returns the number of apples collected so far in the current game.</summary>
    public static int getApples() { return gameRunner.getApples(); }
    ///<summary>Returns the total amount of gold the player should have, including gold from before the current game.
    /// This value is likely different from the value tracked by `DataAndSettingsManager`,
    /// because it is not saved and written until a game ends.</summary>
    public static int getGoldAmount() { return gameRunner.getGoldAmount(); }
    ///<summary>Returns whether the snake is in a position it can revive from.</summary>
    public static bool canRevive() { return gameRunner.canRevive(); }

    /* * * * Changing game states * * * */

    ///<summary>Triggers the `OnGameStateChange` event and updates `previousState` and `currentState`.</summary>
    private static void setState(GameState newState) {
        if (OnGameStateChange != null) {
            OnGameStateChange(newState);
            previousState = currentState;
            currentState = newState;
        }
    }

    ///<summary>Should call when setting position.</summary>
    public static void onInitialize() {
        setState(GameState.SettingPosition);
    }

    ///<summary>Should call when position has been chosen.</summary>
    public static void onPositionSet() {
        setState(GameState.WaitingToStart);
    }

    ///<summary>Should call when going back to setting position.</summary>
    public static void onPositionCancel() {
        setState(GameState.SettingPosition);
    }

    ///<summary>Should call when the user starts the tutorial.</summary>
    public static void onTutorialStart() {
        setState(GameState.TutorialRunning);
    }

    ///<summary>Should call when the user starts the game.</summary>
    public static void onGameStart() {
        setState(GameState.GameRunning);
    }

    ///<summary>Should call when the user pauses the game.</summary>
    public static void onGamePause() {
        setState(GameState.GamePaused);
    }

    ///<summary>Should call when the user resumes the game from pause.</summary>
    public static void onGameResume() {
        setState(GameState.GameRunning);
    }

    ///<summary>Should call when the game ends (snake hits something).</summary>
    public static void onGameEnd() {
        setState(GameState.GameOver);
    }

    ///<summary>Should call when the user restarts from the game over screen.</summary>
    public static void onGameRestart() {
        setState(GameState.WaitingToStart);
    }

    ///<summary>Should call when the user revives from the game over screen.</summary>
    public static void onGameRevive() {
        gameRunner.reviveGame(); // GameRunner will call onGameStart()
    }

    /* * * * Quitting the game * * * */

    ///<summary>Removes all `OnGameStateChange` event listeners and changes the scene to the main menu.</summary>
    public static void quitGame() {
        removeAllListeners();
        SceneManager.LoadScene("Scenes/MainMenu", LoadSceneMode.Single);
    }

    private static void removeAllListeners() {
        OnGameStateChange = null;
    }

}
