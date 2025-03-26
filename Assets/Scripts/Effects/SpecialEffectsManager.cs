using UnityEngine;

public class SpecialEffectsManager : MonoBehaviour
{
    [Header("Effect Types")]
    public EffectType effectType;
    public enum EffectType
    {
        Blood,
        AlienBlood,
        Wood,
        Metal,
        Fire
    }
    [Header("Particle Systems")]
    public ParticleSystem bloodParticles;
    public ParticleSystem alienBloodParticles;
    public ParticleSystem woodParticles;
    public ParticleSystem metalParticles;
    public ParticleSystem fireParticles;

    private TestHealth health;

    void Awake()
    {
        health = GetComponent<TestHealth>();
        if (health != null)
        {
            health.OnHealthChanged += HandleDamageReceived;
        }
    }

    void OnDestroy()
    {
        health.OnHealthChanged -= HandleDamageReceived;
    }

    void HandleDamageReceived(float damageReceived)
    {
        PlayEffect(damageReceived);
    }

    void PlayEffect(float damage)
    {
        ParticleSystem chosenEffect = GetEffectForType(effectType);
        if (chosenEffect == null) return;
        ParticleSystem effectInstance = Instantiate(chosenEffect, transform.position, Quaternion.identity);
        var emission = effectInstance.emission;
        emission.rateOverTime = Mathf.Clamp(damage, 2f, 100f);
        Destroy(effectInstance.gameObject, effectInstance.main.duration);
    }

    private ParticleSystem GetEffectForType(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.Blood:return bloodParticles;
            case EffectType.AlienBlood:return alienBloodParticles;
            case EffectType.Wood:return woodParticles;
            case EffectType.Metal:return metalParticles;
            case EffectType.Fire:return fireParticles;
            default:return null;
        }
    }
   
}
