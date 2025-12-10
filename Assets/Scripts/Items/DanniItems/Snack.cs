using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode; 
using Defender;

public class Snack : UsableItem_Base
{
      [Header("Attraction")]
    [SerializeField] private float attractionRadius = 6f;
    [SerializeField] private float attractionInterval = 0.2f; // how often to rescan
    private Coroutine attractionLoopRoutine;

    [Header("Snack Object")]
    [SerializeField] private SnackObject snackObject;

    [Header("Visuals (Wrapper/Burger)")]
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
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (attractionLoopRoutine != null)
        {
            StopCoroutine(attractionLoopRoutine);
            attractionLoopRoutine = null;
        }
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
            AttractAllCiviliansInRangeServer(); // attract immediately once
            StartAttractionLoopServer(); // then continue to attract as long s is unwrapped
        }
    }
    
    private void StartAttractionLoopServer()
    {
        if (!IsServer) return;

        // no double-start
        if (attractionLoopRoutine != null)
            StopCoroutine(attractionLoopRoutine);

        attractionLoopRoutine = StartCoroutine(AttractionLoop());
    }
    private IEnumerator AttractionLoop()
    {
        var wait = new WaitForSeconds(attractionInterval);

        // keep going while it's server + have a snack object with health + and it’s not dead
        while (IsServer &&
               snackObject != null &&
               snackObject.snackHealth != null &&
               !snackObject.snackHealth.isDead)
        {
            AttractAllCiviliansInRangeServer();
            yield return wait;
        }

        attractionLoopRoutine = null;
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void SetSnackVisualStateClientRpc(bool snackShouldBeVisible)
    {
        if (wrapperVisual) wrapperVisual.SetActive(!snackShouldBeVisible);
        if (snackVisual)   snackVisual.SetActive(snackShouldBeVisible);
    }

    // Server - find all civs and lure them to the snack.
    private void AttractAllCiviliansInRangeServer()
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
