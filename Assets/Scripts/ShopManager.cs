using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    private InventoryItem[] SPAllItems;
    private InventoryItem[] SPItemList;
    private int[] SPItemQuantityList;
    private float[] SPItemPriceList;
    private int ItemListScroll = 0;
    //index of array
    private int ItemListSelected = 0;
    private int MaxItemsShown = 8;
	public TMP_Text SPItemInfo;
    public TMP_Text SPItemTextList;
    public TMP_Text SPHudGold;
    public TMP_Text SPShopOwnerText;

    public RawImage SPBackground;
    public AudioSource SPAudioMusic;
    public AudioSource SPAudioBackground;
    public RawImage SPShopOwnerImg;

    void Start() {
        TextAsset jsonFileAllItems = Resources.Load<TextAsset>("Graphs/allitems");
        InventoryItemHolder holder = JsonUtility.FromJson<InventoryItemHolder>(jsonFileAllItems.text);
        SPAllItems = holder.data;
        TextAsset jsonFileShopInfo = Resources.Load<TextAsset>("Graphs/shop1");
        ShopInformation shopinfo = JsonUtility.FromJson<ShopInformation>(jsonFileShopInfo.text);
        SetBackground(shopinfo.background);
        SetShopOwnerImg(shopinfo.ownersprite);
        SetShopOwnerText(shopinfo.introtext);
        //initializing SPItemList based off shopinfo.storeitems
        List<InventoryItem> invlisttemp = new List<InventoryItem>();
        List<int> invquantitytemp = new List<int>();
        List<float> invpricetemp = new List<float>();
        for (int i = 0; i < shopinfo.storeitems.Length; i++) {
            string uid = shopinfo.storeitems[i];
            int quantity = shopinfo.storequantities[i];
            float price = shopinfo.storeprices[i];
            invlisttemp.Add(GetItemByUid(uid));
            invquantitytemp.Add(quantity);
            invpricetemp.Add(price);
        }
        SPItemList = invlisttemp.ToArray();
        SPItemQuantityList = invquantitytemp.ToArray();
        SPItemPriceList = invpricetemp.ToArray();

        RenderItemTextList();
    }

    void Update() {
        //moving selected up and down
        bool selectedChanged = false;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown("x")) {
            if (ItemListSelected > 0) {
                ItemListSelected -= 1;
                selectedChanged = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown("c")) {
            if (ItemListSelected < SPItemList.Length-1) {
                ItemListSelected += 1;
                selectedChanged = true;
            }
        }
        //updating scroll based on selected
        if (ItemListSelected > ItemListScroll + MaxItemsShown) {
            ItemListScroll += 1;
        }
        else if (ItemListSelected < ItemListScroll) {
            ItemListScroll -= 1;
        }
        //update rendering
        if (selectedChanged) {
            RenderItemTextList();
            SetItemInfo(SPItemList[ItemListSelected].desc);
        }
    }

    //uses ItemListSelected
    void AttemptBuyItem() {
        InventoryItem toBuy = SPItemList[ItemListSelected];
        float price = SPItemPriceList[ItemListSelected];
        int storequantity = SPItemQuantityList[ItemListSelected];
        //todo connect with gamecontext
    }

    void SetItemInfo(string text) {
        SPItemInfo.SetText(text);
    }

    void SetShopOwnerText(string text) {
        SPShopOwnerText.SetText(text);
    }

    void SetItemTextList(string text) {
        SPItemTextList.SetText(text);
    }

    void SetBackground(string filePath) {
        SPBackground.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/" + filePath);
    }

    void SetShopOwnerImg(string filePath) {
        SPShopOwnerImg.texture = Resources.Load<Texture2D>("Images/" + filePath);
    }

    InventoryItem GetItemByUid(string uid) {
        foreach (InventoryItem item in SPAllItems) {
            if (item.uid == uid) {
                return item;
            }
        }
        return null;
    }

    //renders it given the current itemlistscroll, itemlistselected, maxitems shown
    void RenderItemTextList() {
        string s = "";
        for (int i = ItemListScroll; i < Mathf.Min(MaxItemsShown+ItemListScroll,SPItemList.Length); i++) {
            string toAdd = SPItemQuantityList[i] > 1 ? SPItemList[i].name + " (" + SPItemQuantityList[i] + ")" : SPItemList[i].name;
            if (i == ItemListSelected) {
                s += "<b>" + toAdd + "</b>\n";
            } else {
                s += toAdd + "\n";
            }
        }
        SetItemTextList(s);
    }
}