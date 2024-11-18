using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaEffectDimension : EffectDimension<float>
{
    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.alpha = value;
    }

    public override float GetDefaultValue()
    {
        return 1f;
    }
}
