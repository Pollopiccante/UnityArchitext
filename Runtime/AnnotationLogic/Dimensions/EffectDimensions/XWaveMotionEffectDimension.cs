
using UnityEngine;

public class WaveMotionData
{
    public float Amplitude;
    public float Frequency;
    public float Offset;

    public WaveMotionData(float amplitude, float frequency, float offset)
    {
        Amplitude = amplitude;
        Frequency = frequency;
        Offset = offset;
    }

    public Vector3 ToVector()
    {
        return new Vector3(Amplitude, Frequency, Offset);
    }
}

public class XWaveMotionEffectDimension : EffectDimension<WaveMotionData>
{
    public override WaveMotionData GetDefaultValue()
    {
        return new WaveMotionData(0, 0, 0);
    }
    
    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.XWaveMotion = value;
    }
}
