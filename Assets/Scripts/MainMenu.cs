using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class MainMenu : MonoBehaviour {

    public GameObject mainPanel;
    public GameObject statsPanel;
    public Text statsGoldLabel;
    public Text statsHighscoreLabel;
    public Text statsAverageLabel;
    public Text statsGamesPlayedLabel;
    public GameObject resetAverageButton;
    public Text resetAverageButtonLabel;
    public StoreMenu storePanel;
    public GameObject settingsPanel;
    public GameObject aboutPanel;

    public GameObject menuCubePrefab;
    private List<GameObject> menuCubes;
    private static readonly float CUBE_SIZE = 0.47f;
    private static readonly int CUBE_START_DISTANCE = 20;
    private static readonly float CUBE_TIME_INTERVAL = 0.4f;

    public GameObject crownImage;

    /* * * * Lifecycle methods * * * */

    void Awake() {
        #if UNITY_IOS
        System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        #endif
        DataAndSettingsManager.loadData();
    }

    void Start() {
        this.menuCubes = new List<GameObject>();
        this.openMainAction(); // switch to main panel
        this.updateStatsMenu();
        this.startMenuCubes();
        FindObjectOfType<AudioManager>().playMusic(AudioManager.MUSIC_MENU);
        StoreManager.updateLifespanItemCounts();
        MobileAds.Initialize(initStatus => {});
    }

    void Update() {
        this.moveCubes();
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

    /* * * * UI actions * * * */

    ///<summary>Changes the scene.</summary>
    public void playAction() {
        SceneManager.LoadSceneAsync("Scenes/ARScene", LoadSceneMode.Single);
    }

    public void openMainAction() {
        this.deactivateAllPanels();
        this.crownImage.SetActive(DataAndSettingsManager.getNumBoughtForStoreItem(StoreManager.ITEM_KEY_BRAGGING_RIGHTS) > 0);
        this.mainPanel.SetActive(true);
    }

    public void openStatsAction() {
        this.deactivateAllPanels();
        this.updateStatsMenu();
        this.statsPanel.SetActive(true);
    }

    public void resetAverageAction() {
        DataAndSettingsManager.resetAverageScore();
        StoreManager.expendItem(StoreManager.ITEM_KEY_RESET_AVERAGE);
        this.updateStatsMenu();
    }

    public void openStoreAction() {
        this.deactivateAllPanels();
        this.storePanel.gameObject.SetActive(true);
    }

    public void openSettingsAction() {
        this.deactivateAllPanels();
        this.settingsPanel.SetActive(true);
    }

    public void openAboutAction() {
        this.deactivateAllPanels();
        this.aboutPanel.SetActive(true);
    }

    ///<summary>Asks the OS to open the link to ZapSplat.</summary>
    public void visitMusicLinkAction() {
        Application.OpenURL("https://www.zapsplat.com/");
    }

    public void restorePurchasesAction() {
        IAPManager.restorePurchases();
    }

    /* * * * Animated background cubes * * * */

    ///<summary>Randomly generates the background cubes.</summary>
    private void startMenuCubes() {
        int placement = 0, previousPlacement = 0;
        for (int i = 0; i < CUBE_START_DISTANCE * 2 + 1; i++) {
            while (placement == previousPlacement) {
                placement = Random.Range(-17, 18);
            }
            GameObject cube = Instantiate(this.menuCubePrefab, new Vector3(CUBE_SIZE * (CUBE_START_DISTANCE + i), 0f, CUBE_SIZE * placement), Quaternion.identity);
            this.assignRandomColor(cube);
            this.menuCubes.Add(cube);
            previousPlacement = placement;
        }
    }

    ///<summary>Sets the albedo color of the given cube's material to a random color.</summary>
    private void assignRandomColor(GameObject cube) {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);
        Color color = new Color(r, g, b, 1f);
        cube.GetComponent<Renderer>().material.color = color;
    }

    ///<summary>Translates all the background cubes the appropriate distance. Should call this in `Update()`.</summary>
    private void moveCubes() {
        float dx = -CUBE_SIZE * Time.deltaTime / CUBE_TIME_INTERVAL;
        foreach (GameObject cube in this.menuCubes) {
            cube.transform.Translate(dx, 0f, 0f);
            if (cube.transform.position.x < CUBE_SIZE * -CUBE_START_DISTANCE) {
                // move the cube back if it is no longer visible
                cube.transform.position = new Vector3(CUBE_SIZE * CUBE_START_DISTANCE, 0f, cube.transform.position.z);
            }
        }
    }

    /* * * * Helper methods * * * */

    private void deactivateAllPanels() {
        this.mainPanel.SetActive(false);
        this.statsPanel.SetActive(false);
        this.storePanel.gameObject.SetActive(false);
        this.settingsPanel.SetActive(false);
        this.aboutPanel.SetActive(false);
    }

    ///<summary>Updates the statistics display.</summary>
    private void updateStatsMenu() {
        this.statsGoldLabel.text = "" + DataAndSettingsManager.getGoldAmount();
        this.statsHighscoreLabel.text = "Highscore: " + DataAndSettingsManager.getHighscore();
        this.statsAverageLabel.text = "Average: " + DataAndSettingsManager.getAverageScore().ToString("F2"); // 2 decimal places
        this.statsGamesPlayedLabel.text = "Games Played: " + DataAndSettingsManager.getGamesPlayed();
        this.showResetAverageButtonIfNecessary();
    }

    private void showResetAverageButtonIfNecessary() {
        int resetsLeft = DataAndSettingsManager.getNumBoughtForStoreItem(StoreManager.ITEM_KEY_RESET_AVERAGE);
        this.resetAverageButtonLabel.text = "Reset average (" + resetsLeft + ")";
        this.resetAverageButton.SetActive(resetsLeft > 0);
    }

}
