using UnityEngine;
using AIAnimation;

public class AlienAI : AIBase
{
    [Header("Alien Settings")]
    public float tagDistance; // Distance at which alien can "tag" (catch) a civilian
    public Transform mothership; // Reference to mothership (could be used for returning, escaping, etc.)
    public bool isReached = false; // Flag to check if destination is reached
    [HideInInspector] public AIBase currentTargetCiv; // Stores current civilian target (public but can't be messed with in inspector)

    protected override void Start()
    {
        base.Start();

        // Start in Search State when spawned
        ChangeState(new SearchState(this));
    }
    
    void Update()
    {
        base.Update();

        // If the alien reached the mothership, switch back to searching
        if (isReached)
        {
            ChangeState(new SearchState(this));
        }
    }

    // Stop movement (using NavMesh built in bool)
    public void StopMoving()
    {
        agent.isStopped = true;
    }

    // Resume movement
    public void ResumeMoving()
    {
        agent.isStopped = false;
    }
}
