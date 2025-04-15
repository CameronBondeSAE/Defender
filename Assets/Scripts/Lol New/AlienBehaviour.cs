using UnityEngine;
using AIAnimation;
using System.Collections;

public class AlienBehaviour : MonoBehaviour
{
 [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Civ Detection")]
    public float tagDistance = 1.5f;
    public float blockDistance = 2f;
    public LayerMask civLayer;
    private bool isWaitingCoroutine = false;

    [Header("References")]
    public Transform mothership;

    private Rigidbody rb;
    private Transform targetCiv;
    private bool hasCaptured = false;
    private bool waitingForFollow = false;
    private AIAnimationController animController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;

        animController = GetComponent<AIAnimationController>();
        FindNearestCiv();
    }

    void FixedUpdate()
    {
        if (hasCaptured)
        {
            HandleCapturedCiv();
            Debug.Log("caught civ");
        }
        else
        {
            HandleMovementTowardsCiv();
        }

        FaceDirection();
    }

    // Find the nearest Civ to target
    void FindNearestCiv()
    {
        GameObject[] civs = GameObject.FindGameObjectsWithTag("Civilian");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject civ in civs)
        {
            float dist = Vector3.Distance(transform.position, civ.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                targetCiv = civ.transform;
            }
        }
    }

    void HandleMovementTowardsCiv()
    {
        if (targetCiv == null)
            FindNearestCiv();

        if (targetCiv != null)
        {
            MoveTo(targetCiv.position);

            // Check if civ is close enough to tag
            if (Vector3.Distance(transform.position, targetCiv.position) <= tagDistance)
            {
                TagCiv(targetCiv.GetComponent<CivBehaviour>());
            }
        }
    }

    void TagCiv(CivBehaviour civ)
    {
        if (civ == null || civ.isTagged) return;

        civ.StartFollowing(transform);
        hasCaptured = true;
        waitingForFollow = true;
        targetCiv = null; // Clear current Civ target
    }

    void HandleCapturedCiv()
    {
        if (waitingForFollow)
        {
            WaitForFollower();
        }
        else
        {
            MoveTo(mothership.position);
        }
    }

    void WaitForFollower()
    {
        if (targetCiv == null) return;

        Vector3 expectedFollowPos = transform.position - transform.forward;
        float distance = Vector3.Distance(targetCiv.position, expectedFollowPos);

        if (IsCivBlocking() && !isWaitingCoroutine)
        {
            StartCoroutine(WaitAndTurnAround());
        }
        else if (!isWaitingCoroutine && distance <= 1.5f)
        {
            waitingForFollow = false;
            rb.linearVelocity = Vector3.zero;
        }
        else if (!isWaitingCoroutine)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    // Coroutine to handle turning around when the Civ blocks the way
    IEnumerator WaitAndTurnAround()
    {
        isWaitingCoroutine = true;

        rb.linearVelocity = Vector3.zero;

        // Smooth 180-degree turn
        Quaternion targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + 180f, 0f);
        float elapsed = 0f;
        float turnDuration = 0.5f;

        while (elapsed < turnDuration)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsed / turnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        // Wait before resuming
        yield return new WaitForSeconds(2f);

        isWaitingCoroutine = false;
    }

    void MoveTo(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    // Check if the Civ is blocking the path
    bool IsCivBlocking()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = transform.forward;

        Debug.DrawRay(origin, dir * blockDistance, Color.red);
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, blockDistance, civLayer))
        {
            return hit.collider.CompareTag("Civilian");
        }
        return false;
    }
    
    void FaceDirection()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (flatVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            animController?.SetAnimation(AIAnimationController.AnimationState.Walk);
        }
        else
        {
            animController?.SetAnimation(AIAnimationController.AnimationState.Idle);
        }
    }
}
