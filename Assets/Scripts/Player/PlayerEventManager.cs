using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerEventManager : MonoBehaviour
{
   public static PlayerEventManager instance;

    public PlayerEvents events = new PlayerEvents();
    [HideInInspector] public bool isReady = false;

    void OnEnable()
    {
        instance = this;
        events = new PlayerEvents();
        events.onIdle.AddListener(OnIdle);
        events.onMove.AddListener(OnMove);
        events.onShoot.AddListener(OnShoot);
        events.onRunShoot.AddListener(OnRunShoot);
        events.onDeath.AddListener(OnDeath);
    }

    void OnDisable()
    {
        events.onIdle.RemoveListener(OnIdle);
        events.onMove.RemoveListener(OnMove);
        events.onShoot.RemoveListener(OnShoot);
        events.onRunShoot.RemoveListener(OnRunShoot);
        events.onDeath.RemoveListener(OnDeath);
    }

    void Start()
    {
        isReady = true; 
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
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }

    // private void PlayAudio(string audioName)
    // {
    //     Debug.Log($"Playing video: {audioName}");
    // }
}
public class PlayerEvents
{
    public UnityEvent onIdle = new UnityEvent();
    public UnityEvent onMove = new UnityEvent();
    public UnityEvent onShoot = new UnityEvent();
    public UnityEvent onRunShoot = new UnityEvent();
    public UnityEvent onDeath = new UnityEvent();
}
