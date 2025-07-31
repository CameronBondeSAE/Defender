using Tutorials;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamPlayer_Controller : MonoBehaviour
{
    public Vector3 direction;
    public CamPlayer_Model model;
    private MainControls mainControls;

    private void Awake()
    {
        mainControls = new MainControls();
        mainControls.Game.Enable();
    }

    private void OnEnable()
    {
        mainControls.Game.Jump.performed += OnJump;
        mainControls.Game.Move.performed += OnMove;
        mainControls.Game.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        mainControls.Game.Jump.performed -= OnJump;
        mainControls.Game.Move.performed -= OnMove;
        mainControls.Game.Move.canceled -= OnMove;
    }

    private void OnMove(InputAction.CallbackContext obj)
    {
        // Because this game is on the X,Z plane, not 2D X,Y
        direction.x = obj.ReadValue<Vector2>().x;
        direction.z = obj.ReadValue<Vector2>().y;

        if (obj.performed) Debug.Log("OnMove = " + direction.x + ", " + direction.y);
        if (obj.canceled)
            Debug.Log("OnMove cancelled");
    }

    private void OnJump(InputAction.CallbackContext obj)
    {
        Debug.Log("OnJump");
        model.Jump();
    }
}