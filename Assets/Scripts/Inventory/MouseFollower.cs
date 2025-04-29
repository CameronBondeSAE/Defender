using Inventory.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFollower : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private UIInventoryItem item;

    private Mouse currentMouse;

    public void Awake()
    {
        canvas = transform.root.GetComponent<Canvas>();
        item = GetComponentInChildren<UIInventoryItem>();

        currentMouse = Mouse.current;
    }

    public void SetData(Sprite sprite, int quantity)
    {
        item.SetData(sprite, quantity);
    }
    void Update()
    {
        if (currentMouse != null)
        {
            Vector2 position = currentMouse.position.ReadValue();
            Vector2 localPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                position,
                canvas.worldCamera,
                out localPosition
            );

            transform.position = canvas.transform.TransformPoint(localPosition);
        }
    }

    public void Toggle(bool val)
    {
        //Debug.Log($"Item toggled {val}");
        gameObject.SetActive(val);
    }
}
