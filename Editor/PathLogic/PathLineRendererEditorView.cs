using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(PathLineRenderer), true)]
public class PathLineRendererEditorView : Editor
{
    
    
    // section display bools
    SerializedProperty showBasics;
    SerializedProperty showPathProgress;
    SerializedProperty showInsertSubPath;
    
    // basics section
    SerializedProperty showRotationGizmos;
    SerializedProperty showCurrentPositionGizmo;
    SerializedProperty upAxisRoughDirection;
    
    // step to section
    SerializedProperty stepToJumpTo;
    SerializedProperty interPointProgress;
    
    // distance jumps section
    SerializedProperty moveDistance;
    SerializedProperty destroyWhileMoving;
    
    // save as so section
    SerializedProperty pathScriptableObjectName;
    SerializedProperty basePathToSaveTo;
    
    // insert sub path
    SerializedProperty subPath;

    private void OnEnable()
    {
        // section flags
        showBasics = serializedObject.FindProperty("showBasics");
        showPathProgress = serializedObject.FindProperty("showPathProgress");
        showInsertSubPath = serializedObject.FindProperty("showInsertSubPath");
        
        // basics section
        showRotationGizmos = serializedObject.FindProperty("showRotationGizmos");
        showCurrentPositionGizmo = serializedObject.FindProperty("showCurrentPositionGizmo");
        upAxisRoughDirection = serializedObject.FindProperty("upAxisRoughDirection");

        // step to section
        stepToJumpTo = serializedObject.FindProperty("stepToJumpTo");
        interPointProgress = serializedObject.FindProperty("interPointProgress");
        
        // distance jumps section
        moveDistance = serializedObject.FindProperty("moveDistance");
        destroyWhileMoving = serializedObject.FindProperty("destroyWhileMoving");
        
        // save as so section
        pathScriptableObjectName = serializedObject.FindProperty("pathScriptableObjectName");
        basePathToSaveTo = serializedObject.FindProperty("basePathToSaveTo");
        
        // insert sub path
        subPath = serializedObject.FindProperty("subPath");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // get path line renderer, and path
        PathLineRenderer pathLineRenderer = (target as PathLineRenderer);
        if (pathLineRenderer == null)
            return;
        Path path = pathLineRenderer.GetPso().LoadPath();
        
        // display basic setting
        showBasics.boolValue = EditorGUILayout.Foldout(showBasics.boolValue, "Basics");
        if (showBasics.boolValue)
        {
            // display gizmo settings
            showRotationGizmos.boolValue = EditorGUILayout.Toggle("shot rotation gizmos", showRotationGizmos.boolValue);
            showCurrentPositionGizmo.boolValue = EditorGUILayout.Toggle("show current position gizmo", showCurrentPositionGizmo.boolValue);
            upAxisRoughDirection.vector3Value = EditorGUILayout.Vector3Field("rough up axis", upAxisRoughDirection.vector3Value);
            GUILayout.Space(10);

            // display main axis, up axis and current position
            EditorGUILayout.LabelField("Orientation:");
            if (path != null)
                EditorGUILayout.LabelField($"Main-Axis: {path.GetMainAxis()}  /  Up-Axis: {path.GetUp().normalized}");
            GUILayout.Space(20);
        }

        // display path progress
        showPathProgress.boolValue = EditorGUILayout.Foldout(showPathProgress.boolValue, "Path Progression");
        if (showPathProgress.boolValue)
        {
            if (path != null)
            {
                EditorGUILayout.LabelField(
                    $"Index: {path.GetPathPosition().PointIndex}  /  InterPointProgress: {path.GetPathPosition().InterPointProgress}");

                EditorGUILayout.Space(10);
                
                EditorGUILayout.LabelField("Precise Jumps:");
                
                EditorGUILayout.LabelField("New Index:");
                EditorGUILayout.BeginHorizontal();
                
                stepToJumpTo.intValue = EditorGUILayout.IntField(stepToJumpTo.intValue);
                stepToJumpTo.intValue = Mathf.Clamp(stepToJumpTo.intValue, 0, path.GetPoints().Length);
                stepToJumpTo.intValue = Mathf.RoundToInt(GUILayout.HorizontalSlider(stepToJumpTo.intValue, 0, path.GetPoints().Length - 1));
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("New Inter-Point-Progress:");
                EditorGUILayout.BeginHorizontal();
                
                interPointProgress.floatValue = EditorGUILayout.FloatField(interPointProgress.floatValue);
                interPointProgress.floatValue = Mathf.Clamp(interPointProgress.floatValue, 0, 1);
                if (stepToJumpTo.intValue == path.GetPoints().Length)
                    interPointProgress.floatValue = 0f;
                interPointProgress.floatValue = GUILayout.HorizontalSlider(interPointProgress.floatValue, 0f, 1f);
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("Apply"))
                {
                    path.SetPathPosition(new PathPosition(stepToJumpTo.intValue, interPointProgress.floatValue));
                    pathLineRenderer.SyncLineRenderer();
                }
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Move On Path, by Distance:");
                EditorGUILayout.BeginHorizontal();
                moveDistance.floatValue = EditorGUILayout.FloatField(moveDistance.floatValue);
                moveDistance.floatValue = GUILayout.HorizontalSlider(moveDistance.floatValue, 0f, path.GetLength());
                moveDistance.floatValue = Mathf.Clamp(moveDistance.floatValue, 0f, path.GetLength());

                EditorGUILayout.LabelField("Destroy While Moving:");
                destroyWhileMoving.boolValue = EditorGUILayout.Toggle(destroyWhileMoving.boolValue);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Move On Path"))
                {
                    path.MoveDistanceOnPath(moveDistance.floatValue);
                    pathLineRenderer.SyncLineRenderer();

                }
                if (GUILayout.Button("Move In Space"))
                {
                    path.MoveDistanceInSpace(moveDistance.floatValue, destroyWhileMoving.boolValue);
                    pathLineRenderer.SyncLineRenderer();

                }
                EditorGUILayout.EndHorizontal();
            }
        }

        // display sub path insertion section
        showInsertSubPath.boolValue = EditorGUILayout.Foldout(showInsertSubPath.boolValue, "Insert Sub Path");
        if (showInsertSubPath.boolValue && path != null)
        {
            subPath.objectReferenceValue = EditorGUILayout.ObjectField(subPath.objectReferenceValue, typeof(PathScriptableObject)) as PathScriptableObject;
            if (subPath.objectReferenceValue != null)
                if (GUILayout.Button("Insert"))
                {
                    path.InsertSubPath(((PathScriptableObject) subPath.objectReferenceValue).LoadPath());
                    pathLineRenderer.SyncLineRenderer();
                }
        }
        
        // display persistence section
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Persist:");
        basePathToSaveTo.stringValue = EditorGUILayout.TextField("Save at Base Path: ", basePathToSaveTo.stringValue);
        pathScriptableObjectName.stringValue = EditorGUILayout.TextField("Save as: ", pathScriptableObjectName.stringValue);
        if (GUILayout.Button("Save as Scriptable Object"))
        {
            pathLineRenderer.SyncLineRenderer();
            pathLineRenderer.SaveAsScriptableObject(basePathToSaveTo.stringValue + pathScriptableObjectName.stringValue + ".asset");
        }
        
        if (GUILayout.Button("Reload from Scriptable Object"))
        {
            pathLineRenderer.GetPso().ReloadPath();
            pathLineRenderer.SyncLineRenderer();
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    public void OnInspectorUpdate()
    {
        this.Repaint();
    }
}