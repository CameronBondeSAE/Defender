using System;
using System.Collections;
using System.Collections.Generic;
using Defender;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// this crate spawns a random item on a countdown, lets players pick it up,
/// then resets and repeats. the server owns all timers and spawning; clients get ui + audio
/// </summary>
public class NetworkedCrate : NetworkBehaviour, IUsable
{
     [Header("Crate Config")]
    [SerializeField] private CrateItemDatabase itemDatabase;
    [SerializeField] private float itemGenerationCooldown = 3f;
    [SerializeField] private Transform itemSpawnPoint;
    [SerializeField] private float itemFloatHeight = 1.5f;
    [SerializeField] private float pickupRange = 4f;
    [SerializeField] private BoxCollider pickupTrigger;

    [Header("UI")]
    [SerializeField] private GameObject countdownUIPrefab;       
    private CountdownUI countdownUIInstance;                   
    private GameObject countdownUIObj;                             

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip generateItemClip;
    [SerializeField] private AudioClip itemReadyClip;

    // Network Variables
    private NetworkVariable<bool> hasSpawnedItem = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> isGenerating = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<NetworkObjectReference> spawnedItemRef = new NetworkVariable<NetworkObjectReference>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> remainingSeconds = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> countdownActive = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly HashSet<PlayerInventory> nearbyInventories = new();      
    private readonly Dictionary<PlayerInventory, Action> inventoryHandlers = new(); 

