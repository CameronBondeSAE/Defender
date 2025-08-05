using UnityEditor;
using UnityEngine;

namespace mothershipScripts.Health.Editor
{
	[CustomEditor(typeof(global::Health), true)]
	public class Health_Editor : UnityEditor.Editor
	{
		override public void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Take Damage"))
			{
				global::Health health = target as global::Health;
				health.TakeDamage(10);
			}
			if (GUILayout.Button("Kill"))
			{
				global::Health health = target as global::Health;
				health.TakeDamage(1000000);
			}
		}
	}
}