using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreMenu : MonoBehaviour {

    public Text goldLabel;
    public GameObject expendablesListContentRect;
    public GameObject unlockablesListContentRect;
    public GameObject purchasablesListContentRect;
    public StoreListItem listItemPrefab;
    public StoreIAPListItem iapListItemPrefab;
    public Text iapLoadingLabel;

    private static readonly float LIST_ITEM_HEIGHT = 180f;
    private static readonly float LIST_TITLE_HEIGHT = 80f;

    // Start is called before the first frame update
    void Start() {
        this.setupList();
    }

    void OnEnable() {
        this.updateGoldLabel();
    }

    /* TEMPORARY METHODS */
    /*public void addGold() {
        DataAndSettingsManager.setGoldAmount(DataAndSettingsManager.getGoldAmount() + 100);
        this.updateGoldLabel();
    }

    public void subtractGold() {
        DataAndSettingsManager.setGoldAmount(DataAndSettingsManager.getGoldAmount() - 1);
        this.updateGoldLabel();
    }*/

    /* * * * Private methods * * * */

    private void setupList() {
        // expendables
        for (int i = 0; i < StoreManager.getNumExpendables(); i++) {
            StoreListItem item = Instantiate(this.listItemPrefab, new Vector2(0f, -LIST_ITEM_HEIGHT * i - LIST_TITLE_HEIGHT), Quaternion.identity) as StoreListItem;
            RectTransform itemRect = item.gameObject.GetComponent<RectTransform>();
            itemRect.SetParent(this.expendablesListContentRect.transform, false);
            item.setup(i, true, this.updateGoldLabel);
        }

        // unlockables
        for (int i = StoreManager.getNumExpendables(); i < StoreManager.getNumItems(); i++) {
            StoreListItem item = Instantiate(this.listItemPrefab, new Vector2(0f, -LIST_ITEM_HEIGHT * (i - StoreManager.getNumExpendables()) - LIST_TITLE_HEIGHT), Quaternion.identity) as StoreListItem;
            RectTransform itemRect = item.gameObject.GetComponent<RectTransform>();
            itemRect.SetParent(this.unlockablesListContentRect.transform, false);
            item.setup(i, false, this.updateGoldLabel);
        }

        // adjust content rect heights and positions - anchors are at top left and top right
        RectTransform expendablesRect = this.expendablesListContentRect.GetComponent<RectTransform>();
        float expendablesRectHeight = LIST_ITEM_HEIGHT * StoreManager.getNumExpendables() + LIST_TITLE_HEIGHT;
        expendablesRect.sizeDelta = new Vector2(0f, expendablesRectHeight);
        RectTransform unlockablesRect = this.unlockablesListContentRect.GetComponent<RectTransform>();
        float unlockablesRectHeight = LIST_ITEM_HEIGHT * (StoreManager.getNumItems() - StoreManager.getNumExpendables()) + LIST_TITLE_HEIGHT;
        unlockablesRect.sizeDelta = new Vector2(0f, unlockablesRectHeight);
        RectTransform purchasablesRect = this.purchasablesListContentRect.GetComponent<RectTransform>();
        float purchasablesRectHeight = LIST_ITEM_HEIGHT * IAPManager.getNumIAPs() + LIST_TITLE_HEIGHT;
        purchasablesRect.sizeDelta = new Vector2(0f, purchasablesRectHeight);
    }

    /* * * * Public methods * * * */

    public void updateGoldLabel() {
        this.goldLabel.text = "" + DataAndSettingsManager.getGoldAmount();
    }

    public void setupIAPSection(bool iapDidLoad) {
        if (iapDidLoad) {
            // populate IAP section
            this.iapLoadingLabel.gameObject.SetActive(false);
            for (int i = 0; i < IAPManager.getNumIAPs(); i++) {
                StoreIAPListItem item = Instantiate(this.iapListItemPrefab, new Vector2(0f, -LIST_ITEM_HEIGHT * i - LIST_TITLE_HEIGHT), Quaternion.identity) as StoreIAPListItem;
                RectTransform itemRect = item.gameObject.GetComponent<RectTransform>();
                itemRect.SetParent(this.purchasablesListContentRect.transform, false);
                item.setup(i);
            }
        }
        else {
            // change message
            this.iapLoadingLabel.text = "Currently unavailable. In-app purchases might be disabled in your device settings.";
        }
    }

}
