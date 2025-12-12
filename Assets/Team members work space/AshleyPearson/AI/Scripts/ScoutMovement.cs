using AshleyPearson;
using UnityEngine;

public class ScoutMovement : AIBase
{
   public override void OnNetworkSpawn()
   {
      base.OnNetworkSpawn();
      
      if (!IsServer) {return;}
      Debug.Log("[ScoutMovement] Network has spawned, ready to move");
      
      //Fire event to tell other scout states that scout can act now
      //Issues occuring with (!IsServer) code pushing out NPC before server was active
      ScoutEvents.OnScoutReady?.Invoke();
   }

   public void MoveScout(Vector3 destination)
   {
      //Scout to be a bit faster than normal civs
      MoveSpeed = 5f; //IDK if I should change this from here
      
      //Move scout to location
      //Debug.Log("[ScoutMovement] Moving scout to scout location: " + destination);
      base.MoveTo(destination);
   }
}
