using UnityEngine;

public class LaserEyes : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private string alienTag = "Alien";

    [Header("Laser Settings")]
    [SerializeField] private float maxLaserDistance = 30f;

    private LineRenderer leftLR;
    private LineRenderer rightLR;
    private VisionSystem visionSystem;

    private void Awake()
    {
        visionSystem = GetComponent<VisionSystem>();

        // Create the two line renderers
        leftLR = CreateLaserRenderer(leftEye.gameObject);
        rightLR = CreateLaserRenderer(rightEye.gameObject);
    }

    private LineRenderer CreateLaserRenderer(GameObject host)
    {
        LineRenderer lr = host.AddComponent<LineRenderer>();

        // Basic settings
        lr.positionCount = 0;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;

        // Red laser color (with emission)
        Color red = new Color(1f, 0f, 0f, 1f);
        lr.startColor = red;
        lr.endColor = red;

        // Use Unity's default unlit shader
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = red;

        return lr;
    }

    private void FixedUpdate()
    {
        if (visionSystem == null)
        {
            DisableBeams();
            return;
        }

        // Find closest alien through VisionSystem
        GameObject targetAlien;
        bool sawAlien = visionSystem.GetClosestVisibleObjectWithTag(alienTag, out targetAlien);

        if (!sawAlien || targetAlien == null)
        {
            DisableBeams();
            return;
        }

        Vector3 targetPos = targetAlien.transform.position;

        // Draw both lasers toward the target
        CastLaser(leftLR, leftEye.position, targetPos);
        CastLaser(rightLR, rightEye.position, targetPos);
    }

    private void CastLaser(LineRenderer lr, Vector3 origin, Vector3 targetPos)
    {
        Vector3 dir = (targetPos - origin).normalized;
        Vector3 endPoint = origin + dir * maxLaserDistance;

        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, endPoint);
    }

    private void DisableBeams()
    {
        leftLR.positionCount = 0;
        rightLR.positionCount = 0;
    }
}
