#if (UNITY_EDITOR)

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


[CustomEditor(typeof(GenericSection), true)]
public class GenericSectionInspector : Editor
{
    private AllowedGenericTypes _lastGenericType;

    // private SerializedProperty objects;
    private ReorderableList list;
    
    void OnEnable()
    {

        
        if (!((GenericSection) target).JustBaseInspector() && ((GenericSection) target).MustForceIterable())
        {
            list = new ReorderableList(serializedObject, 
                serializedObject.FindProperty("values"), 
                true, true, true, true);
        
            list.drawElementCallback = 
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative(GetFieldName()), GUIContent.none);
                };
        }
    }


    private void GUIOfList()
    {
        list.DoLayoutList();
    }

    private void GUIOfValue()
    {
        GenericSection castTarget = (GenericSection) target;

        SerializedProperty value = serializedObject.FindProperty("value");

        EditorGUI.PropertyField(
            new Rect(0, 100, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight),
            value.FindPropertyRelative(GetFieldName()), GUIContent.none);
        
    }

    private void GUIShared()
    {
        GenericSection castTarget = (GenericSection) target;

        if (castTarget.type != AllowedGenericTypes.GENERIC_SECTION)
            base.OnInspectorGUI();
    }
    
    
    public override void OnInspectorGUI()
    {
        if (((GenericSection) target).JustBaseInspector())
        {
            base.OnInspectorGUI();
            return;
        }
        
        serializedObject.Update();

        GUIShared();
        GUILayout.Space(100);
        
        if (((GenericSection) target).MustForceIterable())
            GUIOfList();
        else
            GUIOfValue();
        
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
    
    public string GetFieldName()
    {
        switch (((GenericSection) target).type)
        {
            case AllowedGenericTypes.BOOL:
                return "boolValue";
            case AllowedGenericTypes.FLOAT:
                return "floatValue";
            case AllowedGenericTypes.STRING:
                return "stringValue";
            case AllowedGenericTypes.INTEGER:
                return "integerValue";
            case AllowedGenericTypes.CHAR:
                return "charValue";
            case AllowedGenericTypes.MESH_PATH_STRATEGY:
                return "meshPathStrategyValue";
            case AllowedGenericTypes.GENERIC_SECTION:
                return "genericSectionValue";
            default:
                return "floatValue";
        }
    }
}
#endif