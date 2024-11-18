using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SkipPathStrategy", menuName = "ScriptableObjects/Paths/SkipPathStrategy", order = 0)] 
public class SkipPathStrategy : PathStrategy
{
    protected override Path GetPath(string text, List<float> letterScaling, AlphabethScriptableObject alphabet)
    {
        return null;
    }

    protected override void ApplyStrategyToPath(Path mainPath, Path ownPath, string text, List<float> letterScaling, AlphabethScriptableObject alphabet)
    {
        float lengthToSkip = alphabet.CalculateTextLength(text, letterScaling);
        mainPath.MoveDistanceOnPath(lengthToSkip);
    }
    
    private static SkipPathStrategy _default = null;
    public static SkipPathStrategy Default()
    {
        if (_default == null)
        {
            _default = CreateInstance<SkipPathStrategy>();
        }
        return _default;
    }
}
