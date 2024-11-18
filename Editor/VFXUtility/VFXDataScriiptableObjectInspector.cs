using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VFXDataScriptableObject), true)]
public class VFXDataScriiptableObjectInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        // button to create vfx effect
        if (GUILayout.Button("Create VFX Effect"))
        {
            VFXDataScriptableObject castTarget = (VFXDataScriptableObject) target;
            VFXUtil.CreateEffectFromVFXData(castTarget);
        }
    }
}
