using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class PathStrategy: ScriptableObject
{
    public void Apply(Path mainPath, string text, List<float> letterScaling, AlphabethScriptableObject alphabet)
    {
        ApplyStrategyToPath(mainPath, GetPath(text, letterScaling, alphabet), text, letterScaling, alphabet);
    }

    protected virtual void ApplyStrategyToPath(Path mainPath, Path ownPath, string text, List<float> letterScaling, AlphabethScriptableObject alphabet)
    {
        mainPath.InsertSubPath(ownPath);
    }
    
    protected abstract Path GetPath(string text, List<float> letterScaling, AlphabethScriptableObject alphabet);
}
