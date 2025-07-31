using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DecoyItem), true)]
public class SolDecoyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Interact"))
        {
            // ‘target’ is the magic variable that editors use to link back to the original component. It’s in the BASE CLASS, so you have to ‘cast’ to get access to YOUR functions.
            DecoyItem decoyItem; 
            decoyItem = target as DecoyItem;
            decoyItem.Use();
        }
        
        if (GUILayout.Button("Stop Interacting"))
        {
            // ‘target’ is the magic variable that editors use to link back to the original component. It’s in the BASE CLASS, so you have to ‘cast’ to get access to YOUR functions.
            DecoyItem decoyItem; 
            decoyItem = target as DecoyItem;
            decoyItem?.StopUsing();
        }
    }
}

