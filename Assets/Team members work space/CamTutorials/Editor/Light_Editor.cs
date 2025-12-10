using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Light_Model), true)]
public class Light_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("Random colour"))
		{
			Light_Model light = target as Light_Model;
			light.ChangeColour_Rpc();
		}
	}
}
