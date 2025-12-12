using Anthill.AI;
using UnityEngine;

public class AttackStateScript : AntAIState
{
    public LaserEyes laserEyes;
    private AntiHeroAISense antiHeroAISense;
    private Animator animator;
    private NetworkedSoundController _soundController;

    [Header("Audio")]
    [SerializeField] private float laserSfxFadeOutDuration = 0.4f; // tweak in Inspector

    public override void Create(GameObject aGameObject)
    {
        laserEyes = aGameObject.GetComponent<LaserEyes>();
        antiHeroAISense = aGameObject.GetComponent<AntiHeroAISense>();
        animator = aGameObject.GetComponent<Animator>();
        _soundController = aGameObject.GetComponent<NetworkedSoundController>();
    }

    public override void Enter()
    {
        laserEyes.enabled = true;
        laserEyes.SetLaserTarget(antiHeroAISense.targetObjectToDestroy.transform);

        animator.Play("Attack");

        // Turn ON looping laser SFX (networked)
        if (_soundController != null)
        {
            _soundController.StartLaserLoop();
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (laserEyes.currentTarget != null)
        {
            Health targetHealth = laserEyes.currentTarget.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(1f);

                if (targetHealth.isDead)
                {
                    laserEyes.ClearLaserTarget();
                    Finish();
                }
            }
            else
            {
                laserEyes.ClearLaserTarget();
                Finish();
            }
        }
        else
        {
            Finish();
        }
    }

    public override void Exit()
    {
        // Stop laser beams
        laserEyes.ClearLaserTarget();
        laserEyes.enabled = false;

        // Fade OUT laser SFX (networked)
        if (_soundController != null)
        {
            _soundController.StopLaserLoop(laserSfxFadeOutDuration);
        }
    }
}
