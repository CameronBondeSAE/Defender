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

    [SerializeField]
    private MouseFollower mouseFollower;

    List<InventoryItem> listofItems = new List<InventoryItem>();

    public event Action<int> OnDescriptionRequest, OnItemActionRequest, OnStartDragging;

    public event Action<int, int> OnSwapItems;

    private int currentlyDraggedItemIndex = -1;

    private void Awake()
    {
        Hide();
        mouseFollower.Toggle(false);
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

    public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        if (listofItems.Count > itemIndex)
        {
            listofItems[itemIndex].SetData(itemImage, itemQuantity);
        }
    }

    private void HandleShowItemActions(InventoryItem inventoryItem)
    {

    }

    private void HandleEndDrag(InventoryItem inventoryItem)
    {
        ResetDraggedItem();
    }

    private void HandleSwap(InventoryItem inventoryItem)
    {
        int index = listofItems.IndexOf(inventoryItem);
        if (index == -1)
        {
            return;
        }
        OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
    }

    private void ResetDraggedItem()
    {
        mouseFollower.Toggle(false);
        currentlyDraggedItemIndex = -1;
    }

    private void HandleBeginDrag(InventoryItem inventoryItem)
    {
        int index = listofItems.IndexOf(inventoryItem);
        if(index == -1)
        {
            return;
        }
        currentlyDraggedItemIndex = index;

        HandleItemSelection(inventoryItem);
        OnStartDragging?.Invoke(index);
    }

    public void CreatDraggedItem(Sprite sprite, int quantity)
    {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(sprite, quantity);
    }

    private void HandleItemSelection(InventoryItem inventoryItem)
    {
        int index = listofItems.IndexOf(inventoryItem);
        if (index == -1)
        {
            return;
        }
        OnDescriptionRequest?.Invoke(index);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ResetSelection();
    }

    private void ResetSelection()
    {
        itemDescription.ResetDescription();
        DeSeletAllItems();
    }

    private void DeSeletAllItems()
    {
        foreach (InventoryItem item in listofItems)
        {
            item.Deselect();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ResetDraggedItem();
    }
}
