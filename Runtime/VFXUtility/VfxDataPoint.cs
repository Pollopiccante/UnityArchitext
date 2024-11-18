using UnityEngine;

public class VfxDataPoint
{
    public char letter = '@';
    public Vector3 positionOffset = Vector3.zero;
    public Quaternion rotationOffset = Quaternion.identity;
    public float scale = 1f;
    public Color color = new Color(0, 0, 0);
    public PathStrategy subPathStrategy;
    public WaveMotionData XWaveMotion = new WaveMotionData(0,0,0);
    public float alpha = 1f;
    public float smoothness = 0.5f;
    public float metalic = 0;
    public float indexStart = 0;
    public float indexEnd = 1;
}
