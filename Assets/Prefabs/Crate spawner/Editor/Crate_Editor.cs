using UnityEditor;
using UnityEngine;

namespace Prefabs.Crate_spawner.Editor
{
	[CustomEditor(typeof(Crate), true)]
	public class Crate_Editor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Spawn item"))
			{
				Crate crate = target as Crate;
				crate.ChooseRandomItem();
			}
		}
	}
}