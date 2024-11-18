using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class MobyDickTextDimensionsDataPoint : BasicDimensionDataPoint 
{
    // moby dick annotation dimensions:
    // 1. urban <-> nature (float: -1 to 1)
    // 2. sea related (float: 0 to 1)
    // 3. quiet vs loud (float: -1 to 1)
    // 4. profound vs basic (string: profound | basic | default)
    // 5. sadness (0 to 1)
    // 6. narration (string: all-knowing | direct-to-reader | default)
    // 7. character (string: Ismael | Captain Ahab)
    public UrbanNatureDimension UrbanNature = new UrbanNatureDimension();
    public SeaRelatedDimension SeaRelated = new SeaRelatedDimension();
    public SilentLoudDimension SilentLoud = new SilentLoudDimension();
    public ProfoundBasicDimension ProfoundBasic = new ProfoundBasicDimension();
    public SadnessDimension Sadness = new SadnessDimension();
    public NarrationDimension Narration = new NarrationDimension();
    public CharacterDimension Character = new CharacterDimension();
    
}

public class MobyDickEffectDimensionsDataPoint : EffectDimensionDataPoint
{
    public ScaleEffectDimension Scale = new ScaleEffectDimension();
    public ColorEffectDimension Color = new ColorEffectDimension();
    public XWaveMotionEffectDimension XWave = new XWaveMotionEffectDimension();
    public AlphaEffectDimension Alpha = new AlphaEffectDimension();
    public SmoothnessEffectDimension Smoothness = new SmoothnessEffectDimension();
    public MetalicEffectDimension Metalic = new MetalicEffectDimension();
    public IndexStartEndEffectDimension IndexStartEnd = new IndexStartEndEffectDimension();
}

