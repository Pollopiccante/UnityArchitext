using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothnessEffectDimension : EffectDimension<float>
{
    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.smoothness = value;
    }

    public override float GetDefaultValue()
    {
        return 0.5f;
    }
}
