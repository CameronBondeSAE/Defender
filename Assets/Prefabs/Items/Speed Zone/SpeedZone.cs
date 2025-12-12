using UnityEngine;
using Defender;

public class SpeedZone : MonoBehaviour
{
    [Header("Speed Zone Seettings")] 
    
    public float boostMultiplier = 2f;
    public float boostDuration = 15f;

    private CharacterBase character;
    private float defaultSpeed;
    private float boostEndTime;
    private bool isBoosting = false;

    private void OnTriggerEnter(Collider other)
    {
        character = other.GetComponentInParent < CharacterBase>();

        if (character != null)
        {
            StartBoost();
        }
    }

    private void Update()
    {
        if (isBoosting && Time.time >= boostEndTime)
        {
            ResetSpeed();
        }
    }

    private void StartBoost()
    {
        if (character == null) return;

        defaultSpeed = character.DefaultSpeed;

        character.MoveSpeed = defaultSpeed * boostMultiplier;

        boostEndTime = Time.time + boostDuration;

        isBoosting = true;
    }

    private void ResetSpeed()
    {
        if (character == null) return;

        character.MoveSpeed = defaultSpeed;
        isBoosting = false;
    }

}