public class MobyDickMapping : AbstractMapping<MobyDickTextDimensionsDataPoint, MobyDickEffectDimensionsDataPoint>
{
    public override MobyDickEffectDimensionsDataPoint Convert(MobyDickTextDimensionsDataPoint textDimensions, int index)
    {
        MobyDickEffectDimensionsDataPoint outDimensions = new MobyDickEffectDimensionsDataPoint();

        // default fly in, start end index
        outDimensions.IndexStartEnd.value = new StartEndIndex(index, index + 1);
        
        // effect dimension parts influenced by multiple text dimensions
        float colorProgress = (Mathf.Sin(((index % 30) / 30f * 360f) * Mathf.Deg2Rad) + 1) / 2f;
        Color finalColor = Color.Lerp(new Color(0,0,128), new Color(173,216,230), colorProgress); // default color is sin wave blue
        float finalScale = 2.1f;
        
        // MAPPING:
        // Sadness: smaller, add darkness to color
        const float lowSadnessScale = 1f;
        const float highSadnessScale = 0.3f;
        const float lowSadnessDarknessOffset = 0;
        const float highSadnessDarknessOffset = 230;

        // Character: Ishmael: Blue
        if (textDimensions.Character.value == "Ishmael")
        {
            finalColor = new Color(0, 0, 255);
        }
        // Sadness: darker, and smaller
        if (textDimensions.Sadness.value > 0f)
        {
            finalScale *= Mathf.Lerp(lowSadnessScale, highSadnessScale, textDimensions.Sadness.value);
            float darknessOffset = Mathf.Lerp(lowSadnessDarknessOffset, highSadnessDarknessOffset, textDimensions.Sadness.value);
            finalColor = new Color(finalColor.r - darknessOffset, finalColor.g - darknessOffset, finalColor.b - darknessOffset);
        }
        // Basic: no metallic reflection, add some randomness to color, add some randomness to scale (minimal)
        if (textDimensions.ProfoundBasic.value == "basic")
        {
            const float randomnessDegree = 0.2f;
            const float approachPercentage = 0.8f;
            // color
            float h, s, v;
            Color.RGBToHSV(finalColor, out h, out s, out v);
            float hMax = (h + randomnessDegree / 2f) % 1f;
            float hMin = (h - randomnessDegree / 2f) % 1f;
            float sMax = (s + randomnessDegree / 2f) % 1f;
            float sMin = (s - randomnessDegree / 2f) % 1f;
            float vMax = (v + randomnessDegree / 2f) % 1f;
            float vMin = (v - randomnessDegree / 2f) % 1f;
            outDimensions.Metalic.value = 0f;
            Color randomColor = Random.ColorHSV(hMin, hMax, sMin, sMax, vMin, vMax);
            finalColor = Color.Lerp(finalColor, new Color(randomColor.r * 255, randomColor.g * 255, randomColor.b * 255), approachPercentage);
            // scale
            const float scaleRandomness = 0.1f;
            finalScale *= Random.Range(1 - scaleRandomness / 2f, 1 + scaleRandomness / 2f);
        }
        // Profound: max metallic reflection, max white, slow movement, slow fly in, slightly bigger
        if (textDimensions.ProfoundBasic.value == "profound")
        {
            outDimensions.Metalic.value = 1f;
            finalColor = new Color(255, 255, 255);
            
            outDimensions.XWave.value = new WaveMotionData(0.1f, 0.5f, 0);
            finalScale *= 1.3f;
            const float flyInTime = 10f;
            float endIndex = index + 1;
            float startIndex = Mathf.Max((index + 1) - flyInTime, 0);
            outDimensions.IndexStartEnd.value = new StartEndIndex(startIndex, endIndex);
        }
        
        // Silent: small
        // Loud: big
        const float silentLoundScalingFactor = 2f;
        if (textDimensions.SilentLoud.value < 0f)
        {
            finalScale /= Mathf.Abs(textDimensions.SilentLoud.value) * silentLoundScalingFactor;
        }else if (textDimensions.SilentLoud.value > 0f)
        {
            finalScale *= Mathf.Abs(textDimensions.SilentLoud.value) * silentLoundScalingFactor;
        }
        
        // sea related: wave motion, dark blue and white
        if (textDimensions.SeaRelated.value != 0f)
        {
            Color[] deepOceanColors = new Color[5];
            ColorUtility.TryParseHtmlString("#001a33", out deepOceanColors[0]);
            ColorUtility.TryParseHtmlString("#003366", out deepOceanColors[1]);
            ColorUtility.TryParseHtmlString("#004080", out deepOceanColors[2]);
            ColorUtility.TryParseHtmlString("#0059b3", out deepOceanColors[3]);
            ColorUtility.TryParseHtmlString("#0066cc", out deepOceanColors[4]);

            int colorToPickFrom = Mathf.CeilToInt(textDimensions.SeaRelated.value * 5);
            int pickedColorIndex = Random.Range(0, Math.Min(colorToPickFrom, 5));
            Color pickedColor = deepOceanColors[pickedColorIndex];

            finalColor = Color.Lerp(finalColor, pickedColor, Mathf.Max(0.4f, textDimensions.SeaRelated.value));
            
            // wave motion
            outDimensions.XWave.value = new WaveMotionData(textDimensions.SeaRelated.value / 2, 0.5f, (index % 10f) / 10f);
        }
        
        // Urban: fixed concrete size ratios (1, 2, 3, 4), like different sized Buildings
        // Nature: Developing Golden Ratio
        float scaleStep = finalScale / 2;
        if (textDimensions.UrbanNature.value < 0f) // Urban
        {
            finalScale = scaleStep * (1 + Random.Range(0, 4));
        }else if (textDimensions.UrbanNature.value > 0f) // Nature
        {
            float progressionAngle = (index % 30) / 30f * 360f;
            float rad = progressionAngle * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad);
            float sinProgress = (sin + 1) / 2f;
            finalScale = (scaleStep / 2f) + sinProgress * (scaleStep * 3.5f);
        }
        
        outDimensions.Color.value = finalColor;
        outDimensions.Scale.value = finalScale;
        
        
        // if (textDimensions.Sadness.value > 0.5f)
        // {
        //     outDimensions.Scale.value = 5f;
        //     outDimensions.Color.value = new Color(255, 0, 0);
        //     outDimensions.XWave.value = new WaveMotionData(0.005f, 200, 0);
        //     
        //     outDimensions.Alpha.value = 0.2f;
        //     outDimensions.IndexStartEnd.value = new StartEndIndex(index, index + 10);
        // }
        // else
        // {
        //     outDimensions.XWave.value = new WaveMotionData(0, 0, 0);
        //     outDimensions.Metalic.value = 1f;
        //     outDimensions.IndexStartEnd.value = new StartEndIndex(index, index + 1);
        //     
        //     Color randomColor = Random.ColorHSV();
        //     outDimensions.Color.value = new Color(randomColor.r * 255, randomColor.g * 255, randomColor.b * 255);
        //     
        // }

        // pass through letter and subPathStrategy
        outDimensions.Letter = textDimensions.Letter;
        outDimensions.SubPathStrategy = textDimensions.SubPathStrategy;

        return outDimensions;
    }
}