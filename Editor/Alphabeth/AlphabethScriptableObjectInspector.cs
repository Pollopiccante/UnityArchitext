using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(AlphabethScriptableObject), true)]
public class AlphabethScriptableObjectInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Configure Widths Automatically"))
        {
            AlphabethScriptableObject castTarget = (target as AlphabethScriptableObject);
            if (castTarget == null)
                return;
            castTarget.ConfigureWidthAutomatically();
            SaveChanges();
        }
    }
}
