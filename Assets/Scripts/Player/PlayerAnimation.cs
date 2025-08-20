using Unity.Netcode;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PlayerAnimation : NetworkBehaviour
{
    public enum PlayerState
    {
        Idle,
        Run,
        Death
    }
    private PlayerState currentState;
    [SerializeField] private Animator animator;
     private readonly NetworkVariable<PlayerState> stateNetVar =
        new NetworkVariable<PlayerState>(
            PlayerState.Idle,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    
     private PlayerState lastRequested = (PlayerState)(-1);
    
    private void Awake()
    {
        FindAnimator();
    }
    
    public override void OnNetworkSpawn()
    {
        stateNetVar.OnValueChanged += OnStateChanged;
        // set start state on server so late-joiners get it
        if (IsServer)
            stateNetVar.Value = PlayerState.Idle;
        Debug.Log($"[PA][{(IsServer ? "SERVER" : $"CLIENT {NetworkManager.LocalClientId}")}] " +
                  $"Spawn NetId={NetworkObjectId}, Owner={OwnerClientId}, IsOwner={IsOwner}, IsSpawned={NetworkObject.IsSpawned}");
        PlayState(stateNetVar.Value);
    }
    
    public override void OnNetworkDespawn()
    {
        stateNetVar.OnValueChanged -= OnStateChanged;
    }
    
    private bool FindAnimator()
    {
        if (animator) return true;
        animator = GetComponent<Animator>()
                   ?? GetComponentInChildren<Animator>(true)
                   ?? GetComponentInParent<Animator>();
        return animator != null;
    }
    
    public void ServerSetState(PlayerState newState)
    {
        if (!IsServer) return;
        if (lastRequested == newState) return;
        if (stateNetVar.Value == newState) return;
        stateNetVar.Value = newState; // shows to everyone
    }
    
    public void RequestState(PlayerState newState)
    {
        if (lastRequested == newState) return;  // unspam updates to fix glitch
        if (stateNetVar.Value == newState) return;

        if (IsServer)
            ServerSetState(newState);          
        else
            SetStateServerRpc(newState);
    }


    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]  
    private void SetStateServerRpc(PlayerState newState)
    {
        if (stateNetVar.Value == newState) return; 
        stateNetVar.Value = newState;
    }
    private void OnStateChanged(PlayerState previous, PlayerState current)
    {
        lastRequested = current;
        PlayState(current);
    }
    
    private void PlayState(PlayerState state)
    {
        if (!FindAnimator()) return;
        switch (state)
        {
            case PlayerState.Idle:
                animator.Play("Idle");
                break;
            case PlayerState.Run:
                animator.Play("Run");
                break;
            case PlayerState.Death:
                animator.Play("Death");
                break;
        }
    }
}
