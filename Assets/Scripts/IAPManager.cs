using System.Collections;
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
    private static readonly string[] IAP_PRODUCT_IDS = {
        PRODUCT_ID_100_GOLD,
        PRODUCT_ID_NO_ADS
    };

    public StoreMenu storeMenu;

    /* * * * Lifecycle methods * * * */

    void Start() {
        instance = this;
        if (storeController == null) {
            this.initializePurchasing();
        }
        else {
            this.storeMenu.setupIAPSection(true);
        }
    }

    /* * * * Public methods * * * */

    public static int getNumIAPs() {
        return IAP_PRODUCT_IDS.Length;
    }

    public static string getProductID(int index) {
        return IAP_PRODUCT_IDS[index];
    }

    public static Product getProductWithID(string productID) {
        return storeController.products.WithID(productID);
    }

    public static void buyProduct(string productID) {
        if (storeIsInitialized()) {
            Product product = storeController.products.WithID(productID);
            if (product != null) {
                storeController.InitiatePurchase(product); // will receive a callback to either ProcessPurchase or OnPurchaseFailed
            }
        }
    }

    /* * * * Product-specific methods * * * */

    public static bool shouldShowAds() {
        if (storeIsInitialized()) {
            // if this nonconsumable doesn't have a receipt, then it hasn't been purchased yet, so show ads
            return !storeController.products.WithID(PRODUCT_ID_NO_ADS).hasReceipt;
        }
        return true;
    }

    /* * * * Private methods * * * */

    private static bool storeIsInitialized() {
        return (storeController != null && storeExtensionProvider != null);
    }

    private void initializePurchasing() {
        if (storeIsInitialized()) {
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
        string id = args.purchasedProduct.definition.id;
        if (id.Equals(PRODUCT_ID_100_GOLD)) {
            int gold = DataAndSettingsManager.getGoldAmount();
            DataAndSettingsManager.setGoldAmount(gold + 100);
            storeMenu.updateGoldLabel();
        }
        else if (id.Equals(PRODUCT_ID_NO_ADS)) {
            //
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason error) {
        Debug.Log("purchase failed: " + error);
    }

}
