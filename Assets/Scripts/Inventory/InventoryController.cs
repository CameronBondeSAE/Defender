using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [SerializeField]
    private UIInventoryPage inventoryUI;

    [SerializeField]
    private InventorySO inventoryData;
    private InventoryInputs inventoryInputs;

    private void Start()
    {
        inventoryInputs = new InventoryInputs();
        inventoryInputs.Enable();
        PrepareUI();
        //inventoryData.Initialize();
    }

    private void PrepareUI()
    {
        inventoryUI.InitializeInventoryUI(inventoryData.Size);
        this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
        this.inventoryUI.OnSwapItems += HandleSwapItems;
        this.inventoryUI.OnStartDragging += HandleDragging;
        this.inventoryUI.OnItemActionRequested += HandleItemActionRequest;
    }

    private void HandleItemActionRequest(int itemIndex)
    {

    }

    private void HandleDragging(int itemIndex)
    {

    }

    private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
    {

    }

    private void HandleDescriptionRequest(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
        {
            inventoryUI.ResetSelection();
            return;
        }
        ItemSO item = inventoryItem.item;
        inventoryUI.UpdateDescription(itemIndex, item.ItemImage,
            item.name, item.Description);
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

