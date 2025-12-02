using UnityEngine;
using AIAnimation;
using System.Collections.Generic;

public class AlienAI : AIBase
{
    [Header("Alien Settings")]
    public float tagDistance; // Distance at which alien can "tag" (catch) a civilian
    public Transform mothership; // Reference to mothership (could be used for returning, escaping, etc.)
    public bool isReached = false; // Flag to check if destination is reached
    [HideInInspector] public AIBase currentTargetCiv; // Stores current civilian target (public but can't be messed with in inspector)
    [HideInInspector] public List<AIBase> escortingCivs = new List<AIBase>(); // Stores current civilian target (public but can't be messed with in inspector)
    
    [Header("Return Settings")]
    [Tooltip("how many extra times this alien will go out again after reaching the motheship, 0 = only one run")]
    public int returnCount = 0;
    private int remainingReturns;

    protected override void Start()
    {
        base.Start();
        remainingReturns = returnCount;
        // Start in Search State when spawned
        ChangeState(new SearchState(this));
    }

    protected override void Update()
    {
        base.Update();

        if (isReached)
        {
            isReached = false;
            if (remainingReturns > 0)
            {
                // go out again if he's got runs left
                remainingReturns--;
                ChangeState(new SearchState(this));
            }
            else
            {
                // if no more runs left, he'd be one with the level
                var gm = FindObjectOfType<DanniLi.GameManager>();
                if (gm != null && gm.IsServer)
                {
                    gm.OnAlienLeftLevel();
                }
                // remove from level
                Destroy(gameObject);
            }
        }
    }
    
    public void FleeFromPosition(Vector3 repellentPosition, float radius, float duration)
    {
        if (!(currentState is FleeState))
        {
            ChangeState(new FleeState(this, repellentPosition, radius, duration));
        }
    }
}
