using UnityEngine;
using Unity.Netcode;

// We are not destroying anything, we are just aiming at it, destruction/despawning is handled elsewhere

public class LaserEyes : NetworkBehaviour
{
    [Header("Setup")]
    public Transform leftEye;
    public Transform rightEye;

    [Header("Laser Visual Settings")]
    [SerializeField] private float targetHeightOffset = 1.5f;
    [SerializeField] private float beamWidth = 0.05f;

    private LineRenderer leftLR;
    private LineRenderer rightLR;

    public Transform currentTarget;

    private void Awake()
    {
        leftLR = CreateLaserRenderer(leftEye.gameObject);
        rightLR = CreateLaserRenderer(rightEye.gameObject);
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;

        if (currentTarget == null)
        {
            DisableLaserClientRpc();
            return;
        }

        if (!currentTarget.gameObject.activeInHierarchy)
        {
            currentTarget = null;
            DisableLaserClientRpc();
            return;
        }

        Vector3 leftOrigin = leftEye.position;
        Vector3 rightOrigin = rightEye.position;
        Vector3 targetPos = currentTarget.position + Vector3.up * targetHeightOffset;

        ShowLaserClientRpc(leftOrigin, rightOrigin, targetPos);
    }

    // Server sets who we're aiming at

    public void SetLaserTarget(Transform target)
    {
        if (!IsServer)
            return;

        currentTarget = target;
    }

    // Server clears target and turns beams off everywhere
    public void ClearLaserTarget()
    {
        if (!IsServer)
            return;

        currentTarget = null;
        DisableLaserClientRpc();
    }

    // client side stuff
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void ShowLaserClientRpc(Vector3 leftOrigin, Vector3 rightOrigin, Vector3 targetPos)
    {
        RenderLaser(leftLR, leftOrigin, targetPos);
        RenderLaser(rightLR, rightOrigin, targetPos);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void DisableLaserClientRpc()
    {
        if (leftLR != null) leftLR.positionCount = 0;
        if (rightLR != null) rightLR.positionCount = 0;
    }

    // visuals
    private void RenderLaser(LineRenderer lr, Vector3 origin, Vector3 target)
    {
        if (lr == null)
            return;

        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, target);
    }

    private LineRenderer CreateLaserRenderer(GameObject host)
    {
        LineRenderer lr = host.AddComponent<LineRenderer>();

        lr.positionCount = 0;
        lr.startWidth = beamWidth;
        lr.endWidth = beamWidth;

        Color red = Color.red;
        lr.startColor = red;
        lr.endColor = red;

        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = red;

        return lr;
    }
}
