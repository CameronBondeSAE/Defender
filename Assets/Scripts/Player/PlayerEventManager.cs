using UnityEngine;
using UnityEngine.Events;

public class PlayerEventManager : MonoBehaviour
{
    public static PlayerEventManager instance;
    [HideInInspector] public bool isReady;

    public PlayerEvents events = new();

    private void Start()
    {
        isReady = true;
    }

    private void OnEnable()
    {
        instance = this;
        events = new PlayerEvents();
        events.onIdle.AddListener(OnIdle);
        events.onMove.AddListener(OnMove);
        events.onShoot.AddListener(OnShoot);
        events.onRunShoot.AddListener(OnRunShoot);
        events.onDeath.AddListener(OnDeath);
    }

    private void OnDisable()
    {
        events.onIdle.RemoveListener(OnIdle);
        events.onMove.RemoveListener(OnMove);
        events.onShoot.RemoveListener(OnShoot);
        events.onRunShoot.RemoveListener(OnRunShoot);
        events.onDeath.RemoveListener(OnDeath);
    }

    private void OnIdle()
    {
        SetAnimation("Idle");
    }

    private void OnMove()
    {
        SetAnimation("Run");
    }

    private void OnShoot()
    {
        SetAnimation("ReadyGun");
    }

    private void OnRunShoot()
    {
        SetAnimation("RunShoot");
    }

    private void OnDeath()
    {
        SetAnimation("Death");
    }

    private void SetAnimation(string animationName)
    {
        var animator = GetComponent<Animator>();
        if (animator != null) animator.Play(animationName);
    }

    // private void PlayAudio(string audioName)
    // {
    //     Debug.Log($"Playing video: {audioName}");
    // }
}

public class PlayerEvents
{
    public UnityEvent onDeath = new();
    public UnityEvent onIdle = new();
    public UnityEvent onMove = new();
    public UnityEvent onRunShoot = new();
    public UnityEvent onShoot = new();
}