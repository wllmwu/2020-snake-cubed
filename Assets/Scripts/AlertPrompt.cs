using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertPrompt : MonoBehaviour {

    public Text messageLabel;

    // Start is called before the first frame update
    void Start() {
        this.closeAction();
    }

    public void showMessage(string message) {
        this.messageLabel.text = message;
        this.setVisible(true);
    }

    public void closeAction() {
        this.setVisible(false);
    }

    private void setVisible(bool isVisible) {
        this.gameObject.GetComponent<Image>().enabled = isVisible;
        this.messageLabel.gameObject.SetActive(isVisible); // the OK button is a child of this object
    }

}
