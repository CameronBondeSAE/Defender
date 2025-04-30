using System;
using System.Collections;
using UnityEngine;

public class BlueBeam : MonoBehaviour
{
    [SerializeField] private float alienKillDelay;

    public delegate void CivilianKilled(GameObject civilian);
    public event CivilianKilled OnCivilianKilled;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Civilian")
        {
            Debug.Log("civ killed");
            OnCivilianKilled?.Invoke(other.gameObject);
            Destroy(other.gameObject);   
        }

        if (other.transform.tag == "Alien") ;
        {
            StartCoroutine(WaitToKillAlien(other.gameObject));
        }

    }

    private IEnumerator WaitToKillAlien(GameObject alien)
    {
        yield return new WaitForSeconds(alienKillDelay);
        Destroy(alien);
    }

    public void TurnOnCollider()
    {
        transform.GetComponent<Collider>().enabled = true;
    }
}
