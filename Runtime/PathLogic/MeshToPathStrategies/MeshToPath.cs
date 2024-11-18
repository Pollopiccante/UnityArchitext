using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshToPath
{
    
    public static Path ConvertMeshToPath(Mesh mesh, float unitSize)
    {
        // create basic hilbert curve path
        Point[] points = new Point[4];
        points[0] = new Point(new Vector3(0, 0, 0), 0, 0);
        points[1] = new Point(new Vector3(0, unitSize, 0), 0, 0);
        points[2] = new Point(new Vector3(unitSize, unitSize, 0), 0, 0);
        points[3] = new Point(new Vector3(unitSize, 0, 0), 0, 0);
        Path basicHilbertCurve = new Path(points, new Vector3(0, 1, 0));
        
        // rasterize mesh
        Raster meshRaster = Rasterizer.RasterizeMesh(mesh, unitSize);
        
        // get lowest and highest point of raster as start points
        Vector3Int highestPos = meshRaster.GetHighestPosition();
        Vector3Int lowestPos = meshRaster.GetLowestPosition();
        
        // create path from lowest to highest position, also mark all points in the raster contained in this path
        // TODO: use A*
        List<Vector3> pathPositions = new List<Vector3>();
        Vector3Int currentPos = lowestPos;
        while (currentPos != highestPos)
        {
            // mark and add to path
            meshRaster.Mark(currentPos);
            pathPositions.Add(meshRaster.RasterPosToRealPos(currentPos));
            // calculate axis with furthest distance
            Vector3Int movement = highestPos - currentPos;
            int maxMovement = Math.Max(Math.Max(Math.Abs(movement.x), Math.Abs(movement.y)), Math.Abs(movement.z));
            if (maxMovement == movement.x)
            {
                currentPos.x += Math.Sign(movement.x);
            }
            if (maxMovement == movement.y)
            {
                currentPos.y += Math.Sign(movement.y);
            }
            if (maxMovement == movement.z)
            {
                currentPos.z += Math.Sign(movement.z);
            }
        }            
        // mark and add final point to path
        pathPositions.Add(meshRaster.RasterPosToRealPos(highestPos));
        meshRaster.Mark(highestPos);

        // create path
        Path path = new Path(pathPositions.ToArray(), new Vector3(1, 0, 0));

        // while the curve has space to spread, spread it
        bool curveSpread = true;
        while (curveSpread)
        {
            curveSpread = false;
            
            // iterate all connections from last to first
            Point[] pathPoints = path.GetPoints();
            for (int i = pathPoints.Length - 1; i >= 1; i--)
            {
                int iPrev = i - 1;
                Vector3Int connStart = meshRaster.RealPosToRasterPos(pathPoints[iPrev].pos);
                Vector3Int connEnd = meshRaster.RealPosToRasterPos(pathPoints[i].pos);
        
                // get shared free directions to spread to
                HashSet<Vector3Int> commonFreeDirs = meshRaster.GetDirectionsToNeighbors(connStart, true);
                commonFreeDirs.IntersectWith(meshRaster.GetDirectionsToNeighbors(connEnd, true));
        
                if (commonFreeDirs.Count > 0)
                {
                    curveSpread = true;
                    
                    // calculate angle
                    Vector3Int dir = commonFreeDirs.First();
                    Vector3 zeroVector = path.GetZeroAxis(iPrev, i);
                    float angle = Vector3.SignedAngle(zeroVector, dir, (connEnd - connStart));
                    angle = Mathf.Round(angle / 90) * 90;
                    
                    // insert sub path
                    path.SubstituteConnection(iPrev, basicHilbertCurve, angle);
                
                    // mark used points in mesh raster
                    meshRaster.Mark(connStart + dir);
                    meshRaster.Mark(connEnd + dir);
                }
            }
        }

        return path;
    }
}
