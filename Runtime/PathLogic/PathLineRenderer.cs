#if (UNITY_EDITOR)


using System;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class PathLineRenderer : MonoBehaviour
{
    // section flags
    public bool showBasics = true;
    public bool showPathProgress = true;
    public bool showInsertSubPath = true;

    // basics section
    public bool showRotationGizmos = false;
    public bool showCurrentPositionGizmo = true;
    public Vector3 upAxisRoughDirection;
    // step to section
    public int stepToJumpTo = 0;
    public float interPointProgress = 0;
    // distance jumps section
    public float moveDistance = 1f;
    public bool destroyWhileMoving;
    // save as so section
    public string basePathToSaveTo;
    public string pathScriptableObjectName;
    // insert sub path
    public PathScriptableObject subPath;

    private void Awake()
    {
        basePathToSaveTo = DirConfiguration.Instance.GetPathScriptableObjectDir();
        pathScriptableObjectName = gameObject.name;
    }


    private PathScriptableObject _pso;

    // connected line renderer
    private LineRenderer _lineRenderer;
    
    public void Reset()
    {
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    public void SetPSO(PathScriptableObject pso)
    {
        _pso = pso;
        upAxisRoughDirection = _pso.LoadPath().GetUp();
    }

    public PathScriptableObject GetPso()
    {
        return _pso;
    }

    public void SaveAsScriptableObject(string path)
    {
        PathScriptableObject pathSo = ScriptableObject.CreateInstance<PathScriptableObject>();

        Path pathCopy = _pso.LoadPath().Copy();
        
        pathSo.points = pathCopy.GetPoints();
        pathSo.pathUp = pathCopy.GetUp().normalized;
        
        AssetDatabase.CreateAsset(pathSo, path);
    }

    public void SyncLineRenderer()
    {
        Point[] points = _pso.LoadPath().GetPoints();
        _lineRenderer.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
            _lineRenderer.SetPosition(i, points[i].pos);
    }
    
    public void OnDrawGizmosSelected()
    {
        Point[] points = _pso.LoadPath().GetPoints();

        void DrawLine(Vector3 start, Vector3 end, Color color, float thickness)
        {
            Handles.DrawBezier(start, end, start, end, color, null, thickness);
        }

        // rotation axis gizmos
        if (showRotationGizmos)
        {
            int startIndex = 0;
            foreach (Tuple<Quaternion, Quaternion> connectionRotation in _pso.LoadPath().GetAllConnectionRotations())
            {
                int endIndex = startIndex + 1;
            
                // rotations
                Quaternion startOutRotation = connectionRotation.Item1;
                Quaternion endInRotation = connectionRotation.Item2;

                // draw start out rotation
                DrawLine(points[startIndex].pos, points[startIndex].pos + (startOutRotation * Vector3.up).normalized * 5,
                    Color.blue, 10f);

                // // draw end in rotation
                DrawLine(points[endIndex].pos, points[endIndex].pos + (endInRotation * Vector3.up).normalized * 5,
                    Color.green, 10f);

                startIndex++;
            }
        }

        if (showCurrentPositionGizmo)
        {
            // draw current virtual position
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_pso.LoadPath().GetVirtualCurrentPoint().pos, 1);
        }
    }

    public static PathLineRenderer CreateFromPath(Path path)
    {
        GameObject myLine = new GameObject("MyPathLineRenderer");
        myLine.transform.position = new Vector3(0,0,0);
        LineRenderer lr = myLine.AddComponent<LineRenderer>();
        lr.material = new Material(Resources.Load<Material>("Materials/LineMaterial"));
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        PathLineRenderer plr = myLine.AddComponent<PathLineRenderer>();
        PathScriptableObject mockPso = ScriptableObject.CreateInstance<PathScriptableObject>();
        mockPso.points = path.GetPoints();
        mockPso.pathUp = path.GetUp();
        mockPso.pathPosition = path.GetPathPosition();
        plr.SetPSO(mockPso);
        plr.Reset();
        plr.SyncLineRenderer();
        return plr;
    }
}
#endif