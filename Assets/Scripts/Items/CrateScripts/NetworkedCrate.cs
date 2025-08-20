using System.Collections;
using System.Collections.Generic;
using Defender;
using Unity.Netcode;
using UnityEngine;

public class NetworkedCrate : NetworkBehaviour, IUsable
{
   [Header("Crate Configuration")]
    [SerializeField] private CrateItemDatabase itemDatabase;
    [SerializeField] private float itemGenerationCooldown = 3f;
    [SerializeField] private Transform itemSpawnPoint;
    [SerializeField] private float itemFloatHeight = 1.5f;
    
    [Header("UI References")]
    [SerializeField] private FloatingUI floatingUI;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip generateItemClip;
    [SerializeField] private AudioClip itemReadyClip;
    
    // Network Variables
    private NetworkVariable<bool> hasSpawnedItem = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isGenerating = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<NetworkObjectReference> spawnedItemRef = new NetworkVariable<NetworkObjectReference>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Local references
    private NetworkObject currentSpawnedItem;
    private Coroutine generationCoroutine;
    
    [Header("Debug Info")]
    [SerializeField] private bool debugMode = false;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Subscribe to network variable changes
        hasSpawnedItem.OnValueChanged += OnHasSpawnedItemChanged;
        isGenerating.OnValueChanged += OnIsGeneratingChanged;
        spawnedItemRef.OnValueChanged += OnSpawnedItemRefChanged;
        UpdateUIState();
        // start auto-generation cycle on server
        if (IsServer)
        {
            StartAutoGeneration();
        }
        if (debugMode)
            Debug.Log($"[NetworkedCrate] Spawned on {(IsServer ? "Server" : "Client")} - NetworkObjectId: {NetworkObjectId}");
    }
    
    public override void OnNetworkDespawn()
    {
        if (hasSpawnedItem != null)
        {
            hasSpawnedItem.OnValueChanged -= OnHasSpawnedItemChanged;
            isGenerating.OnValueChanged -= OnIsGeneratingChanged;
            spawnedItemRef.OnValueChanged -= OnSpawnedItemRefChanged;
        }
        if (IsServer)
        {
            StopAutoGeneration();
        }
        base.OnNetworkDespawn();
    }
    
    #region Network Variable Callbacks
    private void OnHasSpawnedItemChanged(bool prev, bool current)
    {
        UpdateUIState();
        if (debugMode)
            Debug.Log($"[NetworkedCrate] HasSpawnedItem changed: {prev} -> {current}");
    }
    
    private void OnIsGeneratingChanged(bool prev, bool current)
    {
        UpdateUIState();
        if (debugMode)
            Debug.Log($"[NetworkedCrate] IsGenerating changed: {prev} -> {current}");
    }
    
    private void OnSpawnedItemRefChanged(NetworkObjectReference prev, NetworkObjectReference current)
    {
        // update local ref
        if (current.TryGet(out NetworkObject itemNetObj))
        {
            currentSpawnedItem = itemNetObj;
            // set up item floating
            var floatingComponent = itemNetObj.GetComponent<NetworkedFloatingItem>();
            if (floatingComponent != null)
            {
                Vector3 floatPos = itemSpawnPoint.position + Vector3.up * itemFloatHeight;
                floatingComponent.SetFloatingPosition(floatPos);
            }
            if (debugMode)
                Debug.Log($"[NetworkedCrate] New item spawned: {itemNetObj.name}");
        }
        else
        {
            currentSpawnedItem = null;
            if (debugMode)
                Debug.Log($"[NetworkedCrate] Item reference cleared");
        }
        
        UpdateUIState();
    }
    #endregion
    
    #region IUsable Implementation
    public void Use(CharacterBase characterTryingToUse)
    {
        // crates are now auto-generating items
        if (debugMode)
            Debug.Log("[NetworkedCrate] Crates auto-generate items - use E to pick up spawned items only");
    }
    
    public void StopUsing()
    {

    }
    #endregion
    
    #region Auto-Generation System
    /// <summary>
    /// start generation cycle
    /// </summary>
    private void StartAutoGeneration()
    {
        if (!IsServer) return;
        // generate the first item
        if (generationCoroutine != null)
        {
            StopCoroutine(generationCoroutine);
        }
        generationCoroutine = StartCoroutine(AutoGenerationCycle());
        if (debugMode)
            Debug.Log("[NetworkedCrate] Started auto-generation cycle");
    }
    
    /// <summary>
    /// auto generates items when 'slot' is empty
    /// </summary>
    private IEnumerator AutoGenerationCycle()
    {
        while (true)
        {
            // wait until we don't have an item and aren't currently generating
            yield return new WaitUntil(() => !hasSpawnedItem.Value && !isGenerating.Value);
            // wait for the generation interval
            yield return new WaitForSeconds(itemGenerationCooldown);
            // Check AGAIN just in case
            if (!hasSpawnedItem.Value && !isGenerating.Value)
            {
                StartItemGeneration(); // now generate:D
            }
        }
    }
    
    /// <summary>
    /// this is called on despawn
    /// </summary>
    private void StopAutoGeneration()
    {
        if (generationCoroutine != null)
        {
            StopCoroutine(generationCoroutine);
            generationCoroutine = null;
        }
    }
    #endregion
    #region Item Generation
    private void StartItemGeneration()
    {
        if (!IsServer) return;
        
        if (itemDatabase == null || itemDatabase.itemPrefabs.Count == 0)
        {
            Debug.LogWarning("[NetworkedCrate] No item database or no items available!");
            return;
        }
        
        isGenerating.Value = true;
        PlayAudioClientRpc(0); // 0 = generate sound
        
        if (generationCoroutine != null)
        {
            StopCoroutine(generationCoroutine);
        }
        
        generationCoroutine = StartCoroutine(ItemGenerationCoroutine());
        
        if (debugMode)
            Debug.Log($"[NetworkedCrate] Started item generation, cooldown: {itemGenerationCooldown}s");
    }
    
    private IEnumerator ItemGenerationCoroutine()
    {
        yield return new WaitForSeconds(itemGenerationCooldown);
        // generate item
        GameObject itemPrefab = itemDatabase.GetRandomItemPrefab();
        if (itemPrefab != null)
        {
            Vector3 spawnPos = itemSpawnPoint.position + Vector3.up * itemFloatHeight;
            GameObject spawnedItemObj = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
            
            // add script in case forget
            if (spawnedItemObj.GetComponent<NetworkedFloatingItem>() == null)
            {
                spawnedItemObj.AddComponent<NetworkedFloatingItem>();
            }
            NetworkObject itemNetObj = spawnedItemObj.GetComponent<NetworkObject>();
            if (itemNetObj != null)
            {
                itemNetObj.Spawn();
                spawnedItemRef.Value = itemNetObj;
                hasSpawnedItem.Value = true;
                StartCoroutine(CheckItemPickup(itemNetObj));
                
                if (debugMode)
                    Debug.Log($"[NetworkedCrate] Successfully spawned item: {itemPrefab.name}");
            }
            else
            {
                Debug.LogError($"[NetworkedCrate] Item prefab {itemPrefab.name} doesn't have NetworkObject component!");
                Destroy(spawnedItemObj);
            }
        }
        
        isGenerating.Value = false;
        PlayAudioClientRpc(1); // 1 = item ready sound
    }
    
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
            {
                if (debugMode)
                    Debug.Log($"[NetworkedCrate] Item pickup detected - Distance: {distanceFromSpawn:F2}, Parent changed: {parentChanged}, Distance from crate: {distanceFromCrate:F2}");
                break;
            }
            
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
        
        if (debugMode)
            Debug.Log("[NetworkedCrate] Item was picked up, crate will auto-generate next item");
    }
    #endregion
    
    #region UI and Audio
    private void UpdateUIState()
    {
        if (floatingUI == null) return;
        
        if (hasSpawnedItem.Value)
        {
            floatingUI.Disable();
        }
        else if (isGenerating.Value)
        {
            floatingUI.Enable();
        }
        else
        {
            floatingUI.Enable();
        }
    }
    
    [ClientRpc]
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
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }
    #endregion
    
    #region Public Configuration Methods
    /// <summary>
    /// set the  database for this crate (called by GameManager during level setup)
    /// </summary>
    public void SetItemDatabase(CrateItemDatabase database)
    {
        itemDatabase = database;
        if (debugMode)
            Debug.Log($"[NetworkedCrate] Item database set: {database?.name}");
    }
    
    /// <summary>
    /// get current state of the crate
    /// </summary>
    public bool CanGenerateItem()
    {
        return !hasSpawnedItem.Value && !isGenerating.Value;
    }
    #endregion
}
