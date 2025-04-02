using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
	public SoundPack_SO soundPack;

	private void Start()
	{
		PlayRandomSound();
	}

	public void PlayRandomSound()
	{
		Debug.Log("ScriptableObject contains : "+soundPack.clips.Count);
	}
}
