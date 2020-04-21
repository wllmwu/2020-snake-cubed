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
    private int goldFromApples;
    private int goldFromApplesBeforeRevive;
    private bool isHardMode;
    private int consecutiveRounds;
    private int consecutiveRevivals;

    public Text endScoreLabel;
    public Text endHighscoreLabel;
    public Text endAverageScoreLabel;
    public Text endGoldLabel;
    public Text appleGoldLabel;
    public GameObject reviveButton;
    public Text reviveButtonLabel;
    public GameObject reviveWithAdButton;
    public AudioManager audioManager;

    private bool shouldShowAds;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private bool shouldReviveFromAd;

    /* * * * Lifecycle methods * * * */

    void Start() {
        // check if "no ads" is active
        this.shouldShowAds = (StoreManager.shouldShowAds() && IAPManager.shouldShowAds());
    }

    /* * * * StateChangeListener delegate * * * */

    public override void respondToStateChange(GameState newState) {
        if (newState == GameState.GameOver) {
            this.enabled = true;
            this.endGame();
        }
        else {
            if (newState == GameState.WaitingToStart) {
                this.scoreBeforeRevive = 0;
                this.goldFromApplesBeforeRevive = 0;
            }
            this.enabled = false;
        }
    }

    /* * * * UI actions * * * */

    public void reviveAction() {
        StoreManager.expendItem(StoreManager.ITEM_KEY_EXTRA_LIFE);
        this.reviveGame();
    }

    public void reviveWithAdAction() {
        //Debug.Log("reviveWithAdAction");
        if (this.rewardedAd.IsLoaded()) {
            this.rewardedAd.Show();
        }
    }

    ///<summary>Restart the game from the game over screen. This method is linked to a button on the canvas.</summary>
    public void restartAction() {
        this.consecutiveRounds++;
        this.consecutiveRevivals = 0;
        GameStateManager.onGameRestart();
    }

    ///<summary>Quit the game from the game over screen. This method is linked to a button on the canvas.</summary>
    public void quitAction() {
        this.interstitialAd.Destroy();
        GameStateManager.quitGame();
    }

    /* * * * Private methods * * * */

    private void endGame() {
        //Debug.Log("endGame");
        this.updateAndSaveData();
        this.displayData();
        this.showReviveButtonsIfNecessary();
        this.showInterstitialAdIfNecessary();
    }

    private void updateAndSaveData() {
        this.score = GameStateManager.getScore();
        this.highscore = DataAndSettingsManager.getHighscore();
        if (this.score > this.highscore) {
            this.highscore = this.score;
            DataAndSettingsManager.setHighscore(this.highscore);
        }

        this.gold = GameStateManager.getGoldAmount();
        this.isHardMode = DataAndSettingsManager.getHardModeState();
        this.goldFromApples = GameStateManager.getApples() / 2 - this.goldFromApplesBeforeRevive;
        int addition = this.goldFromApples;
        if (this.isHardMode) {
            addition = (int)(addition * 1.5);
        }
        this.gold += addition;
        DataAndSettingsManager.setGoldAmount(this.gold);
        this.goldFromApplesBeforeRevive += this.goldFromApples;

        float average = DataAndSettingsManager.getAverageScore();
        int numGames = DataAndSettingsManager.getGamesPlayed();
        if (consecutiveRevivals > 0) {
            average += (float)(this.score - this.scoreBeforeRevive) / numGames;
        }
        else {
            average = (average * numGames + this.score) / (numGames + 1);
            DataAndSettingsManager.setGamesPlayed(numGames + 1);
        }
        this.averageScore = average;
        DataAndSettingsManager.setAverageScore(average);
        this.scoreBeforeRevive = this.score;

        DataAndSettingsManager.writeData();
    }

    private void displayData() {
        this.endScoreLabel.text = "" + this.score;
        this.endHighscoreLabel.text = "Highscore: " + this.highscore;
        this.endAverageScoreLabel.text = "Average: " + this.averageScore.ToString("F2"); // two digits after the decimal
        this.endGoldLabel.text = "Gold: " + this.gold;
        if (this.isHardMode) {
            this.appleGoldLabel.text = "+ 1.5\u00d7" + (this.goldFromApples);
        }
        else {
            this.appleGoldLabel.text = "+ " + (this.goldFromApples);
        }
    }

    private void showReviveButtonsIfNecessary() {
        bool canRevive = (this.consecutiveRevivals < 3 && GameStateManager.canRevive());
        if (canRevive) {
            int revivesLeft = DataAndSettingsManager.getNumBoughtForStoreItem(StoreManager.ITEM_KEY_EXTRA_LIFE);
            this.reviveButtonLabel.text = "Revive (" + revivesLeft + ")";
            this.reviveButton.SetActive(revivesLeft > 0);
        }
        else {
            this.reviveButton.SetActive(false);
        }
        this.reviveWithAdButton.SetActive(canRevive);
    }

    private void showInterstitialAdIfNecessary() {
        //Debug.Log("consecutiveRounds = " + consecutiveRounds);
        if (this.shouldShowAds && this.consecutiveRounds % 2 == 1 && this.interstitialAd.IsLoaded()) { // show an ad every other round
            this.interstitialAd.Show();
        }
    }

    private void reviveGame() {
        //Debug.Log("reviveGame");
        this.consecutiveRounds++;
        this.consecutiveRevivals++;
        GameStateManager.onGameRevive();
    }

    /* * * * Advertisements * * * */

    private void loadInterstitialAd() {
        //Debug.Log("loadInterstitialAd");
        #if UNITY_ANDROID
            string adUnitID = "ca-app-pub-3940256099942544/1033173712";
        #elif UNITY_IOS
            string adUnitID = "ca-app-pub-3940256099942544/4411468910"; // TODO: change unit ids
        #else
            string adUnitID = "unexpected_platform";
        #endif
        this.interstitialAd = new InterstitialAd(adUnitID);
        AdRequest request = new AdRequest.Builder().Build();
        this.interstitialAd.LoadAd(request);

        // subscribe to event handlers that will pause/resume music
        this.interstitialAd.OnAdOpening += this.handleInterstitialAdShown;
        this.interstitialAd.OnAdClosed += this.handleInterstitialAdClosed;
    }

    public void handleInterstitialAdShown(object sender, EventArgs args) {
        this.pauseMusic();
    }

    public void handleInterstitialAdClosed(object sender, EventArgs args) {
        this.resumeMusic();
        this.loadInterstitialAd();
    }

    private void loadRewardedAd() {
        //Debug.Log("loadRewardedAd");
        #if UNITY_ANDROID
            string adUnitID = "ca-app-pub-3940256099942544/5224354917";
        #elif UNITY_IOS
            string adUnitID = "ca-app-pub-3940256099942544/1712485313"; // TODO: change unit ids
        #else
            string adUnitID = "unexpected_platform";
        #endif
        this.rewardedAd = new RewardedAd(adUnitID);
        AdRequest request = new AdRequest.Builder().Build();
        this.rewardedAd.LoadAd(request);

        // subscribe to event handlers that will pause/resume music and revive
        this.rewardedAd.OnAdOpening += this.handleRewardedAdShown;
        this.rewardedAd.OnUserEarnedReward += this.handleRewardedAdEarnedReward;
        this.rewardedAd.OnAdClosed += this.handleRewardedAdClosed;
    }

    public void handleRewardedAdShown(object sender, EventArgs args) {
        this.pauseMusic();
    }

    public void handleRewardedAdEarnedReward(object sender, Reward args) {
        //Debug.Log("earned reward");
        this.shouldReviveFromAd = true; // will revive when ad is closed
    }

    public void handleRewardedAdClosed(object sender, EventArgs args) {
        //Debug.Log("closed rewarded ad");
        this.resumeMusic();
        if (this.shouldReviveFromAd) {
            //Debug.Log("reviving from rewarded ad");
            this.reviveGame();
        }
        this.shouldReviveFromAd = false;
        this.loadRewardedAd();
    }

    /* * * * Helper methods * * * */

    public void loadAds() {
        this.loadInterstitialAd();
        this.loadRewardedAd();
    }

    private void pauseMusic() {
        this.audioManager.pauseMusic(AudioManager.MUSIC_BACKGROUND);
    }

    private void resumeMusic() {
        this.audioManager.playMusic(AudioManager.MUSIC_BACKGROUND);
    }

}
