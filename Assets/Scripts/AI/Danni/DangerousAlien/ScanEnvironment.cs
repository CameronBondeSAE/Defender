using UnityEngine;
using Anthill.AI;

public class ScanEnvironment : AntAIState
{
    private DangerousAlienControl control;

    public override void Create(GameObject aGameObject)
    {
        control = aGameObject.GetComponent<DangerousAlienControl>();
    }

    public override void Enter()
    {
        if (control != null)
        {
            control.needsScan = false;
        }

        Finish();
    }
}