#if (UNITY_EDITOR)


using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(InspectableTextArrangement), true)]
public class TextArrangementInspector : Editor
{
    private string filename;
    
    public void OnValidate()
    {
        InspectableTextArrangement castTarget = (InspectableTextArrangement) target;
        castTarget.MakeValid();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create Vfx Effect"))
        {
            InspectableTextArrangement castTarget = (InspectableTextArrangement) target;
            castTarget.CreateVfxEffect();            
        }

        GUILayout.Label("File Name:");
        filename = GUILayout.TextField(filename);
        if (GUILayout.Button("Save Vfx Effect Data"))
        {
            InspectableTextArrangement castTarget = (InspectableTextArrangement) target;
            castTarget.SaveVfxData(filename);            
        }
    }
}
#endif