using UnityEngine;

public class LaserEyes : MonoBehaviour
{
    [Header("Setup")]
    public Transform leftEye;
    public Transform rightEye;
    public GameObject targetToDestroy; 
    private LineRenderer leftLR;
    private LineRenderer rightLR;
    private VisionSystem visionSystem;
    
    [SerializeField] private float targetHeightOffset = 1.5f;

    private void Awake()
    {
        visionSystem = GetComponent<VisionSystem>();
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
        Color red = new Color(1f, 0f, 0f, 1f);
        lr.startColor = red;
        lr.endColor = red;
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = red;

        return lr;
    }

    private void FixedUpdate()
    {
        Vector3 target;
        
        if (visionSystem == null)
        {
            DisableBeams();
            return;
        }

        if (targetToDestroy != null)
        {
            target = targetToDestroy.transform.position + Vector3.up * targetHeightOffset;
            if (visionSystem.CanSeeObject(targetToDestroy))
            {
                CastLaser(leftLR, leftEye.position, target);
                CastLaser(rightLR, rightEye.position, target);
            }
            else
            {
                DisableBeams();
            }
        }
    }

    public void CastLaser(LineRenderer lr, Vector3 origin, Vector3 targetPos)
    {
        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, targetPos);
    }

    public void DisableBeams()
    {
        leftLR.positionCount = 0;
        rightLR.positionCount = 0;
    }
}
