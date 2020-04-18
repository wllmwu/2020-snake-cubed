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
    private static readonly float cubeSize = 0.47f;
    private static readonly int cubeStartDistance = 20;
    private static readonly float timeInterval = 0.4f;

    public GameObject crownImage;

    /* * * * Lifecycle methods * * * */

    void Awake() {
        #if UNITY_IOS
        System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        #endif
        DataAndSettingsManager.loadData();
    }

    // Start is called before the first frame update
    void Start() {
        this.menuCubes = new List<GameObject>();
        this.openMainAction(); // switch to main panel
        this.updateStatsMenu();
        StartCoroutine("generateMenuCubes");
        FindObjectOfType<AudioManager>().playMusic(AudioManager.MUSIC_MENU);
        StoreManager.updateLifespanItemCounts();
        MobileAds.Initialize(initStatus => {});
    }

    // Update is called once per frame
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

    ///<summary>Changes the scene. This method is linked to the Play button on the canvas.</summary>
    public void playAction() {
        SceneManager.LoadSceneAsync("Scenes/ARScene", LoadSceneMode.Single);
    }

    public void openMainAction() {
        this.deactivateAllPanels();
        this.crownImage.SetActive(DataAndSettingsManager.getNumBoughtForStoreItem(StoreManager.ITEM_KEY_BRAGGING_RIGHTS) > 0);
        this.mainPanel.SetActive(true);
    }

    ///<summary>This method is linked to the Stats button on the canvas.</summary>
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

    ///<summary>This method is linked to the Store button on the canvas.</summary>
    public void openStoreAction() {
        this.deactivateAllPanels();
        this.storePanel.gameObject.SetActive(true);
    }

    ///<summary>This method is linked to the Settings button on the canvas.</summary>
    public void openSettingsAction() {
        this.deactivateAllPanels();
        this.settingsPanel.SetActive(true);
    }

    public void openAboutAction() {
        this.deactivateAllPanels();
        this.aboutPanel.SetActive(true);
    }

    public void visitMusicLinkAction() {
        Application.OpenURL("https://www.zapsplat.com/");
    }

    public void restorePurchasesAction() {
        IAPManager.restorePurchases();
    }

    /* * * * Animated background cubes * * * */

    ///<summary>Instantiates a new menu cube at a random placement along the starting line every `this.timeInterval` seconds.</summary>
    private IEnumerator generateMenuCubes() {
        int placement = 0, previousPlacement = 0;
        while (true) {
            while (placement == previousPlacement) {
                placement = Random.Range(-17, 17);
            }
            GameObject cube = Instantiate(this.menuCubePrefab, new Vector3(cubeSize * cubeStartDistance, 0f, cubeSize * placement), Quaternion.identity);
            this.assignRandomColor(cube);
            this.menuCubes.Add(cube);
            previousPlacement = placement;
            yield return new WaitForSeconds(timeInterval);
        }
    }

    private void assignRandomColor(GameObject cube) {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);
        Color color = new Color(r, g, b, 1f);
        cube.GetComponent<Renderer>().material.color = color;
    }

    ///<summary>Moves all existing cubes in the -x direction, and destroys any that have moved off the screen.</summary>
    private void moveCubes() {
        float dx = -cubeSize / timeInterval * Time.deltaTime;
        for (int i = 0; i < this.menuCubes.Count; i++) {
            GameObject cube = this.menuCubes[i];
            cube.transform.Translate(dx, 0f, 0f);
            if (cube.transform.position.x < cubeSize * -20) {
                // destroy the cube if it is no longer visible
                this.menuCubes.RemoveAt(i);
                Destroy(cube);
                i--;
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
