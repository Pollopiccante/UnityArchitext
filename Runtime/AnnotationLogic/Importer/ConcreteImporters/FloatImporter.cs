using System.Globalization;
using UnityEngine;


[CreateAssetMenu(fileName = "FloatImporter", menuName = "AnnotationImporter/FloatImporter", order = 0)]
public class FloatImporter : AnnotationImporter
{
    public override ValueWrapper ReadValue(string encodedValue)
    {
        ValueWrapper vw = new ValueWrapper();
        vw.floatValue = float.Parse(encodedValue, CultureInfo.InvariantCulture);
        return vw;
    }

    public override ValueWrapper GetDefaultValue()
    {
        return new ValueWrapper();
       
    }
}
