using Anthill.AI;
using UnityEngine;

public class AttackStateScript : AntAIState
{
    public LaserEyes laserEyes;
    private AntiHeroAISense antiHeroAISense;
    private Animator animator;
    public override void Create(GameObject aGameObject)
    {
        laserEyes = aGameObject.GetComponent<LaserEyes>();
        antiHeroAISense = aGameObject.GetComponent<AntiHeroAISense>();
        animator = aGameObject.GetComponent<Animator>();
    }
    public override void Enter()
    {
       laserEyes.enabled = true;
       laserEyes.targetAlien = antiHeroAISense.latestSeenAlien;
       animator.Play("Attack");
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (laserEyes.targetAlien != null)
        {
            laserEyes.targetAlien.GetComponent<Health>().TakeDamage(1);
            if (laserEyes.targetAlien.GetComponent<Health>().isDead)
            {
                laserEyes.DisableBeams();
                Finish();
            }
        }
    }

    public override void Exit()
    {
        laserEyes.targetAlien = null;
        laserEyes.DisableBeams();
        laserEyes.enabled = false;
    }
}
