using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StylizedToggle : MonoBehaviour {

    public Texture normalOnTexture;
    public Texture colorblindOnTexture;

    void OnEnable() {
        DataAndSettingsManager.OnToggleColorblindMode += this.setColorblindMode;
        this.setColorblindMode(DataAndSettingsManager.getColorblindModeState());
    }
    void OnDisable() {
        DataAndSettingsManager.OnToggleColorblindMode -= this.setColorblindMode;
    }

    ///<summary>The delegate method for `DataAndSettingsManager.OnToggleColorblindMode`.</summary>
    public void setColorblindMode(bool isOn) {
        Toggle toggle = GetComponent<Toggle>();
        if (toggle == null) { return; }
        RawImage image = (RawImage) toggle.graphic;
        if (image == null) { return; }

        if (isOn) {
            image.texture = this.colorblindOnTexture;
        }
        else {
            image.texture = this.normalOnTexture;
        }
    }

}
