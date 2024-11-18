using UnityEngine;

public class ColorEffectDimension : EffectDimension<Color>
{
    public override Color GetDefaultValue()
    {
        return new Color(0, 0, 0);
    }

    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.color = value;
    }
}
