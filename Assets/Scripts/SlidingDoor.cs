using Unity.Netcode;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    Vector3 startPosition;
    public Vector3 movement;
    [HideInInspector] public bool isOpen;
    public float speed = 1;

    public GameObject spark;
    void Start()
    {
        //set the startPosition to the current position
        startPosition = transform.position;
    }

    void Update()
    {
        //if this sliding door isOpen
        if (isOpen)
        {
            //lerp the position of the door by the movement amount based on the speed
            transform.position = Vector3.Lerp(transform.position, startPosition + movement, Time.deltaTime * speed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * speed);
        }
    }
    
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    public void ToggleDoor_Rpc()
    {
        isOpen = !isOpen;
        GetComponent<AudioSource>().Play();
        Instantiate(spark, transform.position, transform.rotation);
    }
}
