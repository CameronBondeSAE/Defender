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
                //civ.escortingAlien.currentTargetCiv = null;
                //civ.escortingAlien = null;
                Destroy(civ.escortingAlien.gameObject);
            }
            Vector3 suckUpPos = GetComponentInParent<MothershipBase>().alienSpawnPosition;
            civ.ChangeState(new MoveToBeamState(civ, suckUpPos));
            // play sound & mark score here
            mothership.OnCivilianCollected(civ); 
        }
    }
}
