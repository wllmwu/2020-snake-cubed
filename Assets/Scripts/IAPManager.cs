﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener {

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;
    private static IAPManager instance;

    public static readonly string ITEM_KEY_IAP_100_GOLD = "iap.100gold";
    public static readonly string ITEM_KEY_IAP_NO_ADS = "iap.noadsperm";
    private static readonly string PRODUCT_ID_100_GOLD = "com.williamwu.scubed.100gold";
    private static readonly string PRODUCT_ID_NO_ADS = "com.williamwu.scubed.noads";
    private static readonly IAPItem[] IAP_ITEMS = {
        new IAPItem(ITEM_KEY_IAP_100_GOLD, PRODUCT_ID_100_GOLD, "100 gold cubes to spend in this store."),
        new IAPItem(ITEM_KEY_IAP_NO_ADS, PRODUCT_ID_NO_ADS, "Permanently disable ads (revive still enabled).")
    };

    public StoreMenu storeMenu;

    /* * * * Lifecycle methods * * * */

    void Start() {
        instance = this;
        if (storeController == null) {
            this.initializePurchasing();
        }
    }

    /* * * * Public methods * * * */

    public static int getNumIAPs() {
        return IAP_ITEMS.Length;
    }

    public static IAPItem getIAPWithIndex(int index) {
        return IAP_ITEMS[index];
    }

    public static Product getProductWithID(string productID) {
        return storeController.products.WithID(productID);
    }

    /* * * * Private methods * * * */

    private void initializePurchasing() {
        if (storeController != null && storeExtensionProvider != null) {
            return; // already initialized
        }
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(PRODUCT_ID_100_GOLD, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_NO_ADS, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder); // will receive a callback to either OnInitialized or OnInitializeFailed
    }

    /* * * * IStoreListener delegate methods * * * */

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        Debug.Log("iap initialized");
        storeController = controller;
        storeExtensionProvider = extensions;
        this.storeMenu.setupIAPSection(true);
    }

    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.Log("store initialization failed: " + error);
        this.storeMenu.setupIAPSection(false);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
        //
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason error) {
        //
    }

}

public class IAPItem {

    private string key;
    private string productID;
    private string description;

    public IAPItem(string key, string productID, string description) {
        this.key = key;
        this.productID = productID;
        this.description = description;
    }

    public string getKey() { return this.key; }
    public string getProductID() { return this.productID; }
    public string getDescription() { return this.description; }
    public int numBought() { return DataAndSettingsManager.getNumBoughtForStoreItem(this.key); }

}
