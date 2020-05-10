using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePauser : StateChangeListener {

    public GameObject pausedPanel;
    public Text pausedScoreLabel;
    public Text pausedGoldLabel;
    public Button quitButton;
    public GameObject confirmQuitRect;

    public GameObject settingsPanel;

    /* * * * StateChangeListener delegate * * * */

    public override void respondToStateChange(GameState newState) {
        if (newState == GameState.GamePaused) {
            this.enabled = true;
            this.setup();
        }
        else {
            this.enabled = false;
        }
    }

    /* * * * Private methods * * * */

    private void setup() {
        this.switchToPausedAction(); // set the correct panel active
        this.cancelQuitAction(); // set the correct buttons active
        this.setupLabels(GameStateManager.getScore(), GameStateManager.getGoldAmount());
    }

    private void setupLabels(int score, int gold) {
        this.pausedScoreLabel.text = "Score: " + score;
        this.pausedGoldLabel.text = "" + gold;
    }

    /* * * * UI actions * * * */

    public void switchToSettingsAction() {
        this.pausedPanel.SetActive(false);
        this.settingsPanel.SetActive(true);
    }

    public void switchToPausedAction() {
        this.settingsPanel.SetActive(false);
        this.pausedPanel.SetActive(true);
    }

    public void resumeAction() {
        GameStateManager.onGameResume();
    }

    public void attemptQuitAction() {
        this.quitButton.gameObject.SetActive(false);
        this.confirmQuitRect.SetActive(true);
    }

    public void confirmQuitAction() {
        GameStateManager.quitGame();
    }

    public void cancelQuitAction() {
        this.confirmQuitRect.SetActive(false);
        this.quitButton.gameObject.SetActive(true);
    }

}
