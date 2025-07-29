using System;
using System.Collections;
using UnityEngine;

public class BlueBeam : MonoBehaviour
{
    [SerializeField] private float alienKillDelay;
    [SerializeField] private Transform shipPosition;

    [SerializeField] private float abductionSpeed;
    public delegate void CivilianKilled(GameObject civilian);
    public event CivilianKilled OnCivilianKilled;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Civilian")
        {
            StartCoroutine(AbductCivilian(other.gameObject));
        }

        if (other.transform.tag == "Alien") 
        {
            StartCoroutine(WaitToKillAlien(other.gameObject));
        }

    }

    private IEnumerator WaitToKillAlien(GameObject alien)
    {
        yield return new WaitForSeconds(alienKillDelay);
        Destroy(alien);
    }

    private IEnumerator AbductCivilian(GameObject civilian)
    {
        if (civilian == null) yield break;

        Rigidbody rb = civilian.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        while (civilian != null && (shipPosition.position - civilian.transform.position).sqrMagnitude > 0.01f)
        {
            // Make sure the civilian wasn't destroyed mid-abduction
            if (civilian == null) yield break;

            Vector3 direction = (shipPosition.position - civilian.transform.position).normalized;
            civilian.transform.position += direction * abductionSpeed * Time.deltaTime;
            yield return null;
        }
        OnCivilianKilled?.Invoke(civilian.gameObject);
        Destroy(civilian.gameObject);
        
    }
    
    public void TurnOnCollider()
    {
        transform.GetComponent<Collider>().enabled = true;
    }
}
