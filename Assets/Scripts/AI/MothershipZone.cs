using UnityEngine;

public class MothershipZone : MonoBehaviour
{ 
    public float alienDistanceThreshold = 1f;
    public float civDistanceThreshold = 3f;

    private void Update()
    {
        AlienAI[] aliens = FindObjectsOfType<AlienAI>();
        foreach (AlienAI alien in aliens)
        {
            if (!alien.isReached && IsAlienAtMothership(alien.transform))
            {
                alien.isReached = true;
            }
        }
        
        GameObject[] civs = GameObject.FindGameObjectsWithTag("Civilian");
        foreach (GameObject civ in civs)
        {
            if (IsCivAtMothership(civ.transform))
            {
                civ.SetActive(false);
            }
        }
    }

    public bool IsAlienAtMothership(Transform alienTransform)
    {
        return Vector3.Distance(transform.position, alienTransform.position) < alienDistanceThreshold;
    }

    public bool IsCivAtMothership(Transform civTransform)
    {
        return Vector3.Distance(transform.position, civTransform.position) < civDistanceThreshold;
    }
}
