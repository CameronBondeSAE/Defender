using UnityEngine;

public class SpecialEffectsManager : MonoBehaviour
{
    public enum EffectType
    {
        Blood,
        AlienBlood,
        Wood,
        Metal,
        Fire
    }

    [Header("Effect Types")] public EffectType effectType;

    [Header("Particle Systems")] public ParticleSystem bloodParticles;

    public ParticleSystem alienBloodParticles;
    public ParticleSystem woodParticles;
    public ParticleSystem metalParticles;
    public ParticleSystem fireParticles;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null) health.OnHealthChanged += HandleDamageReceived;
    }

    private void OnDestroy()
    {
        health.OnHealthChanged -= HandleDamageReceived;
    }

    private void HandleDamageReceived(float damageReceived)
    {
        PlayEffect(damageReceived);
    }

    private void PlayEffect(float damage)
    {
        var chosenEffect = GetEffectForType(effectType);
        if (chosenEffect == null) return;
        var effectInstance = Instantiate(chosenEffect, transform.position, Quaternion.identity);
        var emission = effectInstance.emission;
        emission.rateOverTime = Mathf.Clamp(damage, 2f, 100f);
        Destroy(effectInstance.gameObject, effectInstance.main.duration);
    }

    private ParticleSystem GetEffectForType(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.Blood: return bloodParticles;
            case EffectType.AlienBlood: return alienBloodParticles;
            case EffectType.Wood: return woodParticles;
            case EffectType.Metal: return metalParticles;
            case EffectType.Fire: return fireParticles;
            default: return null;
        }
    }
}