using Defender;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BeerBarrel : UsableItem_Base
{
    [Space]
    [Header("Barrel Settings")]
    [SerializeField] private GameObject barrelModel;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float zAxisRotateToAngle = 40f;

    private enum PouringState { disabled, used };
    [SerializeField] private PouringState state;


    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);

        Debug.Log("Pouring Beer");

        characterTryingToUse.gameObject.GetComponent<PlayerInventory>().DropHeldItem();
        state = PouringState.used;
    }

    private void Update()
    {
        if (state == PouringState.used)
        {
            RotateBarrel();
        }
    }

    private bool firstRotation = false; // temp solution for identifying when to stop rotating -- else it will continuesly loop

    private void RotateBarrel()
    {
        Debug.Log(barrelModel.transform.localEulerAngles.z);

        Quaternion angle = Quaternion.Euler(0,0,zAxisRotateToAngle);
        if (barrelModel.transform.localEulerAngles.z > zAxisRotateToAngle && firstRotation == false)
        {
            Debug.Log("first rotation");
            barrelModel.transform.Rotate(0, 0, 1 * rotationSpeed * Time.deltaTime, Space.Self);
        }
        else if (barrelModel.transform.localEulerAngles.z < zAxisRotateToAngle)
        {
            firstRotation = true;
            Debug.Log("Second rotation");
            barrelModel.transform.Rotate(0, 0, 1 * rotationSpeed * Time.deltaTime, Space.Self);
        }

        Debug.Log("Complete");
    }
}
