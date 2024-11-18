public class SubPathEffectDimension : EffectDimension<PathStrategy>
{
    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.subPathStrategy = value;
    }
    public override PathStrategy GetDefaultValue()
    {
        // return object with the same identity, to connect into a single line by default
        return SkipPathStrategy.Default();
    }
}
