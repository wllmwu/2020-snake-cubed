using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class GameEnder : StateChangeListener {

    private int score;
    private int scoreBeforeRevive;
    private int highscore;
    private float averageScore;
    private int gold;
    private int consecutiveRounds;

    public Text endScoreLabel;
    public Text endHighscoreLabel;
    public Text endAverageScoreLabel;
    public Text endGoldLabel;
    public GameObject reviveButton;
    public Text reviveButtonLabel;
    public AudioManager audioManager;

    private InterstitialAd interstitialAd;

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
        this.consecutiveRounds++;
        GameStateManager.onGameRevive();
        StoreManager.expendItem(StoreManager.ITEM_NAME_EXTRA_LIFE);
    }

    public void reviveWithAdAction() {
        //
    }

    ///<summary>Restart the game from the game over screen. This method is linked to a button on the canvas.</summary>
    public void restartAction() {
        this.consecutiveRounds++;
        GameStateManager.onGameRestart();
    }

    ///<summary>Quit the game from the game over screen. This method is linked to a button on the canvas.</summary>
    public void quitAction() {
        this.interstitialAd.Destroy();
        GameStateManager.quitGame();
    }

    /* * * * Private methods * * * */

    private void endGame() {
        this.saveData();
        this.displayData();
        this.showReviveButtonIfNecessary();
        this.showInterstitialIfNecessary();
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
        average = (average * numGames + (this.score - this.scoreBeforeRevive)) / (numGames + 1); // only count points earned this round (disregard points from before revival)
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

    private void showInterstitialIfNecessary() {
        if (this.consecutiveRounds % 2 == 1 && this.interstitialAd.IsLoaded()) { // show an ad every other round
            this.interstitialAd.Show();
        }
    }

    /* * * * Advertisements * * * */

    public void loadInterstitial() {
        #if UNITY_ANDROID
            string adUnitID = "ca-app-pub-3940256099942544/1033173712";
        #elif UNITY_IPHONE
            string adUnitID = "ca-app-pub-3940256099942544/4411468910"; // TODO: change unit ids
        #else
            string adUnitID = "unexpected_platform";
        #endif
        this.interstitialAd = new InterstitialAd(adUnitID);
        AdRequest request = new AdRequest.Builder().Build();
        this.interstitialAd.LoadAd(request);

        // subscribe to event handlers that will pause/resume music
        this.interstitialAd.OnAdOpening += this.handleAdShown;
        this.interstitialAd.OnAdClosed += this.handleAdClosed;
    }

    public void handleAdShown(object sender, EventArgs args) {
        this.audioManager.pauseMusic(AudioManager.MUSIC_BACKGROUND);
    }

    public void handleAdClosed(object sender, EventArgs args) {
        this.audioManager.playMusic(AudioManager.MUSIC_BACKGROUND);
        this.loadInterstitial();
    }

}
