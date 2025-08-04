using mothershipScripts;
using UnityEngine;

public class MothershipDropZone : MonoBehaviour
{
    public MothershipBase mothership;
    public Transform DropOffZone;

    private void OnTriggerEnter(Collider other)
    {
        var civ = other.GetComponent<AIBase>();
        if (civ != null && civ.IsAbducted)
        {
            if (civ.escortingAlien != null)
            {
                civ.escortingAlien.currentTargetCiv = null;
                civ.escortingAlien = null;
            }
            civ.ChangeState(new MoveToBeamState(civ, mothership.transform.position));
            // play sound & mark score here
            mothership.OnCivilianCollected(civ); 
        }
    }
}
