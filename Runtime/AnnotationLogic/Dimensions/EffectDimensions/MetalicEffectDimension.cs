using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalicEffectDimension : EffectDimension<float>
{
    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.metalic = value;
    }

    public override float GetDefaultValue()
    {
        return 0f;
    }
}
