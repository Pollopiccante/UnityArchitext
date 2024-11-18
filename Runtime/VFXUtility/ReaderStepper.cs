using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public enum ReadingMode
{
    IndexMode = 0,
    LengthMode = 1,
}

[ExecuteAlways]
public class ReaderStepper : MonoBehaviour
{
    public bool pause = true;
    
    
    public float currentReadingTime = 0f;
    public ReadingMode readingMode = ReadingMode.IndexMode;
    public float indicesPerSecond = 1f;
    public float lengthPerSecond = 10f;
    
    private VisualEffect _effect;
    private IndexStepper _indexStepper;

    private Vector3[] linePositions;
    private void Start()
    {
        _effect = gameObject.GetComponent<VisualEffect>();
        _indexStepper = gameObject.GetComponent<IndexStepper>();
        
        // set initial positions 
        _effect.SetVector3("LinePositionLeft", new Vector3(0,5, 20));
        _effect.SetVector3("LinePositionRight", new Vector3(0,5, -20));
        _effect.SetVector3("LineDirectionRight", new Vector3(0,-90, 0));
        _effect.SetVector3("LineDirectionLeft", new Vector3(0,-90, 0));
        
        // save positions on line
        linePositions = VFXDataScriptableObject.GetTextureAsVectors((Texture2D) _effect.GetTexture("LinePositions"), _effect);
        
        for (int i = 1; i< linePositions.Length; i++)
        {
            if (!LinePosGreaterEqual(linePositions[i], linePositions[i - 1]))
            {
                Debug.Log($"NOT BIGGER prev: {linePositions[i - 1]} succ: {linePositions[i]}");
            }
        }
    }

    private Vector3 oldLeftLinePos = new Vector3(0,0,0);
    private Vector3 oldRightLinePos = new Vector3(0,0,0);
    
    public void UpdateReaderPosition(Vector3 leftLinePos, Vector3 rightLinePos, Quaternion leftLineRot, Quaternion rightLineRot)
    {
        Vector3 newLeftLinePos = Vector3.Lerp(oldLeftLinePos, leftLinePos, 0.4f);
        Vector3 newRightLinePos = Vector3.Lerp(oldRightLinePos, rightLinePos, 0.4f);

        if (_effect != null)
        {
            _effect.SetVector3("LinePositionLeft", newLeftLinePos);
            _effect.SetVector3("LinePositionRight", newRightLinePos);
            _effect.SetVector3("LineDirectionLeft", leftLineRot.eulerAngles);
            _effect.SetVector3("LineDirectionRight", rightLineRot.eulerAngles);
        }

        oldLeftLinePos = newLeftLinePos;
        oldRightLinePos = newRightLinePos;
    }

    private float GetLineDistanceBetweenIndices(float from, float to)
    {
        // calc distance as vector
        Vector3 fromPosOnLine = AddLinePositions(GetPositionOnLine(to), -1 * GetPositionOnLine(from));
        // transform to float
        return fromPosOnLine.x * 100 * 100 + fromPosOnLine.y * 100 + fromPosOnLine.z;
    }

    private float GetLineDistanceBetweenIndexAndPos(float from, Vector3 toPos)
    {
        // calc distance as vector
        Vector3 fromPosOnLine = AddLinePositions(toPos, -1 * GetPositionOnLine(from));
        // transform to float
        return fromPosOnLine.x * 100 * 100 + fromPosOnLine.y * 100 + fromPosOnLine.z;
    }

    
    private float LinePositionToFloat(Vector3 linePosition)
    {
        return linePosition.x * 100 * 100 + linePosition.y * 100 + linePosition.z;
    }

    private Vector3 AddToLinePos(Vector3 linePos, float distanceToAdd)
    {
        linePos.z += distanceToAdd;
        linePos.y += Mathf.FloorToInt(linePos.z / 100f);
        linePos.x += Mathf.FloorToInt(linePos.y / 100f);
        linePos.z %= 100;
        linePos.y %= 100;
        return linePos;
    }

    private Vector3 AddLinePositions(Vector3 linePos1, Vector3 linePos2)
    {
        Vector3 linePos = linePos1 + linePos2;
        if (linePos.z >= 0)
            linePos.y += Mathf.FloorToInt(linePos.z / 100f);
        else
            linePos.y += Mathf.CeilToInt(linePos.z / 100f);

        if (linePos.y >= 0)
            linePos.x += Mathf.FloorToInt(linePos.y / 100f);
        else
            linePos.x += Mathf.CeilToInt(linePos.y / 100f);

        linePos.z %= 100;
        linePos.y %= 100;
        return linePos;
    }
    
    private Vector3 GetPositionOnLine(float index)
    {
        Vector3 basePosition = linePositions[Mathf.FloorToInt(index)];
        Vector3 prevPosition = Vector3.zero;
        if (index - 1 >= 0)
            prevPosition = linePositions[Mathf.CeilToInt(index - 1)];
        float distanceBetween = LinePositionToFloat(linePositions[Mathf.CeilToInt(index)] - prevPosition);
        float additionalLength =  distanceBetween * (index % 1);
        basePosition = AddToLinePos(basePosition, additionalLength);
        return basePosition;
    }

    private bool LinePosGreaterEqual(Vector3 linepos1, Vector3 linePos2)
    {
        if (linepos1.x > linePos2.x)
            return true;
        if (linepos1.y > linePos2.y)
            return true;
        if (linepos1.z >= linePos2.z)
            return true;
        return false;
    }
    
    private void Update()
    {
        if (!pause)
        {
            if (readingMode == ReadingMode.IndexMode)
            {
                // add index traveled directly
                currentReadingTime += Time.deltaTime * indicesPerSecond;
                _effect.SetFloat("ReadingIndex", currentReadingTime);
            }else if (readingMode == ReadingMode.LengthMode)
            {
                // calculate length traveled in update, convert to index traveled
                float lengthTraveled = Time.deltaTime * lengthPerSecond;
                Vector3 newPositionOnLine = AddToLinePos(GetPositionOnLine(currentReadingTime), lengthTraveled);

                
                for (int i = Mathf.FloorToInt(currentReadingTime); i < linePositions.Length; i++)
                {
                    // get first index after new position
                    if (LinePosGreaterEqual(GetPositionOnLine(i), newPositionOnLine))
                    {
                        
                        // go to prev and interpolate
                        float distPrevToCurr = GetLineDistanceBetweenIndices(i-1, i);
                        
                        
                        float relativeDistFromPrev = GetLineDistanceBetweenIndexAndPos(i-1, newPositionOnLine);
                        
                        float newReadingIndex = i - 1 + relativeDistFromPrev / distPrevToCurr;
                        currentReadingTime = newReadingIndex;
                        if (currentReadingTime < 0)
                            currentReadingTime = 0;
                        _effect.SetFloat("ReadingIndex", currentReadingTime);
                        break;
                    }
                }
            }
        }
    }

    public void SyncIndexTime()
    {
        // TODO: get texture with width values of letters to calculate local speeds of letters per unit
    }
}
