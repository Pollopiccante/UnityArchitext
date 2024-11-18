
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class DefaultTextDimensionsDataPoint : BasicDimensionDataPoint 
{
    public AngryDimension Angry = new AngryDimension();
    public HumorDimension Humor = new HumorDimension();
}

public class DefaultEffectDimensionsDataPoint : EffectDimensionDataPoint
{
    public ScaleEffectDimension Scale = new ScaleEffectDimension();
    public ColorEffectDimension Color = new ColorEffectDimension();
    public XWaveMotionEffectDimension XWave = new XWaveMotionEffectDimension();
    public AlphaEffectDimension Alpha = new AlphaEffectDimension();
    public SmoothnessEffectDimension Smoothness = new SmoothnessEffectDimension();
    public MetalicEffectDimension Metalic = new MetalicEffectDimension();
    public IndexStartEndEffectDimension IndexStartEnd = new IndexStartEndEffectDimension();
}
public class DefaultMapping : AbstractMapping<DefaultTextDimensionsDataPoint, DefaultEffectDimensionsDataPoint>
{
    public override DefaultEffectDimensionsDataPoint Convert(DefaultTextDimensionsDataPoint textDimensions, int index)
    {
        DefaultEffectDimensionsDataPoint outDimensions = new DefaultEffectDimensionsDataPoint();

        // new default size: x2
        outDimensions.Scale.value = 2f;
        
        if (textDimensions.Angry.value > 0.5f)
        {
            
            outDimensions.Scale.value = 5f;
            outDimensions.Color.value = new Color(255, 0, 0);
            outDimensions.XWave.value = new WaveMotionData(0.005f, 200, 0);
            
            outDimensions.Alpha.value = 0.2f;
            outDimensions.IndexStartEnd.value = new StartEndIndex(index, index + 10);
        }
        else
        {
            outDimensions.XWave.value = new WaveMotionData(0, 0, 0);
            outDimensions.Metalic.value = 1f;
            outDimensions.IndexStartEnd.value = new StartEndIndex(index, index + 1);
            
            Color randomColor = Random.ColorHSV();
            outDimensions.Color.value = new Color(randomColor.r * 255, randomColor.g * 255, randomColor.b * 255);
            
        }

        // pass through letter and subPathStrategy
        outDimensions.Letter = textDimensions.Letter;
        outDimensions.SubPathStrategy = textDimensions.SubPathStrategy;

        return outDimensions;
    }
}

