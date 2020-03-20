using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StylizedToggle : MonoBehaviour {

    public Texture normalOnTexture;
    public Texture colorblindOnTexture;

    // Start is called before the first frame update
    void Start() {
    }
    // Update is called once per frame
    void Update() {
    }

    void OnEnable() {
        DataAndSettingsManager.OnToggleColorblindMode += this.setColorblindMode;
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
