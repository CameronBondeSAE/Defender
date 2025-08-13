using System.Collections;
using UnityEngine;

public class AggressiveCiv : MonoBehaviour
{
    [SerializeField] private IdleCivilianAI idleCivilianAI;
    private bool breakoutTriggered = false;


    private void Start()
    {
        if (idleCivilianAI == null) idleCivilianAI = GetComponent<IdleCivilianAI>();
    }

    private void Update()
    {
        if (idleCivilianAI.IsAbducted && !breakoutTriggered)
        {
            breakoutTriggered = true;
            StartCoroutine(TriggerBreakout());
        }
    }

    private IEnumerator TriggerBreakout()
    {
        while (breakoutTriggered)
        {
            yield return new WaitForSeconds(4);

            if (Random.Range(0, 20) == 2)
            {
                idleCivilianAI.IsAbducted = false;
                breakoutTriggered = false;
            }
        }
        yield return null;
    }

}
