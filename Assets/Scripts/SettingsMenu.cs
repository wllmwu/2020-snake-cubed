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
    public Toggle musicToggle;
    public Toggle soundsToggle;

    void OnEnable() {
        this.closeColorSchemeListAction(); // switch to the correct panel
        this.colorblindModeToggle.isOn = DataAndSettingsManager.getColorblindModeState();
        this.smoothMovementToggle.isOn = DataAndSettingsManager.getSmoothMovementState();
        this.musicToggle.isOn = DataAndSettingsManager.getMusicEnabledState();
        this.soundsToggle.isOn = DataAndSettingsManager.getSoundsEnabledState();
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

    ///<summary>This method is linked to the Music settings toggle.</summary>
    public void toggleMusicAction() {
        DataAndSettingsManager.setMusicEnabledState(this.musicToggle.isOn);
        FindObjectOfType<AudioManager>().setMusicEnabled(this.musicToggle.isOn);
    }

    ///<summary>This method is linked to the Sound Effects settings toggle.</summary>
    public void toggleSoundEffectsAction() {
        DataAndSettingsManager.setSoundsEnabledState(this.soundsToggle.isOn);
        FindObjectOfType<AudioManager>().setSoundsEnabled(this.soundsToggle.isOn);
    }

    /* * * * Helper methods * * * */

    private void updateColorSchemeLabel() {
        int id = DataAndSettingsManager.getColorSchemeID();
        this.colorSchemeLabel.text = ColorSchemesManager.getColorSchemeWithID(id).getName();
    }

}
