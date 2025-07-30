using UnityEditor;
using UnityEngine;
using NicholasScripts; // Make sure this matches your namespace

[CustomEditor(typeof(Generator), true)]
public class GeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Generator generator = target as Generator;

        if (generator == null)
            return;

        if (GUILayout.Button("Activate Generator"))
        {
            generator.UseItem();
        }

        if (GUILayout.Button("Destroy Generator"))
        {
            // Mark the object as dirty in case it's part of a prefab stage
            if (!Application.isPlaying)
                Debug.LogWarning("Destroy will only work in Play Mode.");

            Destroy(generator.gameObject);
        }
    }
}