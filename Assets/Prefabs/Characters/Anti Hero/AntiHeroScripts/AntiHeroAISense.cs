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
    [SerializeField] private GameObject targetAlien;
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
         return visionSystem.GetClosestVisibleObjectWithTag("Alien", out targetAlien);
    }

    private bool IsAlienKilled()
    {
        return false;
    }
}
