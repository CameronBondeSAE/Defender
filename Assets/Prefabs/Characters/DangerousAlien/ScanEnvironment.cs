using UnityEngine;
using Anthill.AI;
/// <summary>
/// basically copying SmartAlien's scan state; he'll wait for scanDuration and clears the needsScan bool.
/// The point is mostly to satisfy the planner condition so it can treat scanning as a completed action and move on
/// </summary>
public class ScanEnvironment : AntAIState
{
    private DangerousAlienControl control;
    private float timer;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<DangerousAlienControl>();
    }
    public override void Enter()
    {
        timer = 0f;
        if (control != null)
        {
            control.needsScan = false;  
        }
    }
    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (control == null)
        {
            Finish();
            return;
        }

        timer += aDeltaTime * aTimeScale;

        if (timer >= control.scanDuration)
        {
            control.needsScan = false;
            Finish();
        }
    }

    public override void Exit()
    {
    }
}