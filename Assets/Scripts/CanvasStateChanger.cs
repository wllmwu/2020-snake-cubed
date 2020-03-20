using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasStateChanger : StateChangeListener {

    /* These booleans determine whether the listening Canvas's GameObject should activate/deactivate upon each change in game state.
     * They should be set for each listening Canvas via the editor.
     * They should match the cases of the GameStateManager.GameState enum. */
    public bool activeForSettingPosition;
    public bool activeForWaitingToStart;
    public bool activeForGameRunning;
    public bool activeForGamePaused;
    public bool activeForGameOver;

    /* * * * StateChangeListener delegate * * * */

    public override void respondToStateChange(GameState newState) {
        switch (newState) {
            case GameState.SettingPosition:
                this.gameObject.SetActive(this.activeForSettingPosition);
                break;
            case GameState.WaitingToStart:
                this.gameObject.SetActive(this.activeForWaitingToStart);
                break;
            case GameState.GameRunning:
                this.gameObject.SetActive(this.activeForGameRunning);
                break;
            case GameState.GamePaused:
                this.gameObject.SetActive(this.activeForGamePaused);
                break;
            case GameState.GameOver:
                this.gameObject.SetActive(this.activeForGameOver);
                break;
            default:
                this.gameObject.SetActive(false);
                break;
        }
    }

}
