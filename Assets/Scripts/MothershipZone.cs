using UnityEngine;

public class MothershipZone : MonoBehaviour
{ 
    private void OnTriggerEnter(Collider other)
        {
            CivilianAI civ = other.GetComponent<CivilianAI>();
            if (civ != null)
            {
                civ.ChangeState(new IdleState(civ));
            }
        }
    
}
