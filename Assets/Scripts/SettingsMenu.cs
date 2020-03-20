using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    public GameObject settingsMenuPanel;
    public Toggle colorblindModeToggle;
    public Text colorSchemeLabel;
    public GameObject colorSchemePanel;
    public Toggle smoothMovementToggle;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }

    void OnEnable() {
        this.closeColorSchemeListAction(); // switch to the correct panel
        this.colorblindModeToggle.isOn = DataAndSettingsManager.getColorblindModeState();
        this.smoothMovementToggle.isOn = DataAndSettingsManager.getSmoothMovementState();
    }

    /* * * * UI actions * * * */

    ///<summary>This method is linked to the Colorblind Mode settings toggle.</summary>
    public void toggleColorblindModeAction() {
        DataAndSettingsManager.setColorblindModeState(this.colorblindModeToggle.isOn);
    }

    ///<summary>This method is linked to the Color Scheme button.</summary>
    public void openColorSchemeListAction() {
        this.settingsMenuPanel.SetActive(false);
        this.colorSchemePanel.SetActive(true);
    }

    public void closeColorSchemeListAction() {
        this.colorSchemePanel.SetActive(false);
        this.updateColorSchemeLabel();
        this.settingsMenuPanel.SetActive(true);
    }

    ///<summary>This method is linked to the Smooth Movement settings toggle.</summary>
    public void toggleSmoothMovementAction() {
        DataAndSettingsManager.setSmoothMovementState(this.smoothMovementToggle.isOn);
    }

    ///<summary>This method is linked to the Sound Effects settings toggle.</summary>
    public void toggleSoundEffectsAction() {
        //
    }

    ///<summary>This method is linked to the Music settings toggle.</summary>
    public void toggleMusicAction() {
        //
    }

    /* * * * Helper methods * * * */

    private void updateColorSchemeLabel() {
        int id = DataAndSettingsManager.getColorSchemeID();
        this.colorSchemeLabel.text = ColorSchemesManager.getColorSchemeWithID(id).getName();
    }

}
