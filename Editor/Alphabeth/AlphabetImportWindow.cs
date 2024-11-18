using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AlphabetImportWindow : EditorWindow
{
    // register window
    [MenuItem("Window/AlphabetImportWindow")]
    public static void ShowWindow()
    {
        GetWindow<AlphabetImportWindow>("AlphabetImportWindow");
    }
    
    // input
    public string savePath = "Assets/Alphabets/AlphabetScriptableObjects/";
    public GameObject[] meshAlphabetAssets;

    private void HandleConversion(GameObject meshAlphabet)
    {
        // check if mesh asset input length is correct
        MeshFilter[] mfs = meshAlphabet.GetComponentsInChildren<MeshFilter>();
        if (mfs.Length != TextUtil.alphabeth.Length)
            throw new ArgumentException("number of meshes was not equal to order string length of alphabet");
            
        // iterate asset meshes, insert at correct index
        Mesh[] outMeshes = new Mesh[mfs.Length];
        for (int i = 0; i < mfs.Length; i++)
        {
            Mesh mf = mfs[i].sharedMesh;
                
            string searchLetter = mf.name.Replace("obj_", "");
            if (searchLetter == "slash")
                searchLetter = "\\";
            if (searchLetter == "period")
                searchLetter = ".";
            char searchCharacter = searchLetter[0];
                
            int meshPosition = TextUtil.alphabeth.IndexOf(searchCharacter);
                
            outMeshes[meshPosition] = mf;
        }

        // create new scriptable object
        AlphabethScriptableObject aso = CreateInstance<AlphabethScriptableObject>();
        aso.name = meshAlphabet.name;
        aso.meshes = outMeshes;
        aso.ConfigureWidthAutomatically();
        
        AssetDatabase.CreateAsset(aso, savePath + aso.name + ".asset");
    }
    
    private void OnGUI()
    {
        // display mesh alphabet assets 
        SerializedObject so = new SerializedObject(this);
        SerializedProperty meshProperty = so.FindProperty("meshAlphabetAssets");
        EditorGUILayout.PropertyField(meshProperty, true);
        so.ApplyModifiedProperties(); // apply changes
        
        // display save path
        savePath = EditorGUILayout.TextField("dir to save SO in:", savePath);
        
        GUILayout.Label("Import Alphabets as Scriptable Objects");
        if (GUILayout.Button("Convert!"))
        {
            foreach (GameObject meshAlphabetAsset in meshAlphabetAssets)
            {
                HandleConversion(meshAlphabetAsset);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
