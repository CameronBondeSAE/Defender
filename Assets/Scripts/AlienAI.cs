using UnityEngine;

public class AlienAI : AIBase
{
    public float searchRange;
    public float tagDistance;
    public Transform mothership;

    [HideInInspector] public CivilianAI currentTargetCiv;

    protected override void Start()
    {
        base.Start();
        ChangeState(new SearchState(this));
    }
}
