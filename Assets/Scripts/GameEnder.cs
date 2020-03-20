using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEnder : StateChangeListener {

    private int score;
    private int scoreBeforeRevive;
    private int highscore;
    private float averageScore;
    private int gold;

    public Text endScoreLabel;
    public Text endHighscoreLabel;
    public Text endAverageScoreLabel;
    public Text endGoldLabel;
    public GameObject reviveButton;
    public Text reviveButtonLabel;

    /* * * * StateChangeListener delegate * * * */

    public override void respondToStateChange(GameState newState) {
        if (newState == GameState.GameOver) {
            this.enabled = true;
            this.endGame();
        }
        else {
            if (newState == GameState.WaitingToStart) {
                this.scoreBeforeRevive = 0;
            }
            this.enabled = false;
        }
    }

    /* * * * UI actions * * * */

    public void reviveAction() {
        GameStateManager.onGameRevive();
        StoreManager.expendItem(StoreManager.ITEM_NAME_EXTRA_LIFE);
    }

    ///<summary>Restart the game from the game over screen. This method is linked to a button on the canvas.</summary>
    public void restartAction() {
        GameStateManager.onGameRestart();
    }

    ///<summary>Quit the game from the game over screen. This method is linked to a button on the canvas.</summary>
    public void quitAction() {
        GameStateManager.quitGame();
    }

    /* * * * Private methods * * * */

    private void endGame() {
        this.saveData();
        this.displayData();
        this.showReviveButtonIfNecessary();
    }

    private void saveData() {
        this.score = GameStateManager.getScore();
        this.highscore = DataAndSettingsManager.getHighscore();
        if (this.score > this.highscore) {
            this.highscore = this.score;
            DataAndSettingsManager.setHighscore(this.highscore);
        }

        float average = DataAndSettingsManager.getAverageScore();
        int numGames = DataAndSettingsManager.getGamesPlayed();
        average = (average * numGames + (this.score - this.scoreBeforeRevive)) / (numGames + 1);
        this.averageScore = average;
        DataAndSettingsManager.setAverageScore(average);
        DataAndSettingsManager.setGamesPlayed(numGames + 1);
        this.scoreBeforeRevive = this.score;

        this.gold = GameStateManager.getGoldAmount();
        DataAndSettingsManager.setGoldAmount(this.gold);
    }

    private void displayData() {
        this.endScoreLabel.text = "" + this.score;
        this.endHighscoreLabel.text = "Highscore: " + this.highscore;
        this.endAverageScoreLabel.text = "Average: " + this.averageScore.ToString("F2"); // two digits after the decimal
        this.endGoldLabel.text = "Gold: " + this.gold;
    }

    private void showReviveButtonIfNecessary() {
        int revivesLeft = DataAndSettingsManager.getNumBoughtForStoreItem(StoreManager.ITEM_NAME_EXTRA_LIFE);
        this.reviveButtonLabel.text = "Revive (" + revivesLeft + ")";
        this.reviveButton.SetActive(revivesLeft > 0 && GameStateManager.canRevive());
    }

}
