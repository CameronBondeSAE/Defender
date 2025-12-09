using Anthill.AI;
using UnityEngine;

public class AttackStateScript : AntAIState
{
    private GameObject targetAlien;
    private LaserEyes laserEyes;
    private VisionSystem visionSystem;
    public override void Create(GameObject aGameObject)
    {
        visionSystem =  aGameObject.GetComponent<VisionSystem>();
        laserEyes = aGameObject.GetComponent<LaserEyes>();
    }
    public override void Enter()
    {
       
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        
    }

    public override void Exit()
    {
        
    }
}
