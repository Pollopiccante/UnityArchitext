using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

public class VFXUtil
{
    public static GameObject CreateEffectFromVFXData(VFXDataScriptableObject vfxData)
    {
        // create vfx instance
        GameObject vfxObject = GameObject.Instantiate(Resources.Load("Prefabs/StaticWordMeshEffect") as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
        VisualEffect vfxComponent = vfxObject.GetComponent<VisualEffect>();

        // assign data to vfx effect
        // basics
        vfxComponent.SetInt("NumberOfElement", vfxData.numberOfElements);
        vfxComponent.SetFloat("LetterScale", vfxData.letterScale);
        // textures
        vfxComponent.SetInt("Dimension", vfxData.textureDimension);
        vfxComponent.SetTexture("Positions", vfxData.positionTexture);
        vfxComponent.SetTexture("LinePositions", vfxData.linePositionTexture);
        vfxComponent.SetTexture("Rotations", vfxData.rotationTexture);
        vfxComponent.SetTexture("Letters", vfxData.letterTexture);
        vfxComponent.SetTexture("Scalings", vfxData.scaleTexture);
        vfxComponent.SetTexture("Colors", vfxData.colorTexture);
        vfxComponent.SetTexture("XWaveMotions", vfxData.xWaveMotionTexture);
        vfxComponent.SetTexture("Alpha_Smoothness_Metalic", vfxData.alphaSmoothnessMetalicTexture);
        vfxComponent.SetTexture("StartEndIndices", vfxData.startEndIndexTexture);
        
        // mesh alphabet
        for (int i = 0; i < TextUtil.alphabeth.Length; i++)
            vfxComponent.SetMesh($"Mesh_ {i}", vfxData.meshAlphabet.meshes[i]);
        
        // add index stepper, and start it
        IndexStepper indexStepper = vfxObject.AddComponent<IndexStepper>();
        indexStepper.pause = false;
        // add reader stepper, and start it
        ReaderStepper readerStepper = vfxObject.AddComponent<ReaderStepper>();
        readerStepper.pause = false;

        return vfxObject;
    }
    
    public static VFXDataScriptableObject CreateVFXDataFromPath(Path path, string text, AlphabethScriptableObject alphabet, List<float> letterScaling, bool flyIn=false)
    {
        // get textures as pCaches
        TextInsertionResult insertionResult = path.Copy().ConvertToPointData(text, alphabet, letterScaling);

        // convert pCaches to textures
        Texture2D positionTexture = PointCacheToTexture2D(insertionResult.positionsTexture);
        Texture2D linePositionTexture = PointCacheToTexture2D(insertionResult.linePositionTexture);
        Texture2D rotationTexture = PointCacheToTexture2D(insertionResult.rotationsTexture);
        Texture2D letterTexture = PointCacheToTexture2D(insertionResult.lettersTexture);
        Texture2D scalesTexture = PointCacheToTexture2D(insertionResult.scalesTexture);
        

        List<Vector3> colorsDummy = Enumerable.Repeat(new Vector3(), insertionResult.scalesTexture.Count).ToList();
        List<Vector3> xWaveDummy = Enumerable.Repeat(new Vector3(), insertionResult.scalesTexture.Count).ToList();
        List<Vector3> ASMDummy = Enumerable.Repeat(new Vector3(1f, 1f, 1f), insertionResult.scalesTexture.Count).ToList();
        
        List<Vector3> startEndIndicesDummy = new List<Vector3>();
        for (int i = 0; i < insertionResult.positionsTexture.Count; i++)
        {
            startEndIndicesDummy.Add(new Vector3(i,i + 5,0));
        }
        Texture2D colorsTexture = PointCacheToTexture2D(colorsDummy);
        Texture2D xWaveTexture = PointCacheToTexture2D(xWaveDummy);
        Texture2D ASMTexture = PointCacheToTexture2D(ASMDummy);
        Texture2D startEndIndicesTexture = PointCacheToTexture2D(startEndIndicesDummy);
        
        
        // create vfx data object, assign textures 
        VFXDataScriptableObject vfxData = ScriptableObject.CreateInstance<VFXDataScriptableObject>();
        vfxData.positionTexture = positionTexture;
        vfxData.linePositionTexture = linePositionTexture;
        vfxData.rotationTexture = rotationTexture;
        vfxData.letterTexture = letterTexture;
        vfxData.scaleTexture = scalesTexture;
        vfxData.colorTexture = colorsTexture;
        vfxData.xWaveMotionTexture = xWaveTexture;
        vfxData.alphaSmoothnessMetalicTexture = ASMTexture;
        vfxData.startEndIndexTexture = startEndIndicesTexture;
        // assign additional information
        vfxData.textureDimension = insertionResult.textureDimension;
        vfxData.letterScale = 1;
        vfxData.numberOfElements = text.Replace(" ", "").Length;
        // assign alphabet
        vfxData.meshAlphabet = alphabet;

        return vfxData;
    }

    public static GameObject CreateEffectFromPath(Path path, string text, AlphabethScriptableObject alphabet, List<float> letterScaling)
    {
        return CreateEffectFromVFXData(CreateVFXDataFromPath(path, text, alphabet, letterScaling, true));
    }
    
    private static Texture2D PointCacheToTexture2D(List<Vector3> pointCache)
    {
        int dimension = Mathf.CeilToInt(Mathf.Sqrt(pointCache.Count));
        Texture2D texture = new Texture2D(dimension, dimension, TextureFormat.RGBAFloat, false);
        for (int i = 0; i < pointCache.Count; i++)
        {
            Vector3 vec = pointCache[i];
            int x = i % dimension;
            int y = i / dimension;
            texture.SetPixel(x,y,new Color(vec.x, vec.y, vec.z));
        }
        texture.Apply();
        return texture;
    }
    
    public static VFXDataScriptableObject CreateVFXDataFromCompleteAnnotation<TextDimensions, EffectDimensions>(CompleteAnnotation<TextDimensions> completeAnnotation, AbstractMapping<TextDimensions, EffectDimensions> mapping, Path path)
    {
        // convert to text dimension data points
        List<TextDimensions> textElements = completeAnnotation.Finish();
        
        Debug.Log($"Number of Text Elements: {textElements.Count}");
        
        // apply mapping to effect dimension data point space
        List<EffectDimensions> effectElements = mapping.ConvertMany(textElements);

        // apply effect dimension elements to vfx data points#
        List<VfxDataPoint> vfxDataPoints = new List<VfxDataPoint>();
        foreach (EffectDimensions eddp in effectElements)
        {
            VfxDataPoint vfxDataPoint = new VfxDataPoint();
            eddp.GetType().GetMethod("Apply").Invoke(eddp, new []{vfxDataPoint});
            vfxDataPoints.Add(vfxDataPoint);
        }
        

        Debug.Log($"Number of VFX DATA Points: {vfxDataPoints.Count}");

        
        // create vfx data from effect points
        AlphabethScriptableObject alphabet = Resources.Load<AlphabethScriptableObject>("alphabet/alphabeth_Absans-Regular");
        VFXDataScriptableObject vfxDataScriptableObject = VFXUtil.ToVfxDataScriptableObject(vfxDataPoints, path, alphabet);

        return vfxDataScriptableObject;
    }

    public static void CreateEffectFromCompleteAnnotation<TextDimensions, EffectDimensions>(CompleteAnnotation<TextDimensions> completeAnnotation, AbstractMapping<TextDimensions, EffectDimensions> mapping, Path path)
    {
        VFXDataScriptableObject data = CreateVFXDataFromCompleteAnnotation(completeAnnotation, mapping, path);
        CreateEffectFromVFXData(data);
    }

    public static VFXDataScriptableObject ToVfxDataScriptableObject(List<VfxDataPoint> dataPoints, Path basePath, AlphabethScriptableObject alphabet)
    {
        Path basePathCopy = basePath.Copy();
        
        // STEP 1: Construct the final basePath, by inserting subPaths as specified by the dataPoints
        // group data by form sections
        PathStrategy currentStrategy = null;
        string groupText = "";
        List<float> letterScaling = new List<float>();
        for (int i = 0; i < dataPoints.Count; i++)
        {
            VfxDataPoint dataPoint = dataPoints[i];
            
            if (currentStrategy == null)
            {
                currentStrategy = dataPoint.subPathStrategy;
                groupText += dataPoint.letter;
                letterScaling.Add(dataPoint.scale);
            }
            else if (currentStrategy == dataPoint.subPathStrategy)
            {
                groupText += dataPoint.letter;
                letterScaling.Add(dataPoint.scale);
            }
            else
            {
                // apply strategy to the base path
                currentStrategy.Apply(basePathCopy, groupText, letterScaling, alphabet);

                // reset
                groupText = "";
                letterScaling = new List<float>();
                currentStrategy = dataPoint.subPathStrategy;
            }
        }
        // apply last strategy to the base path
        currentStrategy.Apply(basePathCopy, groupText, letterScaling, alphabet);

        // STEP 2: create vfx data by inserting the complete text into the base path
        // gather complete text and letter scaling, and colors
        string completeText = "";
        List<float> completeLetterScaling = new List<float>();
        
        for (int i = 0; i < dataPoints.Count; i++)
        {
            completeText += dataPoints[i].letter;
            completeLetterScaling.Add(dataPoints[i].scale);
        }
        
        Debug.Log($"all LETTERS: {completeText.Length}");
        
        // get textures as pCaches
        TextInsertionResult insertionResult = basePathCopy.Copy().ConvertToPointData(completeText, alphabet, completeLetterScaling);
        
        // create vfx data object, assign textures 
        VFXDataScriptableObject vfxData = ScriptableObject.CreateInstance<VFXDataScriptableObject>();
        vfxData.positionTexture = PointCacheToTexture2D(insertionResult.positionsTexture);
        vfxData.linePositionTexture = PointCacheToTexture2D(insertionResult.linePositionTexture);
        vfxData.rotationTexture = PointCacheToTexture2D(insertionResult.rotationsTexture);
        vfxData.letterTexture = PointCacheToTexture2D(insertionResult.lettersTexture);
        vfxData.scaleTexture = PointCacheToTexture2D(insertionResult.scalesTexture);

        // create shortened independent textures
        List<Vector3> colors = new List<Vector3>();
        List<Vector3> xWaveMotions = new List<Vector3>();
        List<Vector3> alphaSmoothnessMetalic = new List<Vector3>();
        List<Vector3> startStopIndices = new List<Vector3>();
        for (int i = 0; i < insertionResult.positionsTexture.Count; i++)
        {
            VfxDataPoint dp = dataPoints[i];
            
            colors.Add(new Vector3(dp.color.r / 255f,dp.color.g / 255f,dp.color.b / 255f));
            xWaveMotions.Add(dp.XWaveMotion.ToVector());
            alphaSmoothnessMetalic.Add(new Vector3(dp.alpha, dp.smoothness, dp.metalic));
            startStopIndices.Add(new Vector3(dp.indexStart,dp.indexEnd,0));
        }

        // apply path independent textures:
        vfxData.colorTexture = PointCacheToTexture2D(colors);
        vfxData.xWaveMotionTexture = PointCacheToTexture2D(xWaveMotions);
        vfxData.alphaSmoothnessMetalicTexture = PointCacheToTexture2D(alphaSmoothnessMetalic);
        vfxData.startEndIndexTexture = PointCacheToTexture2D(startStopIndices);

        // assign additional information
        vfxData.textureDimension = insertionResult.textureDimension;
        vfxData.letterScale = 1;
        vfxData.numberOfElements = completeText.Replace(" ", "").Length;
        // assign alphabet
        vfxData.meshAlphabet = alphabet;
        
       
        return vfxData;
    }
    
    #if (UNITY_EDITOR)
    public static void SaveVFXData(VFXDataScriptableObject data, string name)
    {
        string fileNameTemplate = DirConfiguration.Instance.GetVFXDataScriptableObjectDir(true) + DirConfiguration.GetPCacheFileNamingTemplate();
        string posFileName = String.Format(fileNameTemplate, name, "pos");
        string linePosFileName = String.Format(fileNameTemplate, name, "line_pos");
        string rotFileName = String.Format(fileNameTemplate, name, "rot");
        string letterFileName = String.Format(fileNameTemplate, name, "letter");
        string scaleFileName = String.Format(fileNameTemplate, name, "scale");
        string colorFileName = String.Format(fileNameTemplate, name, "color");
        string xWaveFileName = String.Format(fileNameTemplate, name, "xwave");
        string alphaSmoothnessMetalicFileName = String.Format(fileNameTemplate, name, "AlSmMe");
        string startStopIndexFileName = String.Format(fileNameTemplate, name, "startStopIndex");
        
        
        WritePointCache(data.positionTexture, posFileName);
        WritePointCache(data.linePositionTexture, linePosFileName);
        WritePointCache(data.rotationTexture, rotFileName);
        WritePointCache(data.letterTexture, letterFileName);
        WritePointCache(data.scaleTexture, scaleFileName);
        WritePointCache(data.colorTexture, colorFileName);
        WritePointCache(data.xWaveMotionTexture, xWaveFileName);
        WritePointCache(data.alphaSmoothnessMetalicTexture, alphaSmoothnessMetalicFileName);
        WritePointCache(data.startEndIndexTexture, startStopIndexFileName);
        
        AssetDatabase.Refresh();

        // import pCaches
        AssetDatabase.ImportAsset(posFileName, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(rotFileName, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(letterFileName, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(scaleFileName, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(colorFileName, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(xWaveFileName, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(alphaSmoothnessMetalicFileName, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(startStopIndexFileName, ImportAssetOptions.ForceUpdate);

        AssetDatabase.Refresh();

        // read position part for each pCache
        Texture2D positionTexture = ReadTextureFromPointCache(posFileName);
        Texture2D linePositionTexture = ReadTextureFromPointCache(linePosFileName);
        Texture2D rotationTexture = ReadTextureFromPointCache(rotFileName);
        Texture2D letterTexture = ReadTextureFromPointCache(letterFileName);
        Texture2D scaleTexture = ReadTextureFromPointCache(scaleFileName);
        Texture2D colorTexture = ReadTextureFromPointCache(colorFileName);
        Texture2D xWaveMotionTexture = ReadTextureFromPointCache(xWaveFileName);
        Texture2D alphaSmoothnessMetalicTexture = ReadTextureFromPointCache(alphaSmoothnessMetalicFileName);
        Texture2D startStopIndexTexture = ReadTextureFromPointCache(startStopIndexFileName);
            
        // create vfx data object, assign textures 
        data.positionTexture = positionTexture;
        data.linePositionTexture = linePositionTexture;
        data.rotationTexture = rotationTexture;
        data.letterTexture = letterTexture;
        data.scaleTexture = scaleTexture;
        data.colorTexture = colorTexture;
        data.xWaveMotionTexture = xWaveMotionTexture;
        data.alphaSmoothnessMetalicTexture = alphaSmoothnessMetalicTexture;
        data.startEndIndexTexture = startStopIndexTexture;

        // save vfx data as asset
        AssetDatabase.CreateAsset(data, DirConfiguration.Instance.GetVFXDataScriptableObjectDir(true) + name + ".asset");
        AssetDatabase.SaveAssets();
        Debug.Log($"SAVED TO: {DirConfiguration.Instance.GetVFXDataScriptableObjectDir(true) + name + ".asset"}");
    }
    #endif
    
    private static void WritePointCache(Texture2D texture2D, string fileName)
    {
        List<Vector3> convertedTexture = texture2D.GetPixels().Select(px => new Vector3(px.r, px.g, px.b)).ToList();
        WritePointCache(convertedTexture, fileName);
    }
    private static void WritePointCache(List<Vector3> data, string fileName)
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
    
    #if (UNITY_EDITOR)
    private static Texture2D ReadTextureFromPointCache(string filename)
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
    #endif   
}
