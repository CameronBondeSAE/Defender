using UnityEngine;

public class SeekingCritter : MonoBehaviour
{
    [Header("Critter Stats")] 
    private float speed = 100f;
    private float damage = 50f;
    
    //is alien return state
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UseItem()
    {
        ActivateCritter();
    }

    private void ActivateCritter()
    {
        
    }

    public void Die()
    {
        Destroy(gameObject);
    }
    
}



