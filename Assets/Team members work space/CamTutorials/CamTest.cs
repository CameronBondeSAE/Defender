using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Scenes
{
	public class CamTest : MonoBehaviour
	{
		public List<Object> scenes;
		public Object currentScene;

		private void Awake()
		{
			foreach (Object scene in scenes)
			{
				Debug.Log(scene);
			}
		}

		private void Update()
		{
			if (InputSystem.GetDevice<Keyboard>().spaceKey.wasPressedThisFrame)
			{
				Scene currentScene = SceneManager.GetActiveScene();
				SceneManager.LoadScene(currentScene.name);
			}
		}
	}
}