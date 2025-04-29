using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeAim : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GrenadeTrajectory trajectory;

    [SerializeField] private LineRenderer lineRenderer; // Draws the grenade trajectory
    [SerializeField] private Transform throwPoint; // Starting point for grenade throws
    [SerializeField] private PlayerInputHandler playerInputHandler; // Handles input events for aiming

    [Header("Aiming")] private float horizontalAngle = 0f; // Allows grenade aiming left & right
    [SerializeField] private float verticalAngle = 0f; // Allows grenade aiming up & down
    [SerializeField] private float minVerticalAngle = 20f; // limits how low you can aim grenades
    [SerializeField] private float maxVerticalAngle = 35f; // limits how high you can aim grenades

    [Header("Sensitivity")] [SerializeField]
    private float verticalSensitivity = 0.3f; // Up & down line renderer sensitivity

    [SerializeField] private float horizontalSensitivity = 0.3f; // Left & right line renderer sensitivity

    private bool isAiming = false; // Is the player currently aiming?

    /// <summary>
    /// Finds the PlayerInputHandler and subscribes to the grenade aiming start and stop events.
    /// </summary>
    private void Awake()
    {
        playerInputHandler = FindObjectOfType<PlayerInputHandler>(); // Finds the PlayerInputHandler in the scene

        // Subscribe to input events
        if (playerInputHandler != null)
        {
            playerInputHandler.onAimGrenadeStart += StartAiming;
            playerInputHandler.onAimGrenadeStop += StopAiming;
        }
    }

    /// <summary>
    /// Unsubscribes from grenade aiming events
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from input events when destroyed
        if (playerInputHandler != null)
        {
            playerInputHandler.onAimGrenadeStart -= StartAiming;
            playerInputHandler.onAimGrenadeStop -= StopAiming;
        }
    }

    /// <summary>
    /// Starts the aiming process, locks the cursor, and enables the trajectory line.
    /// </summary>
    public void StartAiming()
    {
        isAiming = true;
        if (lineRenderer != null) lineRenderer.enabled = true; // Enables the LineRenderer to show the trajectory
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor to the center
        // Cursor.visible = false; // Hides cursor <--------- KEEP OR DELETE?? 
    }

    /// <summary>
    /// Stops the aiming process, unlocks the cursor, and disables the trajectory line.
    /// </summary>
    public void StopAiming()
    {
        isAiming = false;
        if (lineRenderer != null) lineRenderer.enabled = false; // Disables the LineRenderer 
        Cursor.lockState = CursorLockMode.None; // Unlocks the cursor
        // Cursor.visible = false; // Hides cursor <--------- KEEP OR DELETE?? 
    }

    private void Update()
    {
        if (isAiming && lineRenderer.enabled)
        {
            UpdateAimDirection(); // Updates the aim direction based on mouse input
            DrawArc(); // Draws the grenade trajectory arc
        }
    }

    /// <summary>
    /// Updates the vertical and horizontal aim angles based on mouse movement.
    /// </summary>
    private void UpdateAimDirection()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue(); // Gets the mouse position ( "X" axis + "Y" axis)

        // Adjust vertical and horizontal angles based on mouse movement and sensitivity
        verticalAngle += mouseDelta.y * verticalSensitivity;
        horizontalAngle += mouseDelta.x * horizontalSensitivity;

        // Clamp the vertical angle to prevent extreme aiming angles
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

        // Calculate the direction of the throw based on the aim angles
        Vector3 direction = Quaternion.Euler(-verticalAngle, horizontalAngle, 0f) * Vector3.forward;
        throwPoint.rotation = Quaternion.LookRotation(direction); // Set the rotation of the throw point
    }

    /// <summary>
    /// Draws the grenade's trajectory arc using the LineRenderer.
    /// </summary>
    private void DrawArc()
    {
        if (lineRenderer == null || trajectory == null) return;

        int pointsCount = trajectory.arcPoints;

        // Array initialized and set to correct size
        if (trajectory.calculatedPoints == null || trajectory.calculatedPoints.Length != pointsCount)
        {
            trajectory.calculatedPoints = new Vector3[pointsCount];
        }

        Vector3 startPos = throwPoint.position; // Starting position of the grenade

        float verticalRad = Mathf.Deg2Rad * verticalAngle; // Converts vertical aim angle from degrees to radians
        float horizontalRad = Mathf.Deg2Rad * horizontalAngle; // Converts horizontal aim angle from degrees to radians

        Vector3 startVel = new Vector3 // Calculates the direction of the grenade throw
        (
            Mathf.Sin(horizontalRad) * Mathf.Cos(verticalRad) *
            trajectory.launchForce, // Velocity in left/right direction
            Mathf.Sin(verticalRad) * trajectory.launchForce, // Velocity in up/down direction
            Mathf.Cos(horizontalRad) * Mathf.Cos(verticalRad) *
            trajectory.launchForce // Velocity in forward/backward direction
        );

        Vector3 gravity = Physics.gravity; // Gets the gravity of the game world 

        Vector3 lastPosition = startPos;
        bool hitDetected = false; // Flag to detect if a hit occurs

        // Raycasting variables
        RaycastHit hit;
        float maxRayDistance = trajectory.launchForce * 2f; // Maximum distance to check for obstacles

        for (int i = 0; i < pointsCount; i++) // Loop through each point in the arc
        {
            float time = i * trajectory.timeBetweenPoints; // Calculate the time at each point

            // Calculate the position of the grenade at each point in the trajectory
            Vector3 point = startPos + startVel * time;

            // Adjusts the grenade's position on the Y axis (up/down)
            point.y = startPos.y + startVel.y * time + 0.5f * gravity.y * time * time; // Adjust for gravity

            // Adjusts the grenade's position on "X" axis (left/right)
            point.x = startPos.x + startVel.x * time;

            // Adjusts the grenade's position on the z-axis (forward/backward)
            point.z = startPos.z + startVel.z * time;


            if (!hitDetected)
            {
                if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out hit,
                        (point - lastPosition).magnitude))
                {
                    trajectory.calculatedPoints[i] = hit.point;
                    hitDetected = true;
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPositions(trajectory.calculatedPoints);
                    return;
                }
            }

            // If no collision, store the calculated point in the trajectory SO
            trajectory.calculatedPoints[i] = point;
            lastPosition = point; // Update the last position
        }

        // If no hit detected, just draw the full trajectory
        lineRenderer.positionCount = pointsCount;
        lineRenderer.SetPositions(trajectory.calculatedPoints);
    }
}