using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Snack : UsableItem_Base
{
    [Header("Snack Params")] [SerializeField]
    private int civiliansToAttract = 3;
    [SerializeField] private float searchRadius = 0f;
    [SerializeField] private float attractDelay = 0.1f;

    [Header("Snack Obj")] 
    [SerializeField] private SnackObject snackObject; // todo

    protected override void Awake()
    {
        base.Awake();
        activationCountdown = 0f;

        if (snackObject == null)
            snackObject = GetComponentInChildren<SnackObject>();
    }

    public override void Use()
    {
        Vector3 dir = CurrentCarrier != null ? CurrentCarrier.forward : transform.forward;
        Launch(dir, launchForce);
        base.Use();
    }

    protected override void ActivateItem()
    {
        SetCollidersEnabled(true);
        if(isActiveAndEnabled)
            StartCoroutine(AttractAfterDelay());
        else
        {
            AttractCivilians();
        }
    }
    private System.Collections.IEnumerator AttractAfterDelay()
    {
        float time = attractDelay;
        while (time > 0f)
        {
            time -= Time.deltaTime;
            yield return null;
        }
        AttractCivilians();
    }

    private void AttractCivilians()
    {
        if (snackObject == null) return;
        AIBase[] all = FindObjectsOfType<AIBase>();
        List<AIBase> candidates = new List<AIBase>();
        if (searchRadius > 0f)
        {
            Vector3 snackPos = snackObject.transform.position;
            for (int i = 0; i < all.Length; i++)
            {
                if(Vector3.SqrMagnitude(all[i].transform.position - snackPos) < searchRadius * searchRadius)candidates.Add(all[i]);
            }
        }
        else
        {
            candidates.AddRange(all);
        }

        if (candidates.Count == 0) return;
        int civilliansPicked = 0;
        while (candidates.Count > 0 && civilliansPicked < civiliansToAttract)
        {
            int randomIndex = Random.Range(0, candidates.Count);
            AIBase chosenCivilian = candidates[randomIndex];
            chosenCivilian.ChangeState(new GetSnackState(chosenCivilian, snackObject.transform));
            candidates.RemoveAt(randomIndex);
            civilliansPicked++;
        }

    }

}
