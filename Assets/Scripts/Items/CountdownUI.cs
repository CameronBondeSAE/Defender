using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : MonoBehaviour
{
      [Header("Refs")]
    public Text countdownText;

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

    public void Init(UsableItem_Base itemToTrack, int startSeconds)
    {
        item = itemToTrack;
        isActive = true;

        // set initial position (no parenting noww)
        var targetPos = GetDesiredTargetPos();
        anchorPos = targetPos;
        transform.position = anchorPos;

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

    void LateUpdate()
    {
        if (!isActive || item == null) return;
        Vector3 desired = GetDesiredTargetPos();
        // choose deadzone based on if the item is currently carried
        float deadzone = (item.IsCarried ? carriedDeadzone : worldDeadzone);
        float dist = Vector3.Distance(anchorPos, desired);
        if (dist > teleportDistance)
        {
            // if there's a big jump (if the item is thrown or teleported) - snap
            anchorPos = desired;
            vel = Vector3.zero;
        }
        else if (dist > deadzone)
        {
            // if there's significant motion, smoothly catch up
            anchorPos = Vector3.SmoothDamp(anchorPos, desired, ref vel, smoothTime);
        }
        transform.position = anchorPos;
        
        // Face the camera (no item rotation!)
        // var cam = Camera.main;
        // if (cam)
        // {
        //     // face cam horizontally to reduce tilt
        //     Vector3 camPos = cam.transform.position;
        //     camPos.y = transform.position.y;
        //     transform.LookAt(camPos);
        //     transform.Rotate(0f, 180f, 0f); // flip to face Text at camera
        // }
    }
    private Vector3 GetDesiredTargetPos()
    {
        if (item == null) return transform.position;
        return item.transform.position + offset;

        // Transform followTarget = (item.IsCarried && item.CurrentCarrier != null)
        //     ? item.CurrentCarrier
        //     : item.transform;
        //
        // return followTarget.position + offset;
    }
}
