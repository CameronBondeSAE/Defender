using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : MonoBehaviour
{
     [Header("Refs")]
    public Text countdownText;

    [Header("Colors")]
    public Color normalCountdownColor = Color.white;
    public Color expiryCountdownColor = Color.red; 

    [Header("Offsets")]
    public Vector3 offset = new Vector3(0, 2.5f, 0);

    [Header("Smoothing / Deadzone")]
    [Tooltip("How quickly this UI catches up once it decides to move.")]
    public float smoothTime = 0.15f;
    [Tooltip("When the item is being carried, ignore jitters that are less than this distance.")]
    public float carriedDeadzone = 0.25f;
    [Tooltip("When the item is on the ground/flying, ignore jitters less than this distance.")]
    public float worldDeadzone = 0.08f;
    [Tooltip("If target jumps farther than this, snap instantly to avoid lagging behind across the map.")]
    public float teleportDistance = 2.0f;

    private UsableItem_Base item;
    private bool isActive;
    private Vector3 anchorPos;
    private Vector3 vel; // for SmoothDamp

    // init to allow expiry styling
    public void Init(UsableItem_Base itemToTrack, int startSeconds, bool useExpiryStyle = false)
    {
        item = itemToTrack;
        isActive = true;

        var targetPos = GetDesiredTargetPos();
        anchorPos = targetPos;
        transform.position = anchorPos;

        // apply color
        if (countdownText)
            countdownText.color = useExpiryStyle ? expiryCountdownColor : normalCountdownColor;

        SetCountdown(startSeconds);
        gameObject.SetActive(true);
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
        if (countdownText) countdownText.text = seconds.ToString();
    }
    
    public void SetExpiryStyle(bool useExpiryStyle)
    {
        if (!countdownText) return;
        countdownText.color = useExpiryStyle ? expiryCountdownColor : normalCountdownColor;
    }

    void LateUpdate()
    {
        if (!isActive || item == null) return;
        Vector3 desired = GetDesiredTargetPos();

        float deadzone = (item.IsCarried ? carriedDeadzone : worldDeadzone);
        float dist = Vector3.Distance(anchorPos, desired);
        if (dist > teleportDistance)
        {
            anchorPos = desired;
            vel = Vector3.zero;
        }
        else if (dist > deadzone)
        {
            anchorPos = Vector3.SmoothDamp(anchorPos, desired, ref vel, smoothTime);
        }
        transform.position = anchorPos;
    }

    private Vector3 GetDesiredTargetPos()
    {
        if (item == null) return transform.position;
        return item.transform.position + offset;
    }
}
