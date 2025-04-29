using UnityEngine;

public class IdleCivilianAI : AIBase
{
    [HideInInspector] public Transform followTarget;

    //public bool isCaptured = false;
    protected override void Start()
    {
        base.Start();
        ChangeState(new IdleState(this));
    }
}
