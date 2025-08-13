using Defender;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// on pickup, the vest is not activated.
/// oncollision enter, attach to the other character (not the owner)
/// </summary>
public class SuicideVest : UsableItem_Base
{
    [Space]
    [Header("Vest Settings")]
    [SerializeField] private float damageAmount;
    [SerializeField] private float explosionRadius;
    [SerializeField] private Transform vestTransform;
    [SerializeField] private CharacterBase owner;
    [SerializeField] private DrawSphereOfInfluence explosionRadiusVisual;
    private enum VestState { disabled, inHand, isAttached };
    [SerializeField] private VestState state;
    [SerializeField] private float attachActivationDelay = 5f;

    [SerializeField] private GameObject sparkParticles;

    public CharacterBase entityAttachedTo;
    
    public Collider vestTrigger;

    private void LateUpdate()
    {
        if (state == VestState.isAttached)
        {
	        if (entityAttachedTo != null)
	        {
		        transform.rotation = entityAttachedTo.transform.rotation;

		        transform.position = entityAttachedTo.transform.position + transform.forward * -0.8f + transform.up * 1.5f;
            }
        }
    }

    public override void StopUsing()
    {
        base.StopUsing();

        state = VestState.disabled;

        Debug.Log("Vest disabled");
    }

    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp);

        state = VestState.inHand;
        
        owner = whoIsPickupMeUp;
        vestTrigger.enabled = true;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (state == VestState.inHand)
        {
            if (collision.gameObject.GetComponent<CharacterBase>())
            {
                if (collision.gameObject != owner.gameObject)
                {
	                owner.GetComponent<PlayerInventory>().DropHeldItem(); // HACK: Need a better way to inform inventory of destroy/unattaching
	                
                    entityAttachedTo = collision.gameObject.GetComponent<CharacterBase>();

                    state = VestState.isAttached;
                    StartCoroutine(Explode());
                }
            }
        }
    }

    private IEnumerator Explode()
    {
        // once attached, explode after a set timer

        if (state == VestState.isAttached)
        {
            sparkParticles.SetActive(true);
            explosionRadiusVisual.enabled = true;

            activationCountdown = 5f;
            StartActivationCountdown_Server(attachActivationDelay);
            SetCollidersEnabled(false);
            
            yield return new WaitForSeconds(activationCountdown);

            Collider[] collidersInRange = new Collider[10];
            Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, collidersInRange);

            foreach (Collider collider in collidersInRange)
            {
                if (collider != null)
                {
                    if (collider.GetComponent<CharacterBase>())
                    {
                        collider.gameObject.GetComponent<Health>().TakeDamage(damageAmount);
                    }
                }
                else
                {
                    continue;
                }
            }

            Debug.Log("Exploded");

            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
