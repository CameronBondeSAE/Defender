using Anthill.AI;
using UnityEngine;

public class AttackStateScript : AntAIState
{
    public LaserEyes laserEyes;
    private AntiHeroAISense antiHeroAISense;
    private Animator animator;
    private NetworkedSoundController _soundController;

    private bool _attackSoundPlayed = false;

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
        if (_soundController != null && !_attackSoundPlayed)
        {
            _soundController.PlayAudioClip();
            _attackSoundPlayed = true;
        }
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (laserEyes.targetToDestroy != null)
        {
            laserEyes.targetToDestroy.GetComponent<Health>().TakeDamage(1);

            if (laserEyes.targetToDestroy.GetComponent<Health>().isDead)
            {
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
        laserEyes.targetToDestroy = null;
        laserEyes.DisableBeams();
        laserEyes.enabled = false;

        // reset for next time we enter Attack
        _attackSoundPlayed = false;
    }
}