using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Raster
{
    private HashSet<Vector3Int> _points;
    private Vector3 _origin;
    private float _rasterUnit;
    private Dictionary<Vector3Int, bool> _markingDictionary;

    public Raster(HashSet<Vector3Int> points, Vector3 origin, float rasterUnit)
    {
        _points = points;
        _origin = origin;
        _rasterUnit = rasterUnit;
        _markingDictionary = new Dictionary<Vector3Int, bool>();
    }
    
    public Vector3 RasterPosToRealPos(Vector3Int rasterPos)
    {
        Vector3 realMovement = ((Vector3) rasterPos) * _rasterUnit;
        return _origin + realMovement;
    }

    public Vector3Int RealPosToRasterPos(Vector3 realPos)
    {
        Vector3 rasterPosNotRounded = (realPos - _origin) / _rasterUnit;
        return Vector3Int.RoundToInt(rasterPosNotRounded);
    }

    public HashSet<Vector3> GetRealPoints()
    {
        return _points.Select(point => RasterPosToRealPos(point)).ToHashSet();
    }

    public Vector3Int GetRasterPointClosestToRealPosition(Vector3 position)
    {
        Vector3Int rasterPos = RealPosToRasterPos(position);
        // check if raster pos is in raster
        if (_points.Contains(rasterPos))
            return rasterPos;
        // otherwise check for closest
        float minDist = Mathf.Infinity;
        Vector3Int? minDistPosition = null;
        foreach (Vector3Int point in _points)
        {
            if (Vector3Int.Distance(point, rasterPos) < minDist)
            {
                minDist = Vector3Int.Distance(point, rasterPos);
                minDistPosition = point;
            }
        }
        if (minDistPosition == null)
            throw new Exception("Raster may not contain any points.");
        return minDistPosition.Value;
    }

    public Vector3Int GetLowestPosition()
    {
        float low = Mathf.Infinity;
        Vector3Int? lowPosition = null;
        foreach (Vector3Int point in _points)
        {
            if (point.y < low)
            {
                low = point.y;
                lowPosition = point;
            }
        }
        if (lowPosition == null)
            throw new Exception("Raster may not contain any points.");
        return lowPosition.Value;
    }
    
    public Vector3Int GetHighestPosition()
    {
        float high = Mathf.NegativeInfinity;
        Vector3Int? highPosition = null;
        foreach (Vector3Int point in _points)
        {
            if (point.y > high)
            {
                high = point.y;
                highPosition = point;
            }
        }
        if (highPosition == null)
            throw new Exception("Raster may not contain any points.");
        return highPosition.Value;
    }

    public void Mark(Vector3Int position)
    {
        _markingDictionary.Add(position, true);
    }

    public bool IsMarked(Vector3Int position)
    {
        if (!_markingDictionary.ContainsKey(position))
            return false;
        return _markingDictionary.GetValueOrDefault(position);
    }

    public HashSet<Vector3Int> GetNeighbors(Vector3Int position, bool onlyUnmarked=false)
    {
        HashSet<Vector3Int> neighbors = new HashSet<Vector3Int>();
        foreach (Vector3Int dir in Rasterizer.GetBasicDirections())
        {
            Vector3Int neighbor = position + dir;
            if (onlyUnmarked)
            {
                if (_points.Contains(neighbor) && !_markingDictionary.ContainsKey(neighbor))
                    neighbors.Add(neighbor);
            }
            else
            {
                if (_points.Contains(neighbor))
                    neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    public HashSet<Vector3Int> GetDirectionsToNeighbors(Vector3Int position, bool onlyUnmarked=false)
    {
        return GetNeighbors(position, onlyUnmarked).Select(neighbor =>
        {
            return neighbor - position;
        }).ToHashSet();
    }

    public Dictionary<Vector3Int, bool> GetMarking()
    {
        return _markingDictionary;
    }

}


public class Rasterizer : MonoBehaviour
{ 
    
    // RayTriangleIntersection and IsPointInsideMesh copied from: https://discussions.unity.com/t/point-inside-mesh/220234/6
    // removed transform from Solution 1; Credit to: Przemyslaw_Zaworski
    static bool RayTriangleIntersection (Vector3 ro, Vector3 rd, Vector3 a, Vector3 b, Vector3 c, out Vector3 hit)
    {
        float epsilon = 0.0000001f;
        hit = new Vector3(0f, 0f, 0f);
        Vector3 ba = b - a;
        Vector3 ca = c - a;
        Vector3 h = Vector3.Cross(rd, ca);
        float det = Vector3.Dot(ba, h);
        if (det > -epsilon && det < epsilon) return false;
        float f = 1.0f / det;
        Vector3 s = ro - a;
        float u = Vector3.Dot(s, h) * f;
        if (u < 0.0f || u > 1.0f) return false;
        Vector3 q = Vector3.Cross(s, ba);
        float v = Vector3.Dot(rd, q) * f;
        if (v < 0.0f || u + v > 1.0f) return false;
        float t = Vector3.Dot(ca, q) * f;
        hit = ro + rd * t;
        return (t > epsilon);
    }
    
    // First method:
    // Create a ray (infinite line starting at input point and going in some random direction).
    // Find intersections between ray and all mesh triangles. An odd number of intersections means it is inside the mesh.
    // position = input point in world space
    // vertices = mesh vertices
    // triangles = mesh triangles (indices)
    static bool IsPointInsideMesh (Vector3 position, Vector3[] vertices, int[] triangles)
    {
        Vector3 epsilon = new Vector3(0.001f, 0.001f, 0.001f);
        Vector3 direction = Vector3.Normalize(UnityEngine.Random.insideUnitSphere + epsilon);
        int intersections = 0;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = vertices[triangles[i + 0]];
            Vector3 b = vertices[triangles[i + 1]];
            Vector3 c = vertices[triangles[i + 2]];
            intersections += RayTriangleIntersection(position, direction, a, b, c, out Vector3 hit) ? 1 : 0;
        }
        return (intersections % 2 == 1);
    }
    
    public static List<Vector3Int> GetBasicDirections()
    {
        List<Vector3Int> dirs = new List<Vector3Int>();
        dirs.Add(Vector3Int.up);
        dirs.Add(Vector3Int.down);
        dirs.Add(Vector3Int.left);
        dirs.Add(Vector3Int.right);
        dirs.Add(Vector3Int.back);
        dirs.Add(Vector3Int.forward);
        return dirs;
    }

    public static Raster RasterizeMesh(Mesh mesh, float rasterUnit=1f)
    {
        // raster to put points on
        Dictionary<Vector3Int, bool> raster = new Dictionary<Vector3Int, bool>();
        // get center point inside of mesh, will be treated as (0, 0, 0) or raster 
        Vector3 centerPoint = mesh.bounds.center;
        // directions to move in
        List<Vector3Int> moveDirections = GetBasicDirections();

        // function to convert raster position to real position
        Vector3 RasterPosToRealPos(Vector3Int rasterPos)
        {
            Vector3 realMovement = ((Vector3) rasterPos) * rasterUnit;
            return centerPoint + realMovement;
        }

        // raster positions that are unexplored, and the updated ones
        List<Vector3Int> unexploredRasterPositions = new List<Vector3Int>();
        List<Vector3Int> newUnexploredRasterPositions = new List<Vector3Int>();
        unexploredRasterPositions.Add(new Vector3Int(0, 0,0)); // add center as first unexplored raster position
        raster.Add(new Vector3Int(0,0,0), true);
        // while there are raster positions unexplored
        while (unexploredRasterPositions.Count > 0)
        {
            // iterate all unexplored points
            foreach (Vector3Int unexploredRasterPosition in unexploredRasterPositions)
            {
                // iterate possible directions to explore
                foreach (Vector3Int dir in moveDirections)
                {
                    // possibly new raster position
                    Vector3Int newRasterPosition = unexploredRasterPosition + dir;
                
                    // do not explore positions already on the raster
                    if (raster.ContainsKey(newRasterPosition))
                        continue;
                    
                    // check if real position is inside the mesh
                    Vector3 realPosition = RasterPosToRealPos(newRasterPosition);
                    // if so: point must be explored further, is added to raster as positive
                    if (IsPointInsideMesh(realPosition, mesh.vertices, mesh.triangles))
                    {
                        newUnexploredRasterPositions.Add(newRasterPosition);
                        raster.Add(newRasterPosition, true);
                    }
                    // if not so: point must not be explored further, is marked as negative in raster
                    else
                        raster.Add(newRasterPosition, false);
                }
            }
            unexploredRasterPositions = new List<Vector3Int>(newUnexploredRasterPositions);
            newUnexploredRasterPositions = new List<Vector3Int>();
        }

        // return all positively marked raster positions, converted to real world coords
        raster = raster.Where(point => point.Value).ToDictionary(v => v.Key, v => v.Value);
        return new Raster(raster.Select(pair => pair.Key).ToHashSet(), centerPoint, rasterUnit);
    }
}
