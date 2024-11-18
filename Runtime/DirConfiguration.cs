#if (UNITY_EDITOR) 

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DirConfiguration", menuName = "ScriptableObjects/DirConfiguration", order = 1)]
public class DirConfiguration : ScriptableObject
{
    public static string _dirConfigPath = "Assets/PackageRefactor/DirConfig.asset";
    private static DirConfiguration _instance;

    public static DirConfiguration Instance
    {
        get{
            if (_instance == null)
            {
                _instance = AssetDatabase.LoadAssetAtPath<DirConfiguration>(_dirConfigPath);
                if (_instance == null)
                {
                    DirConfiguration dirConfiguration = CreateInstance<DirConfiguration>();
                    AssetDatabase.CreateAsset(dirConfiguration, _dirConfigPath);
                }
            }
            return _instance;
        }
    }
    
    public string pathScriptableObjectDir = "DataObjects/PathObjects/";
    public string vfxDataScriptableObjectDir = "DataObjects/VFXObjects/";
    public static string GetPCacheFileNamingTemplate()
    {
        return "{0}" + "_{1}_" + DateTime.Now.ToString("yyyymmdd") + ".pcache";
    }

    public string GetPathScriptableObjectDir()
    {
        return GetLocalPath() + pathScriptableObjectDir;
    }

    public string GetVFXDataScriptableObjectDir(bool fromAssets=false)
    {
        string fullPath = GetLocalPath() + vfxDataScriptableObjectDir;

        if (fromAssets)
        {
            Debug.Log("PARTS OF ASSETS:");
            Debug.Log(fullPath.Split("Assets")[0]);
            Debug.Log(fullPath.Split("Assets")[1]);
            
            return "Assets" + fullPath.Split("Assets")[1];
        }
        
        return fullPath;
    }

    private static string GetLocalPath()
    {
        string[] res = Directory.GetFiles(Application.dataPath, "DirConfig.asset", SearchOption.AllDirectories);
        if (res.Length == 0)
        {
            Debug.LogError("DirConfiguration Script could not be located");
            return null;
        }
        string path = res[0].Replace("DirConfig.asset", "").Replace("\\", "/");
        Debug.Log($"localPath: {path}");
        return path;
    }
}

#endif
