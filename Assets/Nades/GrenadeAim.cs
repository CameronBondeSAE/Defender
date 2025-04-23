using UnityEngine;

public class GrenadeAim : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform throwPoint;
    public float launchForce = 15f;
    public int arcPoints = 30;
    public float timeBetweenPoints = 0.1f;

    public void StartAiming()
    {
        lineRenderer.enabled = true;
    }

    public void StopAiming()
    {
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (lineRenderer.enabled)
        {
            DrawArc();
        }
    }

    void DrawArc()
    {
        Vector3[] points = new Vector3[arcPoints];
        Vector3 startPos = throwPoint.position;
        Vector3 startVel = throwPoint.forward * launchForce;

        for (int i = 0; i < arcPoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 point = startPos + startVel * time;
            point.y += Physics.gravity.y * time * time / 2f;
            points[i] = point;
        }

        lineRenderer.positionCount = arcPoints;
        lineRenderer.SetPositions(points);
    }
}