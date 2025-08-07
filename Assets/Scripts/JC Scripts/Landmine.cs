using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Landmine : MonoBehaviour
{
	public bool isArmed = false;
	public float armTime = 3f;

	// Update is called once per frame
	void Update()
	{
	}

	private void Start()
	{
		StartCoroutine(Activated()); 
	}

	public IEnumerator Activated()
	{
		yield return new WaitForSeconds(armTime);
		isArmed = true;
	}



	private void OnCollisionEnter(Collision other) // Mine detonation 
	{
		Debug.Log("Landmine");
		if (!isArmed && other.gameObject.GetComponent<Health>())
		{
			Activated();
			if (other.gameObject.GetComponent<Health>())
			{
				Debug.Log("Kaboom"); // in polishing, we could add a UI which prints this // 
				other.gameObject.GetComponent<Health>().TakeDamage(100);
				Destroy(gameObject);
				//Destroy(other.gameObject);
			}
		}
	}
}

/// Conciderations
/// Bool can be removed to make mine explosions instant - this may work better in the overall polishing process
/// otherwise what we can do is increase the timer to 3 seconds and keep the blast radius the same 