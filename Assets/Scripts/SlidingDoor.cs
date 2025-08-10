using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 movement;
    public bool isOpen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpen)
        {
            transform.position = Vector3.Lerp(transform.position, startPosition + movement, Time.deltaTime);
        }
    }
}
