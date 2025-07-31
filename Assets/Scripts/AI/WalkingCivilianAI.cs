using UnityEngine;

/// <summary>
/// Walking civ will be initialized in Patrol state. Most of his functionality will be covered in AIBase
/// </summary>
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
