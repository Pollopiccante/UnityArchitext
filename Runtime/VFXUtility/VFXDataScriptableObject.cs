using UnityEngine;
using UnityEngine.VFX;

public class VFXDataScriptableObject : ScriptableObject
{
    public AlphabethScriptableObject meshAlphabet;
    public int numberOfElements;
    public float letterScale;
    public int textureDimension;
    public Texture2D letterTexture;
    public Texture2D positionTexture;
    public Texture2D linePositionTexture;
    public Texture2D rotationTexture;
    public Texture2D scaleTexture;
    public Texture2D colorTexture;
    public Texture2D xWaveMotionTexture;
    public Texture2D alphaSmoothnessMetalicTexture;
    public Texture2D startEndIndexTexture;

    public static Vector3[] GetTextureAsVectors(Texture2D texture2D, VisualEffect effect)
    {
        // save final positions of effect
        int dimensions = effect.GetInt("Dimension");
        Vector3[] vectors = new Vector3[dimensions * dimensions];
        for (int x = 0; x < dimensions; x++)
        {
            for (int y = 0; y < dimensions; y++)
            {
                Color pixelColor = texture2D.GetPixel(y, x);
                vectors[x * dimensions + y] = new Vector3(pixelColor.r, pixelColor.g, pixelColor.b);
            }
        }
        return vectors;
    }
}
