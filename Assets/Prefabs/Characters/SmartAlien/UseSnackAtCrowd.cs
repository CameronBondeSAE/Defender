using Anthill.AI;
using Defender;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// When in this state, he'll walk to the civ crowd center, use the snack item when in range, then set the escort bools so the planner transitions into the escort
/// </summary>
public class UseSnackAtCrowd : AntAIState
{
    private SmartAlienControl control;
    private NavMeshAgent agent;
    private CharacterBase character;
    private Vector3 crowdTarget;
    private bool hasValidCrowd;

    public override void Create(GameObject aGameObject)
    {
        control   = aGameObject.GetComponent<SmartAlienControl>();
        agent     = aGameObject.GetComponent<NavMeshAgent>();
        character = aGameObject.GetComponent<CharacterBase>();
    }

    public override void Enter()
    {
        if (control == null)
        {
            Finish();
            return;
        }

        if (control.currentCivGroup == null ||
            control.currentCivGroup.Count < control.minCivCrowdSize)
        {
            hasValidCrowd = false;
            Finish();
            return;
        }

        hasValidCrowd = true;
        crowdTarget = control.currentCrowdCenter;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(crowdTarget);
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (!hasValidCrowd)
        {
            Finish();
            return;
        }

        if (agent == null || !agent.enabled)
        {
            Finish();
            return;
        }

        float dist = Vector3.Distance(agent.transform.position, crowdTarget);
        if (dist <= control.interactRange)
        {
            UsableItem_Base snack = control.heldItem;

            if (snack != null &&
                snack.RoleForAI == UsableItem_Base.ItemRoleForAI.Snack &&
                character != null)
            {
                snack.Use(character);
                control.heldItem = null; // clear hold item
            }
            else
            {
                Debug.LogWarning("missing snack item");
            }

            agent.isStopped = true;
            Finish();
        }
    }

    public override void Exit()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }
        // when snack is used:
        control.snackDeployed   = true;
        control.escortInProgress = true;
        control.civsAtMothership = false;

    }
}
