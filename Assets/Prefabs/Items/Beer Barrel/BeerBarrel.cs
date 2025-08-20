using Defender;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Visually tips an object (barrel in this case) and spawns an object below (slippery floor)
/// </summary>
public class BeerBarrel : UsableItem_Base
{
    [Space]
    [Header("Barrel Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [Tooltip("The angle you want the barrel to rotate to - this variable is delicate")]
    [SerializeField] private float xAxisRotateToAngle = 40f;
    [Tooltip("The time limit for the barrel to despawn")]
    [SerializeField] private float despawnTimer;

    [Space]
    [Header("Object References")]
    [SerializeField] private GameObject barrel;
    [SerializeField] private GameObject waterParticles;
    [SerializeField] private GameObject slipperyFloorPrefab;
    private SlipperyFloor slipperyFloor;

    private enum PouringState { disabled, used };
    [Space]
    [Header("Current State")]
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
            StartCoroutine(DespawnNetworkObjectDelay_Coroutine());
        }
    }

    private bool firstRotation = false; // temp solution for identifying when to stop rotating -- else it will continuesly loop
    private void RotateBarrel()
    {
        if (!IsServer) { return; }

        //Debug.Log(barrel.transform.localEulerAngles.x);
        barrel.GetComponent<Collider>().enabled = false;

        if (barrel.transform.localEulerAngles.x < xAxisRotateToAngle && firstRotation == false)
        {
            // only reaches half of the rotation -- rotating on the x axis positively ranges from 0 - 90 degrees (first half), back to 90 - 0 degrees (second half)
            Debug.Log("first rotation");
            barrel.transform.Rotate(1 * rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            barrel.transform.position += new Vector3(0, 0.05f, 0) * rotationSpeed * Time.deltaTime;
        }
        else if (barrel.transform.localEulerAngles.x > xAxisRotateToAngle)
        {
            // second half of the rotation reaches the desired angle
            firstRotation = true;
            Debug.Log("Second rotation");
            barrel.transform.Rotate(1 * rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            waterParticles.SetActive(true);
        }

        if (firstRotation == true)
        {
            SpawnSlipperyFloor();
        }

        // TODO: despawn object gameObject.GetComponent<NetworkObject>().Despawn();
    }

    private void SpawnSlipperyFloor()
    {
        if (!IsServer) { return; }

        if (slipperyFloor == null)
        {
            Vector3 randomSpawnPosition = new Vector3(transform.position.x, 20f, transform.position.z);
            RaycastHit hit;

            if (Physics.Raycast(randomSpawnPosition, Vector3.down, out hit, Mathf.Infinity))
            {
                GameObject newGO = Instantiate(slipperyFloorPrefab, hit.point, Quaternion.identity);
                newGO.GetComponent<NetworkObject>().Spawn();
                slipperyFloor = newGO.GetComponent<SlipperyFloor>();
            }
        }
    }

    private IEnumerator DespawnNetworkObjectDelay_Coroutine()
    {
        yield return new WaitForSeconds(despawnTimer);

        if (IsServer)
        {
            DespawnNetworkObjectClient_RPC();
        }
        else
        {
            DestroyObject();
        }
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    private void DespawnNetworkObjectClient_RPC()
    {
        if (IsServer && gameObject.GetComponent<NetworkObject>().IsSpawned == true)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
