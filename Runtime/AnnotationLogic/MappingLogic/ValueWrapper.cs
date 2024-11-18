using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ValueWrapper
{
    public object getValue(Type type)
    {
        if (type == typeof(char))
            return charValue;
        if (type == typeof(int))
            return integerValue;
        if (type == typeof(float))
            return floatValue;
        if (type == typeof(PathStrategy))
            return meshPathStrategyValue;
        if (type == typeof(string))
            return stringValue;
        
        throw new System.ArgumentException($"type {type} is not present in wrapper object");
    }
    
    public int integerValue;
    public char charValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;
    public PathStrategy meshPathStrategyValue;
    public GenericSection genericSectionValue;

    public static List<ValueWrapper> FromString(string s)
    {
        List<ValueWrapper> valuesList = new List<ValueWrapper>();
        foreach (char c in s)
        {
            ValueWrapper vw = new ValueWrapper();
            vw.charValue = c;
            valuesList.Add(vw);
        }
        return valuesList;
    }

    public static List<ValueWrapper> FromGenericSections(List<GenericSection> sections)
    {
        List<ValueWrapper> valuesList = new List<ValueWrapper>();
        foreach (GenericSection s in sections)
        {
            ValueWrapper vw = new ValueWrapper();
            vw.genericSectionValue = s;
            valuesList.Add(vw);
        }
        return valuesList;
    }
}

