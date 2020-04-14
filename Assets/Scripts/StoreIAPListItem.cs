using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System;

public class StoreIAPListItem : MonoBehaviour {

    public Text itemLabel;
    public Button buyButton;
    public Text costLabel;
    public Text descriptionLabel;

    private string productID;
    private bool isConsumable;

    /* * * * Public methods * * * */

    public void setup(int itemIndex) {
        this.productID = IAPManager.getProductID(itemIndex);
        Product product = IAPManager.getProductWithID(productID);
        this.itemLabel.text = product.metadata.localizedTitle;
        this.descriptionLabel.text = product.metadata.localizedDescription;
        this.costLabel.text = product.metadata.localizedPriceString;
        this.isConsumable = (product.definition.type == ProductType.Consumable);
        this.updateButton();
    }

    /* * * * UI actions * * * */

    public void buyAction() {
        FindObjectOfType<AudioManager>().playButtonSound();
        IAPManager.buyProduct(this.productID);
    }

    /* * * * Helper methods * * * */

    private void updateButton() {
        this.buyButton.interactable = (this.isConsumable || !IAPManager.hasPurchasedNonconsumable(this.productID));
    }

}
