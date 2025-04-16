using UnityEngine;

public class AlienAI : AIBase
{
    public float searchRange;
    public float tagDistance;
    public Transform mothership;
    public bool isReached = false;

    [HideInInspector] public CivilianAI currentTargetCiv;

    protected override void Start()
    {
        base.Start();
        ChangeState(new SearchState(this));
    }

     void Update()
    {
        base.Update();
        if (isReached)
        {
            ChangeState(new SearchState(this));
        }
    }
}
