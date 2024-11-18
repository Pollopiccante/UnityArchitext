using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "meshpathstrategy", menuName = "ScriptableObjects/Paths/MeshPathStrategy", order = 0)]
public class MeshPathStrategy : PathStrategy
{
    public Mesh mesh;
    public float meshScaling = 30;
    private static readonly float _lettersPerSection = 3f;
    private static float _errorPercentage = 0.05f;
    private static int maxIterations = 10;

    public MeshPathStrategy(Mesh mesh)
    {
        this.mesh = mesh;
    }

    protected override Path GetPath(string text, List<float> letterScaling, AlphabethScriptableObject alphabet)
    {
        // Step 1: create estimated first path
        float length = alphabet.CalculateTextLength(text, letterScaling);
        float averageCharacterSize = length / text.Length;
        float unitSize = (averageCharacterSize * _lettersPerSection);
        
        // target value for first estimation step
        float targetNumberOfRasterPoints = (length / unitSize) * 1.2f + 1;
        
        // if target raster point number is too low, it is unlikely that a mesh can be filled
        if (targetNumberOfRasterPoints < 20)
            throw new Exception($"target number of raster points was too low: {targetNumberOfRasterPoints}; decrease letters per section, or increase letter count");
        
        // iterate 10 times, or until the error is acceptable
        float error = 0f;
        Mesh preScaledMesh = new Mesh();
        preScaledMesh.vertices = mesh.vertices;
        preScaledMesh.triangles = mesh.triangles;
        for (int i = 0; i < 10; i++)
        {
            Raster raster = Rasterizer.RasterizeMesh(preScaledMesh, unitSize);
            int numberOfRasterPoints = raster.GetRealPoints().Count;
            float newScalingFactor = targetNumberOfRasterPoints / numberOfRasterPoints;
        
            if (0.9 < newScalingFactor && newScalingFactor < 1.1)
                break;
            
            // scaling could increase the volume by scalingFactor³
            // compensate here, so overshooting is impossible
            float scaling = Mathf.Pow(newScalingFactor, 1f / 3f);
            preScaledMesh = ScaleMesh(preScaledMesh, scaling);
        }
        

        
        // create first path
        Path firstPath = MeshToPath.ConvertMeshToPath(preScaledMesh, unitSize);
        
        
        // Step 2: further correct the mesh scale by calculating error over real letter insertion
        Path bestPath = null; // most optimal, slightly too short path
        float bestPathValue = 1;
        string leftovers = "";
        for (int j = 0; j < 10; j++)
        {
            // calculate errors
            Path pathCopy = firstPath.Copy();
            Debug.Log($"COMPLETE TEXT IN MESH PATH: {text.Substring(0, 20)}");
            TextInsertionResult result = pathCopy.ConvertToPointData(text, alphabet, letterScaling);
            int textNotInserted = result.leftoverText.Length;
        
            float fullTextLength = alphabet.CalculateTextLength(text, letterScaling);
        
            float textLengthNotAccomodated = alphabet.CalculateTextLength(result.leftoverText, letterScaling.GetRange(text.Length - textNotInserted - 1, textNotInserted));
            float pathLengthUsed = pathCopy.GetLengthToStartFromCurrentPosition();
            float fullPathLength = pathCopy.GetLength();
        
            // float tooShortScalingFactor = (fullTextLength) / (fullTextLength - textLengthNotAccomodated);
            // float tooShortScalingFactor = pathLengthUsed / (textLengthNotAccomodated + pathLengthUsed);
            float tooShortScalingFactor = (fullTextLength + textLengthNotAccomodated) / fullTextLength;
            float tooLongScalingFactor = (pathLengthUsed) / fullPathLength;
        
            // path to short
            float newScaling;
            if (tooShortScalingFactor > 1f)
            {
                newScaling = Mathf.Pow(tooShortScalingFactor, 1 / 3f);
                preScaledMesh = ScaleMesh(preScaledMesh, newScaling);
            }
            // path too long
            else if (tooLongScalingFactor < 1f)
            {
                newScaling = Mathf.Pow(tooLongScalingFactor, 1 / 3f);
                preScaledMesh = ScaleMesh(preScaledMesh, newScaling);
            }
            
            bool validBestPath = tooShortScalingFactor > 1f;
            float optimalValueBestPath = Mathf.Abs(tooShortScalingFactor - 1);
            if (validBestPath && optimalValueBestPath < bestPathValue)
            {
                bestPathValue = optimalValueBestPath;
                bestPath = firstPath;
                leftovers = result.leftoverText;
        
                // early success
                if (bestPathValue < 0.05)
                {
                    break;
                }
            }
            // recreate path
            firstPath = MeshToPath.ConvertMeshToPath(preScaledMesh, unitSize);
        }
        
        // STEP 3: Get most optimal, slightly too short path
        if (bestPath == null)
        {
            throw new Exception("no appropriate path was generated");
        }
        
        // add straight path to display the leftover letters
        float leftoverLength = alphabet.CalculateTextLength(leftovers, letterScaling.GetRange(text.Length - leftovers.Length - 1, leftovers.Length));
        
        bestPath.AddLineAtEnd(leftoverLength);
        return bestPath;
    }

    public static Mesh ScaleMesh(Mesh mesh, float meshScaling)
    {
        // scale mesh
        Mesh scaledMesh = new Mesh();
        Vector3[] scaledVerts = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vert = mesh.vertices[i];
            scaledVerts[i] = new Vector3(vert.x * meshScaling, vert.y * meshScaling, vert.z * meshScaling);
        }
        scaledMesh.vertices = scaledVerts;
        scaledMesh.triangles = mesh.triangles;
        return scaledMesh;
    }
}
