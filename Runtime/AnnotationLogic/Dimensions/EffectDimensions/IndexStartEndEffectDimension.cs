using System;

public class StartEndIndex
{
    private float start;
    private float end;

    public StartEndIndex(float start, float end)
    {
        if (end <= this.start)
            throw new ArgumentException("end can not be before start");
        this.start = start;
        this.end = end;
    }

    public float GetStart()
    {
        return this.start;
    }

    public float GetEnd()
    {
        return this.end;
    }
}

public class IndexStartEndEffectDimension : EffectDimension<StartEndIndex>
{
    public override StartEndIndex GetDefaultValue()
    {
        return new StartEndIndex(0, 1);
    }

    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.indexStart = value.GetStart();
        dataPoint.indexEnd = value.GetEnd();
    }
}
