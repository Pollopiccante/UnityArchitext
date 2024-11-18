

public class LetterEffectDimension : EffectDimension<char>
{
    public static string name = "letter";

    public override void Apply(VfxDataPoint dataPoint)
    {
        dataPoint.letter = value;
    }
    public override char GetDefaultValue()
    {
        return '@';
    }
}
