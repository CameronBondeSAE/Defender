using UnityEngine;
using Anthill.AI;

public class Action_Abducted : AntAIState
{
    //Needed an empty state so that the planner does not try to continue to action once abducted

    public override void Enter()
    {
        Debug.Log("[Abducted] Scout has been abducted. Yielding control.");
    }
}
