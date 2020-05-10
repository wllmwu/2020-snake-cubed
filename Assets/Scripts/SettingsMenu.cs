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

    public void toggleColorblindModeAction() {
        DataAndSettingsManager.setColorblindModeState(this.colorblindModeToggle.isOn);
    }

    public void openColorSchemeListAction() {
        this.settingsMenuPanel.SetActive(false);
        this.colorSchemePanel.SetActive(true);
    }

    public void closeColorSchemeListAction() {
        this.colorSchemePanel.SetActive(false);
        this.updateColorSchemeLabel();
        this.settingsMenuPanel.SetActive(true);
    }

    public void toggleSmoothMovementAction() {
        DataAndSettingsManager.setSmoothMovementState(this.smoothMovementToggle.isOn);
    }

    public void toggleMusicAction() {
        DataAndSettingsManager.setMusicEnabledState(this.musicToggle.isOn);
        FindObjectOfType<AudioManager>().setMusicEnabled(this.musicToggle.isOn);
    }

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
