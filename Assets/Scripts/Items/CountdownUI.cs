using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : MonoBehaviour
{
    public Text countdownText;
    public Vector3 offset = new Vector3(0, 0.6f, 0);

    private UsableItem_Base _itemBaseToTrack;
    private bool isActive = false;

    void Awake()
    {
        _itemBaseToTrack = GetComponentInParent<UsableItem_Base>();
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
        if (!isActive || _itemBaseToTrack == null)
            return;

        // decide what to follow: carrier (player) or item itself
        Transform followTarget = _itemBaseToTrack.IsCarried && _itemBaseToTrack.CurrentCarrier != null
            ? _itemBaseToTrack.CurrentCarrier
            : _itemBaseToTrack.transform;
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
