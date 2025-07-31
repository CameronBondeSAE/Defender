using Tutorials;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CamPlayer_Model), true)]
public class CamPlayer_Model_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		GUILayout.Label("CamPlayer_Model_Editor");
		if (GUILayout.Button("Jump"))
		{
			CamPlayer_Model playerModel = target as CamPlayer_Model;
			if (playerModel != null)
			{
				playerModel.Jump();
			}
		}		if (GUILayout.Button("Jump"))
		{
			CamPlayer_Model playerModel = target as CamPlayer_Model;
			if (playerModel != null)
			{
				playerModel.Jump();
			}
		}		if (GUILayout.Button("Jump"))
		{
			CamPlayer_Model playerModel = target as CamPlayer_Model;
			if (playerModel != null)
			{
				playerModel.Jump();
			}
		}

		if (GUILayout.Button("Jump"))
		{
			CamPlayer_Model playerModel = target as CamPlayer_Model;
			if (playerModel != null)
			{
				playerModel.Jump();
			}
		}
	}
	
}
