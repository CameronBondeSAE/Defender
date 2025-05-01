using Inventory.SO;
using Inventory.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryPage inventoryUI;

        [SerializeField]
        public InventorySO inventoryData;
        private InventoryInputs inventoryInputs;

        public List<InventoryItem> initialItems = new List<InventoryItem> ();

        private int currentlySelectedIndex = -1; // reference to curently selected item
        [SerializeField] private Transform playerTransform;

        private void Start()
        {
            inventoryInputs = new InventoryInputs();
            inventoryInputs.Enable();
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            foreach (InventoryItem item in initialItems)
            {
                if (item.IsEmpty)
                {
                    continue;
                }
                inventoryData.AddItem(item);
            }
        }
        
        public void AddItemToInventory(ItemSO itemSO)
        {
            InventoryItem newItem = new InventoryItem
            {
                item = itemSO,
                quantity = 1
            };
            inventoryData.AddItem(newItem);
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        private void HandleItemActionRequest(int itemIndex)
        {

        }

        private void HandleDragging(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                return;
            }
            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
        {
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }

            currentlySelectedIndex = itemIndex; // adding currently selected item to reference (recognizing it)

            ItemSO item = inventoryItem.item;
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage,
                item.name, item.Description);
        }

        public void ActivateSelectedItem()
        {
            if (currentlySelectedIndex == -1) return;

            InventoryItem selectedItem = inventoryData.GetItemAt(currentlySelectedIndex);
            if (selectedItem.IsEmpty || selectedItem.item.itemPrefab == null) return;

            // or asl player's child...?
            GameObject instance = Instantiate(selectedItem.item.itemPrefab, playerTransform);
            instance.transform.localPosition = new Vector3(0, 0, 1.5f);
            inventoryData.RemoveItemAt(currentlySelectedIndex);
            currentlySelectedIndex = -1;

        }

        public void Update()
        {
            if (inventoryInputs.Inventory.confirm.triggered)
            {
                if (inventoryInputs.Inventory.confirm.triggered)
                {
                        inventoryUI.Show();
                        foreach (var item in inventoryData.GetCurrentInventoryState())
                        {
                            inventoryUI.UpdateData(item.Key,
                                item.Value.item.ItemImage,
                                item.Value.quantity);
                        }
                }
                else
                {
                    inventoryUI.Hide();
                }
            }
        }
    }
}