using Defender;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

/// <summary>
/// Enlarges an object along with its circle collider to create a slippery floor. Affects players and AI's
/// </summary>
public class SlipperyFloor : NetworkBehaviour
{
    // TODO: need to network the visual for enlarging liquid
    // TODO: if a character is affected by the liquid but its collider is outside of the liquids range and the liquid despawns, the character does not get reset becuase they were removed from the characterBases list upon exiting the collider range
    [Header("Item Settings")]
    [Tooltip("A reference to the particle gameobject -- allows for enlarging it")]
    [SerializeField] private GameObject particles;
    [Tooltip("The size the liquid area will grow to")]
    public Vector3 desiredSize;
    [Tooltip("The speed the liquid grows to the desired size")]
    [SerializeField] private float enlargeSpeed;
    [Tooltip("The time limit for the liquid to despawn")]
    [SerializeField] private float despawnTimer;

    [Space]
    [Header("Effect Settings")]
    [Tooltip("How long the character will be disabled")]
    [SerializeField] private float characterFallDuration;
    [Tooltip("How quick the character falls")]
    [SerializeField] private float fallingSpeed;
    [Tooltip("The velocity the characters rigidbody will be set to")]
    [SerializeField] private float slowDownSpeed;

    [Space]
    [Tooltip("Characters currently being affected by the liquid")]
    [SerializeField] private List<CharacterBase> characterBases;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (CharacterBase characterBase in characterBases)
        {
            Debug.Log("Reset " + characterBase.gameObject.name + "'s changed variables");
            ResetChangedVariablesOnDespawn(characterBase.GetComponent<Collider>());
        }
    }

    private void Update()
    {
        if (!IsServer) { return; }

        StartCoroutine(DespawnNetworkObjectDelay_Coroutine());
        EnlargePuddleSize();
    }

    private void EnlargePuddleSize()
    {
        //Debug.Log(particles.transform.localScale.x);

        if (particles.transform.localScale.x < desiredSize.x || particles.transform.localScale.y < desiredSize.y)
        {
            //Debug.Log("enlarging");
            particles.transform.localScale += new Vector3(1, 1, 0) * enlargeSpeed * Time.deltaTime;
            gameObject.GetComponent<SphereCollider>().radius = desiredSize.x;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterBase>() != null)
        {
            characterBases.Add(other.GetComponent<CharacterBase>());
            StartCoroutine(SlipAndFall_Coroutine(other.gameObject.GetComponent<Collider>()));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterBase>() != null)
        {
            characterBases.Remove(other.GetComponent<CharacterBase>());
        }
    }

    private IEnumerator SlipAndFall_Coroutine(Collider other)
    {
        // unfreezes the rotation of rigidbodies, allowing them to fall over
        // lowers the max linear velocity, so that players/ai's cant just sprint over the liquid
        Debug.Log("Falling over");
        other.GetComponent<Rigidbody>().freezeRotation = false;
        other.GetComponent<Rigidbody>().maxLinearVelocity = slowDownSpeed;
        other.transform.Rotate(Vector3.left * fallingSpeed * Time.deltaTime);
        if (other.GetComponent<PlayerMovement>() != null)
        {
            other.GetComponent<PlayerMovement>().enabled = false; // disables player movement for a small period of time
        }

        yield return new WaitForSeconds(characterFallDuration);

        // re-enables/resets the changed variables
        other.GetComponent<Rigidbody>().maxLinearVelocity = 100f;
        if (other.GetComponent<PlayerMovement>() != null)
        {
            other.GetComponent<PlayerMovement>().enabled = true;
            other.GetComponent<Rigidbody>().freezeRotation = true;
        }
        else if (other.GetComponent<AIHealth>() != null)
        {
            // puts any AI in the upright position with correct constraints
            other.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            other.transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        // remove character from list when finished falling
        characterBases.Remove(other.GetComponent<CharacterBase>());
    }

    private void ResetChangedVariablesOnDespawn(Collider other)
    {
        other.GetComponent<Rigidbody>().maxLinearVelocity = 100f;
        if (other.GetComponent<PlayerMovement>() != null)
        {
            other.GetComponent<PlayerMovement>().enabled = true;
            other.GetComponent<Rigidbody>().freezeRotation = true;
        }
        else if (other.GetComponent<AIHealth>() != null)
        {
            // puts any AI in the upright position with correct constraints
            other.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            other.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
    }

    private IEnumerator DespawnNetworkObjectDelay_Coroutine()
    {
        yield return new WaitForSeconds(despawnTimer);

        gameObject.GetComponent<NetworkObject>().Despawn();
    }
}
