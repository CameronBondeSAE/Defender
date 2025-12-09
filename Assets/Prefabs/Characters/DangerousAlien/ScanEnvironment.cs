using UnityEngine;
using Anthill.AI;

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