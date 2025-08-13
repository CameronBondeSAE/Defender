using System.Collections;
using System.Collections.Generic;
using Defender;
using UnityEngine;

public class Crate : MonoBehaviour, IUsable
{
	public float        timeToNewItemSpawn = 3f;
	public List<ItemSO> availableItems;
	public ItemSO       currentItem;

	public GameObject itemFloating;
	
	public FloatingUI floatingUI;
	public Transform floatingMount;
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void ChooseRandomItem()
    {
	    StartCoroutine(ChooseRandomItem_Sequence());
    }
    
    IEnumerator ChooseRandomItem_Sequence()
    {
	    if (itemFloating != null)
	    {
		    Destroy(itemFloating);
		    itemFloating = null;
	    }

	    yield return new WaitForSeconds(timeToNewItemSpawn);
	    
	    if (availableItems != null && availableItems.Count > 0)
	    {
		    ItemSO randomItem = availableItems[Random.Range(0, availableItems.Count)];
	
		    if (randomItem != null)
		    {
			    if (randomItem.ItemCrateUIPrefab != null)
				    itemFloating = Instantiate(randomItem.ItemCrateUIPrefab, transform.position, Quaternion.identity);
			    if (floatingMount != null)
				    itemFloating.transform.parent = floatingMount.transform; // So the wibble effect works
			    
			    floatingUI.Disable();
		    }
        
	    }
	    else
	    {
		    Debug.LogWarning("No items available to pick up!");
	    }
	
    }

    public void Use(CharacterBase characterTryingToUse)
    {
	    
	    
	    // StartCoroutine(ChooseRandomItem_Sequence())
    }

    public void StopUsing()
    {
	    
    }
}
