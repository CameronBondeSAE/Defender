using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPage : MonoBehaviour
{
    [SerializeField]
    private InventoryItem itemPrefab;

    [SerializeField]
    private RectTransform contantPanel;

    [SerializeField]
    private InventoryDescription itemDescription;

    List<InventoryItem> listofItems = new List<InventoryItem>();

    public Sprite itemImage;
    public int quantity;
    public string title, description;

    private void Awake()
    {
        Hide(); 
        itemDescription.ResetDescription();
    }

    public void InitializeInventoryUI(int inventorySize)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            InventoryItem item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            item.transform.SetParent(contantPanel);
            listofItems.Add(item);
            item.OnItemClicked += HandleItemSelection;
            item.OnItemBeginDrag += HandleBeginDrag;
            item.OnItemDrop += HandleSwap;
            item.OnItemEndDrag += HandleEndDrag;
            item.OnRightMouseButtonClicked += HandleShowItemActions;
        }
    }

    private void HandleShowItemActions(InventoryItem item)
    {
        throw new NotImplementedException();
    }

    private void HandleEndDrag(InventoryItem item)
    {
        throw new NotImplementedException();
    }

    private void HandleSwap(InventoryItem item)
    {
        throw new NotImplementedException();
    }

    private void HandleBeginDrag(InventoryItem item)
    {
        throw new NotImplementedException();
    }

    private void HandleItemSelection(InventoryItem item)
    {
        itemDescription.SetDescription(itemImage, title, description);
        listofItems[0].Select();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        itemDescription.ResetDescription();

        listofItems[0].SetData(itemImage, quantity);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
