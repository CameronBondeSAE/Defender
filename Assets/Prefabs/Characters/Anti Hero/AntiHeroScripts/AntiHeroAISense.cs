using Anthill.AI;
using UnityEngine;

public enum AntiHeroAIScenario
{
    SeeAlien = 0,
    IsAlienKilled = 1
}
public class AntiHeroAISense : MonoBehaviour, ISense
{
    [SerializeField] private VisionSystem visionSystem;
    public GameObject targetObjectToDestroy;
    private void Awake()
    {
        visionSystem = GetComponent<VisionSystem>();
    }
    public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
    {
        aWorldState.Set(AntiHeroAIScenario.SeeAlien, IsSeeAlien());
        aWorldState.Set(AntiHeroAIScenario.IsAlienKilled, IsAlienKilled());
    }

    private bool IsSeeAlien()
    {
        GameObject foundObj;
        
        if (visionSystem.GetClosestVisibleObjectWithTag("Damageable", out foundObj))
        {
            targetObjectToDestroy = foundObj;
            return true;
        }
        
        if (visionSystem.GetClosestVisibleObjectWithTag("Alien", out foundObj))
        {
            targetObjectToDestroy = foundObj;
            return true;
        }

        return false;
    }

    private bool IsAlienKilled()
    {
        return false;
    }
}
