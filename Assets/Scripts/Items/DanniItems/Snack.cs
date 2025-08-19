using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode; 
using Defender;

public class Snack : UsableItem_Base
{
      [Header("Attraction")]
    [SerializeField] private float attractionRadius = 6f;

    [Header("Snack Object")]
    [SerializeField] private SnackObject snackObject;

    [Header("Visuals (Wrapper ↔ Snack)")]
    [SerializeField] private GameObject wrapperVisual;
    [SerializeField] private GameObject snackVisual;

    protected override void Awake()
    {
        base.Awake();
        activationCountdown = 0f;
        expiryDuration = 0f;
        if (snackObject == null)
            snackObject = GetComponentInChildren<SnackObject>(true);
        if (wrapperVisual) wrapperVisual.SetActive(true);
        if (snackVisual)   snackVisual.SetActive(false);
    }
    
    protected override void OnManualUse(CharacterBase characterTryingToUse)
    {
        TryStartActivationNow(force: true);
    }
    protected override void ActivateItem()
    {
        base.ActivateItem();
        bool makeSnackVisible = wrapperVisual != null && wrapperVisual.activeSelf;

        SetSnackVisualStateClientRpc(makeSnackVisible);
        if (IsServer && makeSnackVisible)
        {
            AttractAllCiviliansInRange_Server();
        }
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void SetSnackVisualStateClientRpc(bool snackShouldBeVisible)
    {
        if (wrapperVisual) wrapperVisual.SetActive(!snackShouldBeVisible);
        if (snackVisual)   snackVisual.SetActive(snackShouldBeVisible);
    }

    // Server - find all civs and lure them to the snack.
    private void AttractAllCiviliansInRange_Server()
    {
        if (snackObject == null) return;

        AIBase[] allCivilians = FindObjectsOfType<AIBase>();
        if (allCivilians == null || allCivilians.Length == 0) return;

        Vector3 snackPosition = snackObject.transform.position;

        for (int i = 0; i < allCivilians.Length; i++)
        {
            if (allCivilians[i] == null) continue;

            if (attractionRadius > 0f)
            {
                float squaredDistance =
                    (allCivilians[i].transform.position - snackPosition).sqrMagnitude;
                if (squaredDistance > attractionRadius * attractionRadius)
                    continue;
            }
            // change state on server
            allCivilians[i].ChangeState(new GetSnackState(allCivilians[i], snackObject.transform));
        }
    }
}
