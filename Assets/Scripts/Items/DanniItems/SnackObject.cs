using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnackObject : MonoBehaviour
{
    [Header("Snack Settings")]
    public float maxHealth = 10f; 
    public float biteDamage = 2f; 
    public float eatingInterval = 1f;

    private float currentHealth;
    private bool isActive = false;

    private Coroutine eatingRoutine;
    private List<AIBase> snackers = new List<AIBase>();

    private Snack owner;
    private MonoBehaviour ownerMono;

    public void Setup(Snack owner, MonoBehaviour ownerMono)
    {
        this.owner = owner;
        this.ownerMono = ownerMono;
        currentHealth = maxHealth;
    }

    public void OnSnackDropped(int numCivs)
    {
        isActive = true;
        currentHealth = maxHealth;
        // lure civs
        AttractCivilians(numCivs);
    }

    private void AttractCivilians(int numCivs)
    {
        // find all non-captured civs
        var allCivs = GameObject.FindGameObjectsWithTag("Civilian");
        List<AIBase> eligibleCivs = new List<AIBase>();
        foreach (var obj in allCivs)
        {
            var civ = obj.GetComponent<AIBase>();
            if (civ != null && !civ.IsAbducted && !(civ.CurrentState is GetSnackState))
                eligibleCivs.Add(civ);
        }

        if (eligibleCivs.Count == 0) return;
        int count = Mathf.Min(numCivs, eligibleCivs.Count);
        // shuffle civs & pick n
        for (int i = 0; i < eligibleCivs.Count; i++)
        {
            var temp = eligibleCivs[i];
            int randomIndex = Random.Range(i, eligibleCivs.Count);
            eligibleCivs[i] = eligibleCivs[randomIndex];
            eligibleCivs[randomIndex] = temp;
        }

        for (int i = 0; i < count; i++)
        {
            var civ = eligibleCivs[i];
            civ.ChangeState(new GetSnackState(civ, transform));
            snackers.Add(civ);
        }
    }
    public bool TakeBite()
    {
        // play eat animation & sound
        if (!isActive) return false;
        currentHealth -= biteDamage;
        if (currentHealth <= 0)
        {
            StartCoroutine(DestroyAfterDelay());
            isActive = false;
            return false;
        }
        return true;
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
