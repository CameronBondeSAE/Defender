using System;
using UnityEngine;

public class AreaStateDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
	    AlienAI alienAI = other.GetComponent<AlienAI>();
	    if (alienAI != null)
	    {
		    if (alienAI.CurrentState is ReturnState)
		    {
			    // Is an alien RETURNING to the ship
			    Debug.Log("ALIEN RETURNING TO SHIP");
		    }
	    }
    }
}
