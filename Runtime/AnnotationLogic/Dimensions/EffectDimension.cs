using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectDimension<T>: Dimension<T>
{
    public abstract void Apply(VfxDataPoint dataPoint);
}
