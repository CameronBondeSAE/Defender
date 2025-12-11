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
        laserEyes.targetToDestroy = antiHeroAISense.targetObjectToDestroy;

        animator.Play("Attack");

        // Turn ON looping laser SFX (networked)
        if (_soundController != null)
        {
            _soundController.StartLaserLoop();
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (laserEyes.targetToDestroy != null)
        {
            Health targetHealth = laserEyes.targetToDestroy.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(1f);

                if (targetHealth.isDead)
                {
                    laserEyes.DisableBeams();
                    Finish();
                }
            }
            else
            {
                // No health component, just finish
                laserEyes.DisableBeams();
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
        laserEyes.targetToDestroy = null;
        laserEyes.DisableBeams();
        laserEyes.enabled = false;

        // Fade OUT laser SFX (networked)
        if (_soundController != null)
        {
            _soundController.StopLaserLoop(laserSfxFadeOutDuration);
        }
    }
}
