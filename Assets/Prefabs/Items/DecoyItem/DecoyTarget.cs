using System.Collections;
using UnityEngine;

public class DecoyTarget : AIBase
{
    public DecoyItem decoyItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        decoyItem = GetComponent<DecoyItem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAbducted == true)
        {
            Destroy();
        }
    }
    
    public void Destroy()
    {
        if (decoyItem != null)
        {
            decoyItem.blnDie = true;
        }
        transform.gameObject.SetActive(false);

        StartCoroutine(Dying());
    }

    IEnumerator Dying()
    {
        yield return new WaitForSeconds(2f);
        GameObject.Destroy(gameObject);
    }
}