    // Local refs
    private NetworkObject currentSpawnedItem;
    private Coroutine countdownCoroutine; // NEW

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        hasSpawnedItem.OnValueChanged += OnHasSpawnedItemChanged;
        isGenerating.OnValueChanged += OnIsGeneratingChanged;
        spawnedItemRef.OnValueChanged += OnSpawnedItemRefChanged;
        remainingSeconds.OnValueChanged += OnRemainingSecondsChanged; 
        countdownActive.OnValueChanged += OnCountdownActiveChanged;   
        SpawnLocalCountdownUI();
        UpdateUIState();
        if (IsServer && !hasSpawnedItem.Value && !isGenerating.Value && !countdownActive.Value)
        {
            StartSpawnCountdown();
        }
    }

    public override void OnNetworkDespawn()
    {
        hasSpawnedItem.OnValueChanged -= OnHasSpawnedItemChanged;
        isGenerating.OnValueChanged -= OnIsGeneratingChanged;
        spawnedItemRef.OnValueChanged -= OnSpawnedItemRefChanged;
        remainingSeconds.OnValueChanged -= OnRemainingSecondsChanged; 
        countdownActive.OnValueChanged -= OnCountdownActiveChanged;   
        if (IsServer)
        {
            foreach (var kv in inventoryHandlers)
            {
                var inv = kv.Key;
                var handler = kv.Value;
                if (inv == null) continue;
                var ih = inv.GetComponent<PlayerInputHandler2>();
                if (ih != null) ih.onInventory -= handler;
            }
            inventoryHandlers.Clear();

            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
        }
        nearbyInventories.Clear();
        if (countdownUIObj != null)
        {
            Destroy(countdownUIObj);
            countdownUIObj = null;
            countdownUIInstance = null;
        }

        base.OnNetworkDespawn();
    }
    private void SpawnLocalCountdownUI()
    {
        if (countdownUIObj != null || countdownUIPrefab == null || itemSpawnPoint == null) return;

        countdownUIObj = Instantiate(countdownUIPrefab, itemSpawnPoint.position, Quaternion.identity, transform);
        countdownUIInstance = countdownUIObj.GetComponent<CountdownUI>();
        if (countdownUIInstance != null && countdownUIInstance.countdownText != null)
        {
            countdownUIInstance.countdownText.color = countdownUIInstance.crateCountdownColor;
            countdownUIInstance.Hide(); // only visible while counting down
        }
    }

    #region Network Variable Callbacks
    private void OnHasSpawnedItemChanged(bool prev, bool current) => UpdateUIState();
    private void OnIsGeneratingChanged(bool prev, bool current) => UpdateUIState();

    private void OnRemainingSecondsChanged(int previouse, int current) 
    {
        if (countdownUIInstance != null && countdownActive.Value)
            countdownUIInstance.SetCountdown(current);
    }

    private void OnCountdownActiveChanged(bool prev, bool current) 
    {
        UpdateUIState();
        if (current && countdownUIInstance != null)
            countdownUIInstance.SetCountdown(remainingSeconds.Value);
    }

    private void OnSpawnedItemRefChanged(NetworkObjectReference prev, NetworkObjectReference current)
    {
        if (current.TryGet(out NetworkObject itemNetObj))
        {
            currentSpawnedItem = itemNetObj;
            // floating while waiting in crate
            var floating = itemNetObj.GetComponent<NetworkedFloatingItem>();
            if (floating != null)
            {
                Vector3 floatPos = itemSpawnPoint.position + Vector3.up * itemFloatHeight;
                floating.SetFloatingPosition(floatPos);
            }
        }
        else
        {
            currentSpawnedItem = null;
        }
        UpdateUIState();
    }
    #endregion

    #region Trigger & give item
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory == null) return;

        nearbyInventories.Add(inventory);

        // subscribe to the inventory pick up key press
        var playerInput = inventory.GetComponent<PlayerInputHandler2>();
        if (playerInput != null && !inventoryHandlers.ContainsKey(inventory))
        {
            Action handler = () => OnPickupKeyPressed(inventory); 
            inventoryHandlers[inventory] = handler;
            playerInput.onInventory += handler;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        var inv = other.GetComponentInParent<PlayerInventory>();
        if (inv == null) return;

        nearbyInventories.Remove(inv);

        var ih = inv.GetComponent<PlayerInputHandler2>();
        if (ih != null && inventoryHandlers.TryGetValue(inv, out var handler))
        {
            ih.onInventory -= handler;
            inventoryHandlers.Remove(inv);
        }
    }
    private void OnPickupKeyPressed(PlayerInventory inventory)
    {
        if (!IsServer) return;
        if (inventory == null) return;
        if (!nearbyInventories.Contains(inventory)) return;
        if (!hasSpawnedItem.Value || currentSpawnedItem == null) return;
        IPickup itemPickup = currentSpawnedItem.GetComponent<IPickup>();
        if (itemPickup == null)
            itemPickup = currentSpawnedItem.GetComponentInChildren<IPickup>(true);
        if (itemPickup == null) return;
        // STOP floating once it belongs to player
        var floating = currentSpawnedItem.GetComponent<NetworkedFloatingItem>();
        if (floating == null)
            floating = currentSpawnedItem.GetComponentInChildren<NetworkedFloatingItem>(true);
        if (floating != null)
        {
            floating.OnPickedUp();  
            floating.enabled = false; 
        }
        // give it to player inventory (ryPickupItem handles parenting/physics/colliders)
        bool hasItem = inventory.TryPickupItem(itemPickup);
        if (hasItem)
            OnItemPickedUp(); 
    }
    #endregion

    public void Use(CharacterBase characterTryingToUse) { }
    public void StopUsing() { }

    #region Countdown → Spawn (int seconds; no doubles)
    private void StartSpawnCountdown()
    {
        if (!IsServer) return;
        if (hasSpawnedItem.Value || isGenerating.Value || countdownActive.Value) return;

        int startSeconds = Mathf.Max(1, Mathf.CeilToInt(itemGenerationCooldown));
        remainingSeconds.Value = startSeconds;
        countdownActive.Value = true;

        PlayAudioClientRpc(0); 

        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownThenSpawn());
    }

    private IEnumerator CountdownThenSpawn() 
    {
        while (remainingSeconds.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingSeconds.Value = Mathf.Max(0, remainingSeconds.Value - 1);
        }
        countdownCoroutine = null;
        countdownActive.Value = false;
        StartItemGenerationImmediate();
    }

    private void StartItemGenerationImmediate() 
    {
        if (!IsServer) return;

        isGenerating.Value = true;

        if (itemDatabase == null || itemDatabase.itemPrefabs.Count == 0)
        {
            isGenerating.Value = false;
            return;
        }

        GameObject itemPrefab = itemDatabase.GetRandomItemPrefab();
        if (itemPrefab == null)
        {
            isGenerating.Value = false;
            return;
        }
        Vector3 spawnPos = itemSpawnPoint.position + Vector3.up * itemFloatHeight;
        GameObject spawnedItemObj = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        if (spawnedItemObj.GetComponent<NetworkedFloatingItem>() == null)
            spawnedItemObj.AddComponent<NetworkedFloatingItem>(); // float while in crate

        NetworkObject itemNetObj = spawnedItemObj.GetComponent<NetworkObject>();
        if (itemNetObj != null)
        {
            itemNetObj.Spawn();
            spawnedItemRef.Value = itemNetObj;
            currentSpawnedItem = itemNetObj;
            hasSpawnedItem.Value = true;
            StartCoroutine(CheckItemPickup(itemNetObj));
        }
        else
        {
            Destroy(spawnedItemObj);
        }

        isGenerating.Value = false;
        PlayAudioClientRpc(1); 
    }
    #endregion

    #region Backup pickup monitor (movement/parent change)
    private IEnumerator CheckItemPickup(NetworkObject itemNetObj)
    {
        Vector3 originalPosition = itemNetObj.transform.position;
        Transform originalParent = itemNetObj.transform.parent;

        while (itemNetObj != null && itemNetObj.IsSpawned)
        {
            float distanceFromSpawn = Vector3.Distance(itemNetObj.transform.position, originalPosition);
            bool parentChanged = itemNetObj.transform.parent != originalParent;
            float distanceFromCrate = Vector3.Distance(itemNetObj.transform.position, itemSpawnPoint.position);

            if (distanceFromSpawn > 1f || parentChanged || distanceFromCrate > 3f)
                break;

            yield return new WaitForSeconds(0.2f);
        }
        OnItemPickedUp();
    }

    private void OnItemPickedUp()
    {
        if (!IsServer) return;

        hasSpawnedItem.Value = false;
        spawnedItemRef.Value = default;
        currentSpawnedItem = null;
        StartSpawnCountdown(); 
    }
    #endregion

    #region UI and Audio
    private void UpdateUIState()
    {
        if (countdownUIInstance == null) return;

        bool showCountdown =
            countdownActive.Value &&
            !hasSpawnedItem.Value &&
            !isGenerating.Value;

        if (showCountdown)
        {
            countdownUIInstance.Show();
            countdownUIInstance.SetCountdown(remainingSeconds.Value);
        }
        else
        {
            countdownUIInstance.Hide();
        }
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Unreliable)]
    private void PlayAudioClientRpc(int audioType)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = audioType switch
        {
            0 => generateItemClip,
            1 => itemReadyClip,
            _ => null
        };

        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay);
    }
    #endregion

    #region Helper/Config
    public void SetItemDatabase(CrateItemDatabase database) => itemDatabase = database;

    public bool CanGenerateItem() 
    {
        return !hasSpawnedItem.Value && !isGenerating.Value && !countdownActive.Value;
    }
    #endregion
}
