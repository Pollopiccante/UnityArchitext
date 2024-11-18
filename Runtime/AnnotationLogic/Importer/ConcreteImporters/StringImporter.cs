using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StringImporter", menuName = "AnnotationImporter/StringImporter", order = 2)]
public class StringImporter : AnnotationImporter
{
    public override ValueWrapper ReadValue(string encodedValue)
    {
        ValueWrapper vw = new ValueWrapper();
        vw.stringValue = encodedValue;
        return vw;
        
    }

    public override ValueWrapper GetDefaultValue()
    {
        ValueWrapper vw = new ValueWrapper();
        vw.stringValue = "";
        return vw;
    }
}
