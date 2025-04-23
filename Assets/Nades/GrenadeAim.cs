using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeAim : MonoBehaviour
{
    [Header("Components")]
    [ SerializeField] private LineRenderer lineRenderer; // Draws the grenade trajectory
    [SerializeField] private Transform throwPoint; // Starting point for grenade throws
    [SerializeField] private PlayerInputHandler playerInputHandler; // Handles input events for aiming

    [Header("Grenade Trajectory Settings")]
    [SerializeField] private float launchForce = 9f; // How hard the grenade is thrown
    [SerializeField] private float timeBetweenPoints = 0.1f; // Makes the line renderer arc visually smoother (less = more curve)
    [SerializeField] private int arcPoints = 30; // Number of points used to draw the line renderer trajectory (more = more curve)

    [Header("Aiming")]
    private float horizontalAngle = 0f; // Allows grenade aiming left & right 
    [SerializeField] private float verticalAngle = 0f; // Allows grenade aiming up & down 
    [SerializeField] private float minVerticalAngle = 20f; // limits how low you can aim grenades
    [SerializeField] private float maxVerticalAngle = 35f; // limits how high you can aim grenades 

    [Header("Sensitivity")]
    [SerializeField] private float verticalSensitivity = 0.3f; // Up & down line renderer sensitivity
    [SerializeField] private float horizontalSensitivity = 0.3f; // Left & right line renderer sensitivity


    private bool isAiming = false;

    /// <summary>
    /// Finds the PlayerInputHandler and subscribes to the grenade aiming start and stop events.
    /// </summary>
    private void Awake()
    {
        playerInputHandler = FindObjectOfType<PlayerInputHandler>(); // Finds the PlayerInputHandler in the scene
        if (playerInputHandler != null)
        {
            playerInputHandler.onAimGrenadeStart += StartAiming; // Start aiming when the event is triggered
            playerInputHandler.onAimGrenadeStop += StopAiming; // Stop aiming when the event is triggered
        }
    }

    /// <summary>
    /// Unsubscribes from grenade aiming events
    /// </summary>
    private void OnDestroy()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.onAimGrenadeStart -= StartAiming;
            playerInputHandler.onAimGrenadeStop -= StopAiming;
        }
    }

    /// <summary>
    /// Starts the aiming process, locks the cursor and enables the trajectory line. (optional hides cursor)
    /// </summary>
    public void StartAiming()
    {
        isAiming = true;
        if (lineRenderer != null) lineRenderer.enabled = true; // Enables the LineRenderer to show the trajectory


        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center
        // Cursor.visible = false; // Hides cursor <--------- KEEP OR DELETE?? 
    }

    /// <summary>
    /// Stops the aiming process, unlocks the cursor and disables the trajectory line. (optional hides cursor)
    /// </summary>
    public void StopAiming()
    {
        isAiming = false;
        if (lineRenderer != null) lineRenderer.enabled = false; // Disables the LineRenderer 

        Cursor.lockState = CursorLockMode.None; // Unlocks cursor
        // Cursor.visible = false; // Hides cursor <--------- KEEP OR DELETE?? 
    }

    void Update()
    {
        if (isAiming && lineRenderer.enabled)
        {
            UpdateAimDirection(); // Updates the aim direction based on mouse movement
            DrawArc(); // Draw the trajectory arc using the line renderer
        }
    }

    /// <summary>
    /// Adjusts the vertical and horizontal aiming angles according to the mouse
    /// </summary>
    void UpdateAimDirection()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue(); // Gets the mouse position ( "X" axis + "Y" axis)

        // Adjusts vertical angle based on mouse movement and sensitivity
        verticalAngle += mouseDelta.y * verticalSensitivity;

        // Adjusts horizontal angle based on mouse movement and sensitivity
        horizontalAngle += mouseDelta.x * horizontalSensitivity;

        // Clamps the vertical angle between minimum and maximum limits
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);


        Vector3 direction = Quaternion.Euler(-verticalAngle, horizontalAngle, 0f) * Vector3.forward;
        throwPoint.rotation = Quaternion.LookRotation(direction);
    }

    
    /// <summary>
    /// Draws the grenade's trajectory arc using the LineRenderer.
    /// </summary>
    void DrawArc()
    {
        if (lineRenderer == null) return;

        Vector3[] points = new Vector3[arcPoints]; // Stores all the points along the visual arc
        Vector3 startPos = throwPoint.position; // Starting position of the arc (where the grenade is thrown from)

       
        float verticalRad = Mathf.Deg2Rad * verticalAngle;  // Converts vertical aim angle from degrees to radians
        float horizontalRad = Mathf.Deg2Rad * horizontalAngle; // Converts horizontal aim angle from degrees to radians

       
        Vector3 startVel = new Vector3  //Calculates the direction of the grenade throw
        (
            Mathf.Sin(horizontalRad) * Mathf.Cos(verticalRad) * launchForce, // Velocity in left/right direction
            Mathf.Sin(verticalRad) * launchForce, // Velocity in up/down direction
            Mathf.Cos(horizontalRad) * Mathf.Cos(verticalRad) * launchForce // Velocity in forward/backward direction
        );

        Vector3 gravity = Physics.gravity;  // Gets the gravity of the game world 

        for (int i = 0; i < arcPoints; i++) // Loop through each point in the arc
        {
            float time = i * timeBetweenPoints; // Calculate the time at each point

            Vector3 point = startPos + startVel * time; 
            
            // Adjusts the grenade's position on the Y axis (up/down)
             point.y = startPos.y + startVel.y * time + 0.5f * gravity.y * time * time;
            
             // Adjusts the grenade's position on "X" axis (left/right)
             point.x = startPos.x + startVel.x * time; 
            
             // Adjusts the grenade's position on the z-axis (forward/backward)
             point.z = startPos.z + startVel.z * time;

            points[i] = point;
        }

        // Sets the trajectory points to the LineRenderer
        lineRenderer.positionCount = arcPoints;
        lineRenderer.SetPositions(points);
    }
}