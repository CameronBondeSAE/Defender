using UnityEngine;
using AIAnimation;

public class AgroAlienAI : AIBase
{
    [Header("Attack Damage")]
    public float damageMin = 5f;
    public float damageMax = 10f;
    

    private AIAnimationController animController;

    protected override void Start()
    {
        base.Start();
        ChangeState(new PatrolState(this));
        animController = GetComponent<AIAnimationController>();
    }

    void Update()
    {
        base.Update();
    }

    public void StopMoving() => agent.isStopped = true;
    public void ResumeMoving() => agent.isStopped = false;
}
