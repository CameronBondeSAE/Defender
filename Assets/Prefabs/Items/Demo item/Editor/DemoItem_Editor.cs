using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DemoItemBase), true)]
public class DemoItem_Editor : Editor
{
	override public void OnInspectorGUI()
	{
		if (GUILayout.Button("Use"))
		{
			DemoItemBase item = target as DemoItemBase;
			item?.Use(null);
		}
		
		base.OnInspectorGUI();
	}
}