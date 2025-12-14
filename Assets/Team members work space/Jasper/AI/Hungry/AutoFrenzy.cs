using System.Collections;
using UnityEngine;

public class AutoFrenzy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitAndDamage());
    }
    
    private IEnumerator WaitAndDamage()
    {
        yield return new WaitForSeconds(5f);
        
        GetComponent<Health>().TakeDamage(9);
    }

}
