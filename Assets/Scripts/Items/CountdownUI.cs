using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : MonoBehaviour
{
    public Text countdownText;
    public Vector3 offset = new Vector3(0, 0.6f, 0);

    private UsableItem itemToTrack;
    private bool isActive = false;

    void Awake()
    {
        itemToTrack = GetComponentInParent<UsableItem>();
        Hide();
    }

    public void Show()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        isActive = false;
        if (countdownText) countdownText.text = "";
        gameObject.SetActive(false);
    }

    public void SetCountdown(int seconds)
    {
        if (countdownText)
            countdownText.text = seconds.ToString();
    }

    void LateUpdate()
    {
        if (!isActive || itemToTrack == null)
            return;

        // decide what to follow: carrier (player) or item itself
        Transform followTarget = itemToTrack.IsCarried && itemToTrack.CurrentCarrier != null
            ? itemToTrack.CurrentCarrier
            : itemToTrack.transform;
        transform.position = followTarget.position + offset;

        // face camera and no flipping
        var cam = Camera.main;
        if (cam)
        {
            Vector3 camPos = cam.transform.position;
            camPos.y = transform.position.y;
            transform.LookAt(camPos);
            transform.Rotate(0, 180, 0);
        }
    }
}
