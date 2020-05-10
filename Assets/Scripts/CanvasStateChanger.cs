using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>A subclass of `StateChangeListener` for Canvases and other UI objects.
/// Add this script to the objects as a component, then set the `activeFor*` flags accordingly.</summary>
public class CanvasStateChanger : StateChangeListener {

    /* These booleans determine whether the listening Canvas's GameObject should activate/deactivate upon each change in game state.
     * They should be set for each listening Canvas in the editor.
     * They should match the cases of the GameStateManager.GameState enum. */
    public bool activeForSettingPosition;
    public bool activeForWaitingToStart;
    public bool activeForTutorialRunning;
    public bool activeForGameRunning;
    public bool activeForGamePaused;
    public bool activeForGameOver;

    /* * * * StateChangeListener delegate * * * */

    ///<summary>Sets `this.gameObject` active or inactive based on the `activeFor*` flags.</summary>
    public override void respondToStateChange(GameState newState) {
        switch (newState) {
            case GameState.SettingPosition:
                this.gameObject.SetActive(this.activeForSettingPosition);
                break;
            case GameState.WaitingToStart:
                this.gameObject.SetActive(this.activeForWaitingToStart);
                break;
            case GameState.TutorialRunning:
                this.gameObject.SetActive(this.activeForTutorialRunning);
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
