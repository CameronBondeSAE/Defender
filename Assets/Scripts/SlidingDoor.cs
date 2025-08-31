using Unity.Netcode;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    Vector3 startPosition;
    public Vector3 movement;
    [HideInInspector] public bool isOpen;
    public float speed = 1;
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
    }
}
