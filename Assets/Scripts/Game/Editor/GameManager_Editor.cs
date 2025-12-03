using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DanniLi.GameManager), true)]
public class GameManager_Editor : Editor
{
	override public void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		DanniLi.GameManager gameManager = target as DanniLi.GameManager;

		if (GUILayout.Button("Win"))
		{
			gameManager.DoWin();
		}

		if (GUILayout.Button("Lose"))
		{
			gameManager.DoLose();
		}
		
		if (GUILayout.Button("Test Load Scene"))
		{
			gameManager.TestLoadScene();
		}
		if (GUILayout.Button("Test Unload Scene"))
		{
			gameManager.TestUnloadScene();
		}
	}
}
