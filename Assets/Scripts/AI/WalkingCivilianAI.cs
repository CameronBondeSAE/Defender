using UnityEngine;

public class WalkingCivilianAI : AIBase
{
    [HideInInspector] public Transform followTarget;
    //public bool isCaptured = false;
    protected override void Start()
    {
        base.Start();
        ChangeState(new PatrolState(this));
    }
}
