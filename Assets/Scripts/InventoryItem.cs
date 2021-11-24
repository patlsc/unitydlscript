using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem {
	public string name;
	public string uid;
	public string desc;
	public Dictionary<string,string> vals;
}

[System.Serializable]
public class InventoryItemHolder {
	public InventoryItem[] data;
}

[System.Serializable]
public class ShopInformation {
	public string[] storeitems;
	public int[] storequantities;
	public int[] storeprices;
	public string introtext;
	public string ownersprite;
	public string background;
}