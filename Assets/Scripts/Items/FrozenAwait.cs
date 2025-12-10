using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class FrozenAwait : MonoBehaviour
{
    public IEnumerator FunFreezeWait(float frozenTime)
    {
        
        NavMeshAgent agent1 = null;
        PlayerMovement agent2 = null;
        bool player = false;
        bool success = true;
        agent1 = GetComponent<NavMeshAgent>();
        if (agent1 == null)
        {
            agent2 = GetComponent<PlayerMovement>();
            player = true;
        }

        if (!player) agent1.enabled = false;
        else if (agent2 != null) agent2.enabled = false;
        else success = false;

        if (success)
        {
            yield return new WaitForSeconds(0.1f); // Small buffer for scrupt to set time
            yield return new WaitForSeconds(frozenTime);
            if (!player) agent1.enabled = true;
            else agent2.enabled = true;
        }
        Destroy(this);
    }
}
