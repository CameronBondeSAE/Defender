using Anthill.AI;
using UnityEngine;

public class IdleStateScript : AntAIState
{
    private VisionSystem visionSystem;
    public LaserEyes laserEyes;
    private Animator animator;
    public override void Create(GameObject aGameObject)
    {
        visionSystem =  aGameObject.GetComponent<VisionSystem>();
        laserEyes = aGameObject.GetComponent<LaserEyes>();
        animator = aGameObject.GetComponent<Animator>();
    }
    public override void Enter()
    {
       laserEyes.enabled = false;
       animator.Play("Idle1");
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        GameObject targetAlien;
        bool sawAlien = visionSystem.GetClosestVisibleObjectWithTag("Alien", out targetAlien);

        if (sawAlien == true)
        {
            Finish();
        }
    }

    public override void Exit()
    {
        
    }
}
