
public class ScaleEffectDimension : EffectDimension<float>
{
    public static string name = "scale";
   
    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.scale = value;
    }
    public override float GetDefaultValue()
    {
        return 1f;
    }
}
