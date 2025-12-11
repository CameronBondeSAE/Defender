using Anthill.AI;
using UnityEngine;

public class IdleStateScript : AntAIState
{
    private VisionSystem visionSystem;
    public LaserEyes laserEyes;
    private Animator animator;
    private NetworkedSoundController soundController;

    [Header("Voiceline Settings")]
    [SerializeField] private bool playVoicelinesInIdle = true;
    [SerializeField] private float minDelayBetweenLines = 3f;
    [SerializeField] private float maxDelayBetweenLines = 6f;

    private float idleTimer = 0f;
    private float nextVoiceDelay = 0f;

    public override void Create(GameObject aGameObject)
    {
        visionSystem = aGameObject.GetComponent<VisionSystem>();
        laserEyes = aGameObject.GetComponent<LaserEyes>();
        animator = aGameObject.GetComponent<Animator>();
        soundController = aGameObject.GetComponent<NetworkedSoundController>();
    }

    public override void Enter()
    {
        laserEyes.enabled = false;
        animator.Play("Idle1");

        idleTimer = 0f;
        nextVoiceDelay = Random.Range(minDelayBetweenLines, maxDelayBetweenLines);

        // say a line as soon as we enter idle
        if (playVoicelinesInIdle && soundController != null)
        {
            soundController.PlayVoicelinesAudioClip();
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        GameObject targetObj;
        bool sawAlien = visionSystem.GetClosestVisibleObjectWithTag("Alien", out targetObj);

        if (sawAlien == true)
        {
            Finish();
            return;
        }

        // No alien, we are chilling so time for voicelines
        if (!playVoicelinesInIdle || soundController == null)
        {
            return;
        }

        idleTimer += aDeltaTime;

        if (idleTimer >= nextVoiceDelay)
        {
            soundController.PlayVoicelinesAudioClip();
            idleTimer = 0f;
            nextVoiceDelay = Random.Range(minDelayBetweenLines, maxDelayBetweenLines);
        }
    }

    public override void Exit() {}
}
