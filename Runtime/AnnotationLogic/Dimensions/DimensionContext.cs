
using System;
using System.Reflection;
using Unity.VisualScripting;

public class BasicDimensionDataPoint
{
    public LetterEffectDimension Letter = new LetterEffectDimension();
    public SubPathEffectDimension SubPathStrategy = new SubPathEffectDimension();
}

public class EffectDimensionDataPoint: BasicDimensionDataPoint
{
    public void Apply(VfxDataPoint vfxDataPoint)
    {
        Type ownType = this.GetType();
        FieldInfo[] fields = ownType.GetFields();

        foreach (FieldInfo field in fields)
        {
            object fieldValue = field.GetValue(this);
            MethodInfo applyMethod = fieldValue.GetType().GetMethod("Apply");
            if (applyMethod != null)
                applyMethod.Invoke(fieldValue, new []{vfxDataPoint});
            else
                throw new Exception("All fields of EffectDimensionDataPoints must be of type: EffectDimension");
        }
    }
}
