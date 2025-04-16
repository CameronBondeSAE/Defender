using UnityEngine;

public class CivilianAI : AIBase
{
    [HideInInspector] public Transform followTarget;
    public float followDistance;

    protected override void Start()
    {
        base.Start();
        ChangeState(new PatrolState(this));
    }
}
