using Anthill.AI;
using UnityEngine;
using UnityEngine.Serialization;

public enum AntiHeroAIScenario
{
    SeeAlien = 0,
    IsAlienKilled = 1
}
public class AntiHeroAISense : MonoBehaviour, ISense
{
    [SerializeField] private VisionSystem visionSystem;
    public GameObject latestSeenAlien;
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
         return visionSystem.GetClosestVisibleObjectWithTag("Alien", out latestSeenAlien);
    }

    private bool IsAlienKilled()
    {
        return false;
    }
}
