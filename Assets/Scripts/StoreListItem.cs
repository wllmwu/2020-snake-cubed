using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StoreListItem : MonoBehaviour {

    public Text itemLabel;
    public Button buyButton;
    public Text costLabel;
    public Text descriptionLabel;

    private int itemID;
    private bool isExpendableItem;
    private Action menuResponder;

    /* * * * Public methods * * * */

    public void setup(int itemID, bool isExpendableItem, Action menuResponder) {
        this.itemID = itemID;
        this.isExpendableItem = isExpendableItem;
        this.menuResponder = menuResponder;
        this.updateLabelsAndButton();
    }

    /* * * * UI actions * * * */

    public void buyAction() {
        FindObjectOfType<AudioManager>().playButtonSound();
        if (StoreManager.buyItem(this.itemID)) {
            this.updateLabelsAndButton();
            this.menuResponder();
        }
    }

    /* * * * Helper methods * * * */

    private void updateLabelsAndButton() {
        StoreItem si = StoreManager.getItemWithID(this.itemID);
        string name = si.getName();
        if (this.isExpendableItem) {
            name += " (" + si.getNumBought() + " left)";
        }
        this.itemLabel.text = name;
        this.costLabel.text = "" + si.getCost();
        this.buyButton.interactable = (this.isExpendableItem || si.getNumBought() == 0);
        this.descriptionLabel.text = si.getDescription();
    }

}
