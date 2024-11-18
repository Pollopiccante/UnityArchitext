using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathStrategyNamePair
{
    public string name;
    public PathStrategy pathStrategy;

    public PathStrategyNamePair(string name, PathStrategy pathStrategy)
    {
        this.name = name;
        this.pathStrategy = pathStrategy;
    }
}

[CreateAssetMenu(fileName = "PathStrategyImporter", menuName = "AnnotationImporter/PathStrategyImporter", order = 1)]
public class PathStrategyImporter : AnnotationImporter
{
    public List<PathStrategyNamePair> meshes = new List<PathStrategyNamePair>();
    
    public override ValueWrapper ReadValue(string encodedValue)
    {
        ValueWrapper vw = new ValueWrapper();
        
        foreach (PathStrategyNamePair pair in meshes)
        {
            Debug.Log($"PAIR: {pair.name} EncodedValue: {encodedValue}");
            if (pair.name.Equals(encodedValue))
            {
                vw.meshPathStrategyValue = pair.pathStrategy;
                return vw;
            }
        }
        throw new ArgumentException($"Encoded Path Strategy Value \"{encodedValue}\" could not be decoded.");
    }

    public override ValueWrapper GetDefaultValue()
    {
        ValueWrapper vw = new ValueWrapper();
        vw.meshPathStrategyValue = SkipPathStrategy.Default();
        return vw;
    }
}
