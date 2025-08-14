using UnityEngine;
using UnityEditor;
using Unity.Netcode;

[CustomEditor(typeof(LevelLoader), true)]
public class LevelLoader_Editor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();
    var loader = (LevelLoader)target;
    EditorGUILayout.Space(8);
    EditorGUILayout.LabelField("Test Loading Levels", EditorStyles.boldLabel);
    if (!Application.isPlaying)
    {
      return;
    }

    using (new EditorGUI.DisabledScope(NetworkManager.Singleton == null))
    {
      if (GUILayout.Button("Load Next Levels"))
        loader.NextLevelServerRpc();
      if (GUILayout.Button("Reload Current Levels"))
        loader.ReloadCurrentServerRpc();
    }
  }
}
