using UnityEngine;

public class Landmine : MonoBehaviour
{
    public bool BigSteps = false; // bool activates a 2-second timer for the landmine to explode, activates  
    
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Landmine");
        if (other.gameObject.GetComponent<Health>())
        {
            Debug.Log("Kaboom"); // in polishing, we could add a UI which prints this // 
            other.gameObject.GetComponent<Health>().TakeDamage(100);
            Destroy(gameObject);
        }
    }
}

/// Conciderations
/// Bool can be removed to make mine explosions instant - this may work better in the overall polishing process
/// otherwise what we can do is increase the timer to 3 seconds and keep the blast radius the same 