using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [SerializeField]
    private InventoryPage inventoryUI;

    private InventoryInputs inventoryInputs;
    public int inventorySize = 10;

    private void Start() 
    {
        inventoryInputs = new InventoryInputs();
        inventoryInputs.Enable();
        inventoryUI.InitializeInventoryUI(inventorySize);
    }

    public void Update()
    {
        if (inventoryInputs.Inventory.confirm.triggered)
        {
            if (inventoryUI.isActiveAndEnabled == false)
            {
                inventoryUI.Show();
            }
            else
            {
                inventoryUI.Hide();
            }
        }
    }
}
