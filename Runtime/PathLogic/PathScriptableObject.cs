using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathScriptableObject : ScriptableObject
{
    public Point[] points;
    public List<int> holes;
    public Vector3 pathUp;
    [SerializeField]
    public PathPosition pathPosition;

    private Path path = null;
    
    public Path LoadPath()
    {
        if (path == null)
        {
            path = new Path(points, holes, pathUp).Copy();
            path.SetPathPosition(pathPosition);
        }
        
        return path;
    }

    public void ReloadPath()
    {
        path = new Path(points, holes, pathUp).Copy();
        path.SetPathPosition(pathPosition);
    }

    public void LoadToScene()
    {
        Path path = LoadPath();
        
        // create new go
        GameObject pathObjParent = new GameObject(this.name);
        
        // add LineRenderer
        LineRenderer lr = pathObjParent.AddComponent<LineRenderer>();
        
        // get line positions
        Vector3[] positions = new Vector3[points.Length];
        for (int i = 0; i < positions.Length; i++)
            positions[i] = path.GetPoints()[i].pos;

        // fill line renderer
        lr.positionCount = positions.Length;
        lr.SetPositions(positions);
        lr.material = Resources.Load("Materials/LineMaterial", typeof(Material)) as Material;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        
        #if (UNITY_EDITOR)
        // add PathLineRenderer
        PathLineRenderer plr = pathObjParent.AddComponent<PathLineRenderer>();
        plr.upAxisRoughDirection = pathUp;
        plr.SetPSO(this);
        #endif
    }
}
