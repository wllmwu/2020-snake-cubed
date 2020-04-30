using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {
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

    // Start is called before the first frame update
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

    public static int getScore() { return gameRunner.getScore(); }
    public static int getApples() { return gameRunner.getApples(); }
    public static int getGoldAmount() { return gameRunner.getGoldAmount(); }
    public static bool canRevive() { return gameRunner.canRevive(); }

    /* * * * Changing game states * * * */

    private static void setState(GameState newState) {
        if (OnGameStateChange != null) {
            OnGameStateChange(newState);
            previousState = currentState;
            currentState = newState;
        }
    }

    public static void onInitialize() {
        // game setup
        setState(GameState.SettingPosition);
    }

    public static void onPositionSet() {
        // position has been chosen
        setState(GameState.WaitingToStart);
    }

    public static void onPositionCancel() {
        // position is being changed
        setState(GameState.SettingPosition);
    }

    public static void onTutorialStart() {
        // tutorial has begun
        setState(GameState.TutorialRunning);
    }

    public static void onGameStart() {
        // game has begun
        setState(GameState.GameRunning);
    }

    public static void onGamePause() {
        // game has been paused
        setState(GameState.GamePaused);
    }

    public static void onGameResume() {
        // game has resumed from pause
        setState(GameState.GameRunning);
    }

    public static void onGameEnd() {
        // game has ended
        setState(GameState.GameOver);
    }

    public static void onGameRestart() {
        // game has reset (position is still the same)
        setState(GameState.WaitingToStart);
    }

    public static void onGameRevive() {
        gameRunner.reviveGame(); // GameRunner will call onGameStart()
    }

    /* * * * Quitting the game * * * */

    public static void quitGame() {
        removeAllListeners();
        SceneManager.LoadScene("Scenes/MainMenu", LoadSceneMode.Single);
    }

    private static void removeAllListeners() {
        OnGameStateChange = null;
    }

}
