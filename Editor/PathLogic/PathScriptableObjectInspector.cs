using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(PathScriptableObject), true)]
public class PathScriptableObjectInspector : Editor
{
    private string text = "";
    private string pathToSaveVFXData;
    private string nameToSave = "test";
    private AlphabethScriptableObject alphabet;
    private bool instanciateVFXEffectInSceneImmediatly = true;

    private void Awake()
    {
        PathScriptableObject castTarget = (PathScriptableObject) target;
        nameToSave = castTarget.name;
        pathToSaveVFXData = DirConfiguration.Instance.GetVFXDataScriptableObjectDir();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        // cast target
        PathScriptableObject castTarget = (target as PathScriptableObject);
        if (castTarget == null)
            throw new Exception("target not found");
        
        // convert to vfx data
        EditorGUILayout.LabelField("Convert to VFX data object");
        pathToSaveVFXData = EditorGUILayout.TextField("path to save vfx data: ", pathToSaveVFXData);
        nameToSave = EditorGUILayout.TextField("name: ", nameToSave);
        text = EditorGUILayout.TextField("text: ", text);
        alphabet = EditorGUILayout.ObjectField("alphabeth: ", alphabet, typeof(AlphabethScriptableObject)) as AlphabethScriptableObject;
        instanciateVFXEffectInSceneImmediatly = EditorGUILayout.Toggle("instanciate VFXEffect in scene immediatly", instanciateVFXEffectInSceneImmediatly);
        if (GUILayout.Button("Convert to VFX data"))
        {
            
            // dummy scaling
            List<float> letterScaling = Enumerable.Repeat(1f ,TextUtil.ToSingleLine(text).Replace(" ", "").Length).ToList();
            VFXUtil.CreateEffectFromPath(castTarget.LoadPath().Copy(), text, alphabet, letterScaling);

            // // get textures
            // TextInsertionResult insertionResult = castTarget.LoadPath().Copy().ConvertToPointData(text, alphabet, letterScaling);
            //
            // // create pCache files, fill them with point data
            // string imagePathTemplate = pathToSaveVFXData + DirConfiguration.GetPCacheFileNamingTemplate();
            // string posFileName = String.Format(imagePathTemplate, nameToSave, "pos");
            // string rotFileName = String.Format(imagePathTemplate, nameToSave, "rot");
            // string letterFileName = String.Format(imagePathTemplate, nameToSave, "letter");
            // WritePointCache(insertionResult.positionsTexture, posFileName);
            // WritePointCache(insertionResult.rotationsTexture, rotFileName);
            // WritePointCache(insertionResult.lettersTexture, letterFileName);
            // AssetDatabase.Refresh();
            //
            // // import pCaches
            // AssetDatabase.ImportAsset(posFileName, ImportAssetOptions.ForceUpdate);
            // AssetDatabase.ImportAsset(rotFileName, ImportAssetOptions.ForceUpdate);
            // AssetDatabase.ImportAsset(letterFileName, ImportAssetOptions.ForceUpdate);
            //
            // AssetDatabase.Refresh();
            //
            // // read position part for each pCache
            // Texture2D positionTexture = ReadTextureFromPointCache(posFileName);
            // Texture2D rotationTexture = ReadTextureFromPointCache(rotFileName);
            // Texture2D letterTexture = ReadTextureFromPointCache(letterFileName);
            //
            // // create vfx data object, assign textures 
            // VFXDataScriptableObject vfxData = CreateInstance<VFXDataScriptableObject>();
            // vfxData.positionTexture = positionTexture;
            // vfxData.rotationTexture = rotationTexture;
            // vfxData.letterTexture = letterTexture;
            // // assign additional information
            // vfxData.textureDimension = insertionResult.textureDimension;
            // vfxData.letterScale = 1;
            // vfxData.numberOfElements = text.Replace(" ", "").Length;
            // // assign alphabet
            // vfxData.meshAlphabet = alphabet;
            //
            // // save vfx data as asset
            // AssetDatabase.CreateAsset(vfxData, pathToSaveVFXData + nameToSave + ".asset");
            // AssetDatabase.SaveAssets();
            //
            // // instanciate in scene
            // if (instanciateVFXEffectInSceneImmediatly)
            // {
            //     VFXDataScriptableObject loadedData = AssetDatabase.LoadAssetAtPath(pathToSaveVFXData + nameToSave + ".asset", typeof(VFXDataScriptableObject)) as VFXDataScriptableObject;
            //     VFXUtil.CreateEffectFromVFXData(loadedData);
            // }
        }
        
        GUILayout.Space(30);
        
        // button to update line renderer
        if (GUILayout.Button("Load To Scene"))
            castTarget.LoadToScene();
    }

    private void WritePointCache(List<Vector3> data, string fileName)
    {
        // write point cache file
        using (StreamWriter sw = File.CreateText(fileName))
        {
            // header
            sw.WriteLine("pcache");
            sw.WriteLine("format ascii 1.0");
            sw.WriteLine("comment Exported with PCache.cs");
            sw.WriteLine($"elements {data.Count}");
            sw.WriteLine("property float position.x");
            sw.WriteLine("property float position.y");
            sw.WriteLine("property float position.z");
            sw.WriteLine("end_header");
            // points
            foreach (Vector3 cachePoint in data)
            {
                Vector3 pos = cachePoint;
                sw.WriteLine($"{pos.x} {pos.y} {pos.z}");
            }
            sw.Close();
        }
    }

    private Texture2D ReadTextureFromPointCache(string filename)
    {
        Object[] allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(filename);
        Texture2D positionTexture = null;
        foreach (Object asset in allAssetsAtPath)
            if (asset.name == "position")
                positionTexture = (Texture2D)asset;

        if (positionTexture == null)
            throw new Exception("position was not found");

        return positionTexture;
    }
}
