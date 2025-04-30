using UnityEngine;

[CreateAssetMenu(fileName = "GrenadeTrajectory", menuName = "Grenade/Trajectory")]
public class GrenadeTrajectory : ScriptableObject
{
    [Header("Trajectory Settings")]
    public float launchForce = 9f;           // Force of the throw
    public float timeBetweenPoints = 0.1f;   // Time gap between points on the line
    public int arcPoints = 30;               // Number of points on the line


    public Vector3[] calculatedPoints;      // Store calculated trajectory points
}
