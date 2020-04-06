using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener {

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    public static readonly string ITEM_KEY_IAP_100_GOLD = "iap.100";
    public static readonly string ITEM_KEY_IAP_NO_ADS = "iap.noadsperm";
    private static readonly string PRODUCT_ID_100_GOLD = "com.williamwu.scubed.100gold";
    private static readonly string PRODUCT_ID_NO_ADS = "com.williamwu.scubed.noads";
    private static readonly IAPItem[] IAP_ITEMS = {
        new IAPItem(ITEM_KEY_IAP_100_GOLD, PRODUCT_ID_100_GOLD, "100 gold cubes to spend in this store."),
        new IAPItem(ITEM_KEY_IAP_NO_ADS, PRODUCT_ID_NO_ADS, "Permanently disable ads (revive option still enabled).")
    };

    /* * * * Lifecycle methods * * * */

    void Start() {
        if (storeController == null) {
            this.initializePurchasing();
        }
    }

    /* * * * Public getters * * * */

    public static int getNumIAPs() {
        return IAP_ITEMS.Length;
    }

    public static IAPItem getIAPWithID(int id) {
        return IAP_ITEMS[id];
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
        storeController = controller;
        storeExtensionProvider = extensions;
        // TODO: make a delegate or something that populates the store menu iap section, and hide loading text
    }

    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.Log("store initialization failed: " + error);
        // TODO: replace loading text with "currently unavailable" message
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
        //
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason error) {
        //
    }

}
