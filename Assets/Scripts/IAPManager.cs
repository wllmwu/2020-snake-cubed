using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class IAPManager : MonoBehaviour, IStoreListener {

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    public static readonly string ITEM_KEY_IAP_100_GOLD = "iap.100gold";
    public static readonly string ITEM_KEY_IAP_NO_ADS = "iap.noadsperm";
    private static readonly string PRODUCT_ID_100_GOLD = "com.williamwu.scubed.100gold";
    private static readonly string PRODUCT_ID_NO_ADS = "com.williamwu.scubed.noads";
    private static readonly string[] IAP_PRODUCT_IDS = {
        PRODUCT_ID_100_GOLD,
        PRODUCT_ID_NO_ADS
    };

    public StoreMenu storeMenu;

    #if UNITY_IOS
    private static string appReceipt;
    #endif

    /* * * * Lifecycle methods * * * */

    void Start() {
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

    ///<summary>Returns whether interstitial ads should be shown, based on whether the user has bought the permanent no-ads product.
    /// If this cannot be determined, defaults to yes. Should also check whether the user has bought temporary no-ads items.</summary>
    public static bool shouldShowAds() {
        if (storeIsInitialized()) {
            // if this nonconsumable doesn't have a (valid) receipt, then it hasn't been purchased yet, so show ads
            #if UNITY_IOS
            return !validateReceipt(appReceipt, PRODUCT_ID_NO_ADS);
            #elif UNITY_ANDROID
            string receipt = storeController.products.WithID(PRODUCT_ID_NO_ADS).receipt;
            return (!String.IsNullOrEmpty(receipt) && validateReceipt(receipt, PRODUCT_ID_NO_ADS));
            #else
            return !storeController.products.WithID(PRODUCT_ID_NO_ADS).hasReceipt;
            #endif
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
        #if UNITY_IOS
        appReceipt = builder.Configure<IAppleConfiguration>().appReceipt;
        #endif
        builder.AddProduct(PRODUCT_ID_100_GOLD, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_NO_ADS, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder); // will receive a callback to either OnInitialized or OnInitializeFailed
    }

    ///<summary>Returns whether the given IAP receipt a) is a valid receipt and b) contains the given product ID.
    /// IMPORTANT: Only call this method when running on iOS or Android. Otherwise, perhaps assume the receipt is valid.</summary>
    private static bool validateReceipt(string receipt, string intendedProductID) {
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
        try {
            var result = validator.Validate(receipt); // checks whether the receipt is valid, throws if not
            bool foundMatch = false;
            foreach (IPurchaseReceipt productReceipt in result) {
                // user may provide a "valid" receipt from somewhere else, so look for the ID of the intended product
                if (productReceipt.productID.Equals(intendedProductID)) {
                    foundMatch = true;
                    break;
                }
            }
            return foundMatch;
        }
        catch (IAPSecurityException) {
            Debug.Log("invalid receipt");
            return false;
        }
        catch (System.Exception) {
            Debug.Log("something bad happened while validating receipt");
            return false;
        }
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
        bool validPurchase = true;
        string purchasedProductID = args.purchasedProduct.definition.id;
        #if UNITY_IOS || UNITY_ANDROID
        validPurchase = validateReceipt(args.purchasedProduct.receipt, purchasedProductID);
        #endif
        if (validPurchase) {
            if (purchasedProductID.Equals(PRODUCT_ID_100_GOLD)) {
                int gold = DataAndSettingsManager.getGoldAmount();
                DataAndSettingsManager.setGoldAmount(gold + 100);
                storeMenu.updateGoldLabel();
            }
            // no action needed for the no-ads product
            FindObjectOfType<AlertPrompt>().showMessage("Purchase successful!");
        }
        else {
            FindObjectOfType<AlertPrompt>().showMessage("Purchase failed validation.");
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason error) {
        Debug.Log("purchase failed: " + error);
        FindObjectOfType<AlertPrompt>().showMessage("An error occurred while processing the purchase. " +
            "Make sure in-app purchases are allowed in your device settings.");
    }

}
