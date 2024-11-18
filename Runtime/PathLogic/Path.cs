using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;


public class TextInsertionResult
{
    // basic vfx data
    public int textureDimension;
    public List<Vector3> positionsTexture;
    public List<Vector3> linePositionTexture;
    public List<Vector3> rotationsTexture;
    public List<Vector3> lettersTexture;
    public List<Vector3> scalesTexture;
    
    // additional vfx data
    public List<Vector3> colorsTexture;
    
    // left over text that did not fit
    public string leftoverText;
    // was the path used up
    public bool pathWasUsedUp;

    public TextInsertionResult(int textureDimension, List<Vector3> positionsTexture, List<Vector3> linePositionTexture , List<Vector3> rotationsTexture, List<Vector3> lettersTexture, List<Vector3> scalesTexture, string leftoverText, bool pathWasUsedUp)
    {
        this.textureDimension = textureDimension;
        this.positionsTexture = positionsTexture;
        this.linePositionTexture = linePositionTexture;
        this.rotationsTexture = rotationsTexture;
        this.lettersTexture = lettersTexture;
        this.scalesTexture = scalesTexture;
        this.leftoverText = leftoverText;
        this.pathWasUsedUp = pathWasUsedUp;
    }
}

[Serializable]
public class PathPosition
{
    public readonly static float epsilon = 1E-03f;
    public int PointIndex = 0;
    public float InterPointProgress = 0f;

    public PathPosition(int pointIndex, float interPointProgress)
    {
        // round interPoint progress
        if (0 - epsilon < interPointProgress && interPointProgress < 0)
            interPointProgress = 0;
        if (1 < interPointProgress && interPointProgress < 1 + epsilon)
            interPointProgress = 1;
        // check for invalid progress values
        if (interPointProgress > 1f || interPointProgress < 0f)
            throw new ArgumentException($"InterPointProgress must be a value between 0 and 1! But was {interPointProgress}");
        
        this.PointIndex = pointIndex;
        this.InterPointProgress = interPointProgress;
    }

    public void GoNext()
    {
        InterPointProgress = 0f;
        PointIndex++;
    }

    public PathPosition GetNext()
    {
        return new PathPosition(PointIndex + 1, 0f);
    }

    public bool HasNext(Path path)
    {
        if (PointIndex + 1 >= path.GetPoints().Length)
            return false;
        return true;
    }

    public PathPosition GetPrevious()
    {
        if (PointIndex == 0)
            throw new Exception("There is no previous position to 0.");
        return new PathPosition(PointIndex - 1, 0f);
    }

    public Point GetVirtualPoint(Path path)
    {
        Point[] points = path.GetPoints();

        // check if path has enough points
        int biggestIndex = PointIndex;
        if (InterPointProgress != 0f)
            biggestIndex++;
        if (biggestIndex >= points.Length)
            throw new ArgumentException("Path is not long enough, the position is not on the path!");

        // return real point
        if (InterPointProgress == 0f)
            return points[PointIndex];

        // return virtual point
        return Point.Lerp(points[PointIndex], points[PointIndex + 1], InterPointProgress);
    }

   
}

[Serializable]
public class Point
{
    public Vector3 pos;
    public float rotIn;
    public float rotOut;

    public Point(Vector3 pos)
    {
        this.pos = pos;
    }
    
    public Point(Vector3 pos, float rotIn, float rotOut)
    {
        this.pos = pos;
        this.rotIn = rotIn;
        this.rotOut = rotOut;
    }

    public static Point Lerp(Point p1, Point p2, float percentage)
    {
        return new Point(
            Vector3.Lerp(p1.pos, p2.pos, percentage), 
            Mathf.Lerp(p1.rotIn, p2.rotIn, percentage), 
            Mathf.Lerp(p1.rotOut, p2.rotOut, percentage)
            );
    }

    public Point Copy()
    {
        return new Point(this.pos, this.rotIn, this.rotOut);
    }
}

public class Path
{
    // input
    private Point[] _points; // position of each line corner
    private List<int> _holes = new List<int>(); // positions that do not have a connection to the next line
    private Vector3 _pathUp; // direction perpendicular to the startPoint-EndPoint Axis, specifying the up direction of the whole path
    
    // internal
    private PathPosition _currentPathPosition;


    
    public Path(Vector3[] points): this(points, new List<int>(), Enumerable.Repeat(0f, points.Length).ToArray(), Enumerable.Repeat(0f, points.Length).ToArray(), Vector3.up)
    {
    }
    public Path(Vector3[] points, Vector3 pathUp): this(points, new List<int>(), Enumerable.Repeat(0f, points.Length).ToArray(), Enumerable.Repeat(0f, points.Length).ToArray(), pathUp)
    {
    }
    
    public Path(Vector3[] points, List<int> holes): this(points, holes, Enumerable.Repeat(0f, points.Length).ToArray(), Enumerable.Repeat(0f, points.Length).ToArray(), Vector3.up)
    {
    }

    public Path(Point[] points, List<int> holes, Vector3 pathUp)
    {
        
        Debug.Log($"up: {pathUp} other: {points[points.Length - 1].pos - points[0].pos}");
        if (Vector3.Angle(pathUp, points[points.Length - 1].pos - points[0].pos) == 0)
            throw new ArgumentException("pathUp direction can not be parallel to startPoint-endPoint Axis");
        
        _points = points;
        _holes = holes;
        _pathUp = pathUp;
        _currentPathPosition = new PathPosition(0, 0f);
    }

    public Path(Point[] points, Vector3 pathUp)
    {
        Debug.Log($"Pathup: {pathUp}");
        
        if (Vector3.Angle(pathUp, points[points.Length - 1].pos - points[0].pos) == 0)
            throw new ArgumentException("pathUp direction can not be parallel to startPoint-endPoint Axis");
        
        
        _points = points;
        _pathUp = pathUp;
        _currentPathPosition = new PathPosition(0, 0f);
    }
    public Path(Vector3[] points, List<int> holes, float[] rotationsIn, float[] rotationsOut, Vector3 pathUp)
    {
        // validate
        if (points.Length < 2)
            throw new ArgumentException("less then two points do not make a path");
        if (points.Length != rotationsIn.Length || points.Length != rotationsOut.Length)
            throw new ArgumentException("rotation array length did not fit points array length; must be equal");
        if (Vector3.Angle(pathUp, points[points.Length - 1] - points[0]) == 0)
            throw new ArgumentException("pathUp direction can not be parallel to startPoint-endPoint Axis");
        foreach (int hole in holes)
            if (hole >= points.Length)
                throw new ArgumentException($"holes can not exists in points that dont exists {points.Length} {hole}");
        
        
        _points = new Point[points.Length];
        for (int i = 0; i < _points.Length; i++)
            _points[i] = new Point(points[i], rotationsIn[i], rotationsOut[i]);
        _holes = holes;
        _pathUp = pathUp;
        _currentPathPosition = new PathPosition(0, 0f);
    }
    
    public Point[] GetPoints()
    {
        return _points;
    }

    public Vector3 GetUp()
    {
        return _pathUp;
    }

    public void SetUp(Vector3 up)
    {
        // check if up is perpendicular to main axis
        if (0f != Vector3.Dot(up, GetMainAxis()))
            throw new ArgumentException("up direction vector must be orthogonal to main axis of path");
        _pathUp = up;
    }

    public void SetPathPosition(PathPosition pathPosition)
    {
        _currentPathPosition = pathPosition;
    }
    public PathPosition GetPathPosition()
    {
        return _currentPathPosition;
    }
    
    // calculate length by following the path
    public float GetLength()
    {
        float l = 0;
        for (int i = 0; i < _points.Length - 1; i++)
            if (_holes != null && _holes.Contains(i))
                l += Vector3.Distance(_points[i + 1].pos, _points[i].pos);
        return l;
    }

    // calculate length to the end, starting at the current position
    public float GetLengthToEndFromCurrentPosition()
    {
        float l = 0;
        for (int i = _currentPathPosition.PointIndex; i < _points.Length - 1; i++)
            if (_holes.Contains(i))
                l += Vector3.Distance(_points[i + 1].pos, _points[i].pos);
        return l;
    }
    
    // calculate length to the start, starting at the current position
    public float GetLengthToStartFromCurrentPosition()
    {
        float l = 0;
        for (int i = _currentPathPosition.PointIndex; i >= 0; i--)
            if (_holes.Contains(i))
                l += Vector3.Distance(_points[i + 1].pos, _points[i].pos);
        return l;
    }
    
    // virtual current point, that can be in between real points on the path
    public Point GetVirtualCurrentPoint()
    {
        return _currentPathPosition.GetVirtualPoint(this);
    }
    
    // check if the path end was reached already
    public bool IsDone()
    {
        return _currentPathPosition.PointIndex == _points.Length - 1;
    }

    // distance between start end and point (in space, not on the path)
    public float GetStartToEndSpaceDistance()
    {
        return Vector3.Distance(_points[0].pos, _points[_points.Length - 1].pos);
    }

    // direction from first point to last point
    public Vector3 GetMainAxis()
    {
        return (_points[_points.Length - 1].pos - _points[0].pos).normalized;
    }
    
    public Path Copy()
    {
        Point[] points = new Point[_points.Length];
        for(int i=0; i<_points.Length; i++)
            points[i] = _points[i].Copy();
        return new Path(points, _holes, _pathUp);
    }
    
    // rotate all points around a pivot
    public Path Rotate(Quaternion q, Vector3 rotationCenter = new Vector3())
    {
        Path pathCopy = Copy();
        
        // apply rotation to point
        for (int i = 0; i < pathCopy._points.Length; i++)
        {
            Vector3 point = pathCopy._points[i].pos;
            Vector3 dir = point - rotationCenter; // get point direction relative to pivot
            dir = q * dir; // rotate direction
            
            // calculate rotated position, in/out rotations stay the same (relative to up direction, which is rotated); 
            pathCopy._points[i] = new Point(dir + rotationCenter, pathCopy._points[i].rotIn, pathCopy._points[i].rotOut);
        }
        
        // apply rotation to up
        pathCopy._pathUp = q * pathCopy._pathUp;
        return pathCopy;
    }

    // move all points by a vector
    public Path Move(Vector3 vec)
    {
        Path pathCopy = Copy();
        for (int i = 0; i < pathCopy._points.Length; i++)
            pathCopy._points[i] = new Point(pathCopy._points[i].pos + vec, pathCopy._points[i].rotIn, pathCopy._points[i].rotOut);
        return pathCopy;
    }

    // moves all points so, that the first point is at the specified position
    public Path MoveStartTo(Vector3 position)
    {
        return Move(position - _points[0].pos);
    }

    // calculate remaining space that can be filled up, from current point
    public float GetRemainingSpace()
    {
        if (IsDone())
            return 0f;
        
        Point virtualCurrentPoint = GetVirtualCurrentPoint();
        float maxDist = Mathf.NegativeInfinity;
        for (int i = _currentPathPosition.PointIndex; i < _points.Length; i++)
        {
            float currentDist = Vector3.Distance(virtualCurrentPoint.pos, _points[i].pos);
            // TODO what did it do before
            if (maxDist < currentDist)
            {
                maxDist = currentDist;
            }
        }
        return maxDist;
    }


    public PathPosition FindDistancePathPositionFromPathPosition(PathPosition pp, float distance)
    {
        // find first real point with higher distance
        Point startPoint = pp.GetVirtualPoint(this);
        float toPointDist = 0f;
        while (toPointDist < distance)
        {
            pp = pp.GetNext();
            Point distPoint = pp.GetVirtualPoint(this);
            toPointDist = Vector3.Distance(startPoint.pos, distPoint.pos);
        }
        // go one back because while loop overshot
        pp = pp.GetPrevious();

        // axis with the correct point on it
        Point axisStart = pp.GetVirtualPoint(this);
        Point axisEnd = pp.GetNext().GetVirtualPoint(this);

        // distance to axis start
        float axisStartDist = Vector3.Distance(startPoint.pos, axisStart.pos);

        // calc distance to progress
        float alpha = Mathf.Deg2Rad * Vector3.Angle(startPoint.pos - axisStart.pos, axisEnd.pos - axisStart.pos);
        float gamma = Mathf.Asin((Mathf.Sin(alpha) * axisStartDist) /  distance);
        float beta = Mathf.Deg2Rad * (180 - Mathf.Rad2Deg * alpha - Mathf.Rad2Deg * gamma);
        float distanceProgressed;
        float roundedAlpha = Mathf.Round(Mathf.Rad2Deg * alpha * 1000.0f) * 0.001f;

        // special cases, axis is aligned with current position
        if (roundedAlpha == 0f)
            distanceProgressed = distance + axisStartDist;
        else if (Mathf.Approximately(roundedAlpha, 180f))
            distanceProgressed = distance - axisStartDist;
        else
            distanceProgressed = (distance * Mathf.Sin(beta)) / Mathf.Sin(alpha);

        pp.InterPointProgress = distanceProgressed / Vector3.Distance(axisStart.pos, axisEnd.pos);
        return pp;
    }
    
    [CanBeNull]
    public PathPosition FindDistancedPathPosition(float distance)
    {
        // return FindDistancePathPositionFromPathPosition(_currentPathPosition, distance);
        
        // find first real point with higher distance
        Point startPoint = GetVirtualCurrentPoint();
        float toPointDist = 0f;
        PathPosition pp = _currentPathPosition;
        while (toPointDist < distance)
        {
            if (!pp.HasNext(this))
                return null;
            
            pp = pp.GetNext();
            Point distPoint = pp.GetVirtualPoint(this);
            toPointDist = Vector3.Distance(startPoint.pos, distPoint.pos);
        }
        // go one back because while loop overshot
        pp = pp.GetPrevious();

        // axis with the correct point on it
        Point axisStart = pp.GetVirtualPoint(this);
        Point axisEnd = pp.GetNext().GetVirtualPoint(this);

        // distance to axis start
        float axisStartDist = Vector3.Distance(startPoint.pos, axisStart.pos);

        // calc distance to progress
        float alpha = Mathf.Deg2Rad * Vector3.Angle(startPoint.pos - axisStart.pos, axisEnd.pos - axisStart.pos);
        float gamma = Mathf.Asin((Mathf.Sin(alpha) * axisStartDist) /  distance);
        float beta = Mathf.Deg2Rad * (180 - Mathf.Rad2Deg * alpha - Mathf.Rad2Deg * gamma);
        float distanceProgressed;
        float roundedAlpha = Mathf.Round(Mathf.Rad2Deg * alpha * 1000.0f) * 0.001f;

        // special cases, axis is aligned with current position
        if (roundedAlpha == 0f)
            distanceProgressed = distance + axisStartDist;
        else if (Mathf.Approximately(roundedAlpha, 180f))
            distanceProgressed = distance - axisStartDist;
        else
            distanceProgressed = (distance * Mathf.Sin(beta)) / Mathf.Sin(alpha);

        pp.InterPointProgress = distanceProgressed / Vector3.Distance(axisStart.pos, axisEnd.pos);
        return pp;
    }

    
    
    // checks if there is a real point at the current position
    // if not: a real one is created, and the position is moved there
    // returns the index of the newly created point, or of the real point
    public PathPosition CreateRealPointAtPosition(PathPosition pp)
    {
        if (pp.InterPointProgress > 0f)
        {
            List<Point> adjustedPoints = new List<Point>(_points);
            adjustedPoints.Insert(pp.PointIndex + 1, pp.GetVirtualPoint(this));
            _points = adjustedPoints.ToArray(); // write back adjusted points
            
            // adjust holes
            List<int> newHoles = new List<int>();
            foreach (int hole in _holes)
            {
                if (hole <= pp.PointIndex)
                    newHoles.Add(hole);
                else
                    newHoles.Add(hole + 1);
            }
            _holes = newHoles;
            
            
            return new PathPosition(pp.PointIndex + 1, 0f);
        }
        return new PathPosition(pp.PointIndex, pp.InterPointProgress);
    }
    
    // creates real point at the current position and moves the position there
    public void CreateRealPointAtCurrentPosition()
    {
        _currentPathPosition = CreateRealPointAtPosition(_currentPathPosition);
    }
    
    // inserts a path from the current point on
    public bool InsertSubPath(Path originalSubPath)
    {
        Path subPath = originalSubPath.Copy();
        
        // sub path too spacious: no insertion possible
        float distanceSubPath = subPath.GetStartToEndSpaceDistance();
        if (distanceSubPath > GetRemainingSpace())
            return false;

        // check if there is a real starting point currently
        if (_currentPathPosition.InterPointProgress > 0f)
        {
            // if not: create real point at virtual position
            List<Point> adjustedPoints = new List<Point>(_points);
            adjustedPoints.Insert(_currentPathPosition.PointIndex + 1, GetVirtualCurrentPoint());
            _points = adjustedPoints.ToArray(); // write back adjusted points
            _currentPathPosition.GoNext(); // move to the new point
        }

        // get the first virtual point in space with distance of subPath distance
        PathPosition finalPointPP = FindDistancedPathPosition(subPath.GetStartToEndSpaceDistance());
        if (finalPointPP == null)
            return false;
        Point finalPoint = finalPointPP.GetVirtualPoint(this);

        // get old and new facing direction of the sub path
        Vector3 formerPathDirection = subPath.GetMainAxis();
        Vector3 substitutedPathPartAxis = (finalPoint.pos - GetVirtualCurrentPoint().pos).normalized;

        // calculate rotation needed
        Quaternion rot = Quaternion.FromToRotation(formerPathDirection, substitutedPathPartAxis);

        // align subgraph start and end
        subPath = subPath.Rotate(rot);
        subPath = subPath.MoveStartTo(GetVirtualCurrentPoint().pos);

        // get relative up of substituted path part
        Vector3 axisRelativeUp = Quaternion.FromToRotation(GetMainAxis(), substitutedPathPartAxis) * _pathUp;
        // align subPath up with relative up
        subPath = subPath.Rotate(Quaternion.FromToRotation(subPath._pathUp, axisRelativeUp), subPath.GetPoints()[0].pos);
        // rotate subPath around its main axis
        float angle = Mathf.Lerp(finalPoint.rotIn, GetVirtualCurrentPoint().rotOut, 0.5f);
        subPath = subPath.Rotate(Quaternion.AngleAxis(angle, subPath.GetMainAxis()), subPath.GetPoints()[0].pos);
        
        
        // remove all points in original path that are in between current point, and final point
        // current and final point are not removed
        int amountToRemove = finalPointPP.PointIndex - _currentPathPosition.PointIndex;
        List<Point> outPoints = new List<Point>(_points);
        List<int> outHoles = new List<int>(_holes);
        outPoints.RemoveRange(_currentPathPosition.PointIndex + 1, amountToRemove);
        
        /*outHoles = outHoles // adjust hole indices when removing a range
            .FindAll(hole =>
                hole < _currentPathPosition.PointIndex + 1 ||
                _currentPathPosition.PointIndex + 1 + amountToRemove <= hole)
            .Select(hole =>
            {
                int outHole = hole;
                if (_currentPathPosition.PointIndex + 1 + amountToRemove <= hole)
                    outHole += amountToRemove;
                return outHole;
            }).ToList();*/
        
        // insert subPath
        List<Point> pointsToInsert = new List<Point>(subPath.GetPoints());
        // don't insert first point (already present)
        pointsToInsert.RemoveAt(0);
        // substitute end of subPath with final point, only if necessary
        pointsToInsert.RemoveAt(pointsToInsert.Count - 1);
        if (finalPointPP.InterPointProgress > 0f)
            pointsToInsert.Add(finalPoint);
        // insert points
        outPoints.InsertRange(_currentPathPosition.PointIndex + 1, pointsToInsert.ToArray());

        // insert holes
        subPath._holes.ForEach(h =>
        {
            outHoles.Add(h + _currentPathPosition.PointIndex);
        });
        
        // save new points
        _points = outPoints.ToArray();
        _holes = outHoles;

        // go to end of subgraph
        _currentPathPosition = new PathPosition(_currentPathPosition.PointIndex + pointsToInsert.Count, 0f);
        
        return true;
    }

    public bool MoveDistanceOnPath(float distance)
    {
        // if at the end of path, no more moving
        if (_currentPathPosition.PointIndex >= _points.Length - 1)
            return false;
        
        // go along the path until a point is hit, that overshoots
        float currInterPoint = _currentPathPosition.InterPointProgress;
        float previousInterPoint = currInterPoint;
        int currIndex = _currentPathPosition.PointIndex;
        float distTraveled = 0f;
        float distToNextPoint = 0f;
        while (distTraveled < distance)
        {
            // end of path reached
            if (currIndex + 1 >= _points.Length)
                return false;
            
            // travel to next point by inter point progress
            distToNextPoint = (1 - currInterPoint) * Vector3.Distance(_points[currIndex].pos, _points[currIndex + 1].pos);
            distTraveled += distToNextPoint;
            
            // reset inter point progress and increment index
            previousInterPoint = currInterPoint;
            currInterPoint = 0f;
            currIndex++;
        }
        // go back one, to get the point before overshooting 
        currIndex--; 
        distTraveled -= distToNextPoint;
        currInterPoint = previousInterPoint;
        // calc distance left to travel
        float distanceLeftToTravel = distance - distTraveled;

        // get distance left on the current connection
        float connectionDist = Vector3.Distance(_points[currIndex].pos, _points[currIndex + 1].pos);
        float distLeftOnCurrentConnection = (1 - currInterPoint) * connectionDist;

        // the current connection should suffices, only adjust the inter point progress
        if (distLeftOnCurrentConnection > distanceLeftToTravel)
        {
            float newProgress = currInterPoint + distanceLeftToTravel / connectionDist;
            _currentPathPosition = new PathPosition(currIndex, newProgress);
        }
        else
            throw new Exception("distance was not found on the expected connection");
        
        return true;
    }

    // returns the number of points removed
    public int RemovePointsBetween(PathPosition startPathPosition, PathPosition endPathPosition)
    {
        
        
        // filter out all points in between
        List<Point> newPoints = new List<Point>();
        int pointsBefore = _points.Length;
        for (int i = 0; i < _points.Length; i++)
        {
            if (startPathPosition.PointIndex < i && i < endPathPosition.PointIndex)
                continue;
            newPoints.Add(_points[i]);
        }
        _points = newPoints.ToArray();
        int pointsAfter = _points.Length;

        // adjust holes
        List<int> newHoles = new List<int>();
        foreach (int hole in _holes)
        {
            if (hole <= startPathPosition.PointIndex)
                newHoles.Add(hole);
            else
                newHoles.Add(hole - (pointsBefore - pointsAfter));
        }
        _holes = newHoles;
        
        return pointsBefore - pointsAfter;
    }
    
    public bool MoveDistanceInSpace(float distance, bool destroyPathWhileMoving = false)
    {
        if (destroyPathWhileMoving)
        {
            // create real point at current position
            CreateRealPointAtCurrentPosition();

            // find position with distance on path
            PathPosition pp = FindDistancedPathPosition(distance);

            if (pp == null)
                return false;

            // create real point at final position
            PathPosition finalPosition = CreateRealPointAtPosition(pp);

            // remove all points between the new start and new end position
            int pointsRemoved = RemovePointsBetween(_currentPathPosition, finalPosition);
            _currentPathPosition = new PathPosition(finalPosition.PointIndex - pointsRemoved, 0f);
        }
        else
        {
            // find position with distance on path
            PathPosition pp = FindDistancedPathPosition(distance);

            if (pp == null)
                return false;
            
            // set as new position
            _currentPathPosition = pp;
        }
        
        return true;
    }
    
    public Tuple<Quaternion, Quaternion> GetRotationsForConnection(int pointIndex1, int pointIndex2)
    {
        Point startPoint = _points[pointIndex1];
        Point endPoint = _points[pointIndex2];

        // get local up direction vector by adjusting by rotation from main axis to connection axis
        Vector3 connectionAxis = endPoint.pos - startPoint.pos;
        Vector3 localUp = Quaternion.FromToRotation(GetMainAxis(), connectionAxis) * _pathUp;
        
        // rotate local up around the connection axis by the specified angles, to get the facing direction og the point
        Vector3 facingDirectionStart = Quaternion.AngleAxis(startPoint.rotOut, connectionAxis) * localUp;
        Vector3 facingDirectionEnd = Quaternion.AngleAxis(endPoint.rotIn, connectionAxis) * localUp;

        // create rotations using facing direction and up direction (connection axis)
        Quaternion startPointRotation = Quaternion.LookRotation(connectionAxis, facingDirectionStart);
        Quaternion endPointRotation = Quaternion.LookRotation(connectionAxis, facingDirectionEnd);

        return new Tuple<Quaternion, Quaternion>(startPointRotation, endPointRotation);
    }

    public List<Tuple<Quaternion, Quaternion>> GetAllConnectionRotations()
    {
        List<Tuple<Quaternion, Quaternion>> outConnectionRotations = new List<Tuple<Quaternion, Quaternion>>();
        
        // create rotations of all connections
        for (int i = 0; i < _points.Length - 1; i++)
        {
            int j = i + 1;
            outConnectionRotations.Add(GetRotationsForConnection(i, j));
        }
        return outConnectionRotations;
    }

    // returns the position and rotation of each connection on the path,
    // rotations are averaged between start and end rotation
    public List<Tuple<Vector3, Quaternion>> PositionRotationFormat(Quaternion preOffset=new Quaternion(), bool filterWithHoles = false)
    {
        List<Tuple<Vector3, Quaternion>> outPoints = new List<Tuple<Vector3, Quaternion>>();
        List<Tuple<Quaternion, Quaternion>> connectionRotations = GetAllConnectionRotations();
        
        // append first point of connection and averaged rotation
        for (int i = 0; i < connectionRotations.Count; i++)
        {
            Vector3 pointPos = _points[i].pos;
            Tuple<Quaternion, Quaternion> connectionRotation = connectionRotations[i];
            Quaternion averageRotation = Quaternion.Lerp(connectionRotation.Item1, connectionRotation.Item2, 0.5f);
            
            // apply offset to rotation
            Quaternion outRotation = averageRotation * preOffset;

            if (filterWithHoles && (_holes.Contains(i) || i >= connectionRotations.Count - 1) && (_holes.Contains(i - 1) || i - 1 < 0))
            {
                continue;
            }
            
            outPoints.Add(new Tuple<Vector3, Quaternion>(pointPos, outRotation));
        }
        
        return outPoints;
    }

    public bool PathPositionsWithHoleInBetween(PathPosition from, PathPosition to)
    {
        // validate correct sequence
        if (from.PointIndex > to.PointIndex ||
            (from.PointIndex == to.PointIndex && from.InterPointProgress > to.InterPointProgress))
        {
            Debug.Log($"from: {from.PointIndex}; {from.InterPointProgress} to {to.PointIndex}; {to.InterPointProgress}");
            throw new ArgumentException("from path position must be before to path position");
        }

        // check if from or to are in a hole
        if (from.InterPointProgress > 0f && _holes.Contains(from.PointIndex))
            return true;
        if (to.InterPointProgress > 0f && _holes.Contains(to.PointIndex))
            return true;
        // on the same index: no hole
        if (from.PointIndex == to.PointIndex || (to.PointIndex - from.PointIndex == 1))
            return false;
        // check if a hole is between them, if they are not on the same index
        for (int i=from.PointIndex + 1; i<to.PointIndex; i++)
            if (_holes.Contains(i))
                return true;
        // nothing found, no holes
        return false;
    }

    public bool TakeValidPositionForLetterOfWidth(float letterWidth)
    {
        // check the letter would violate a hole restriction with its position
        PathPosition currentPP = new PathPosition(_currentPathPosition.PointIndex, _currentPathPosition.InterPointProgress);
        PathPosition nextPP = FindDistancedPathPosition(letterWidth);
        if (nextPP == null)
        {
            return false;
        }
        bool foundPointAfterHole = true;
        while (PathPositionsWithHoleInBetween(currentPP, nextPP))
        {
            // if violation happens: push letter to next segment (that is not a hole), and check again
            foundPointAfterHole = false;
            foreach (int hole in _holes)
            {
                if (hole >= currentPP.PointIndex)
                {
                    currentPP.PointIndex = hole + 1;
                    while (_holes.Contains(currentPP.PointIndex))
                        currentPP.PointIndex += 1;
                    currentPP.InterPointProgress = 0f;

                    SetPathPosition(currentPP);
                            
                    nextPP = FindDistancedPathPosition(letterWidth);
                    foundPointAfterHole = true;
                    break;
                }
            }
            if (!foundPointAfterHole)
                break;
        }
        return foundPointAfterHole;
    }
    
    public TextInsertionResult ConvertToPointData(string text, AlphabethScriptableObject alphabet, List<float> scaleData, bool splitWords=true)
    {
        //_holes.Sort();
        
        // prepare text
        string textWithoutSpace = text.Replace(" ", "");
        string[] words = text.Split(" ");
        
        // get size of textures
        int textureSize = Mathf.CeilToInt(Mathf.Sqrt(textWithoutSpace.Length));
        
        // create cache for letter widths
        Dictionary<char, float> widthCache = alphabet.GetWidthDictionary();

        // enter letters by width, count inserted characters + spaces
        int insertedCharacters = 0;
        int insertedNonSpaceCharacters = 0;
        for (int wordsIndex = 0; wordsIndex < words.Length; wordsIndex++)
        {
            string word = words[wordsIndex];
            
            // check if word is too long for current line
            for (int letterIndex = 0; letterIndex < word.Length - 1; letterIndex++)
            {
                char currentLetter = word[letterIndex];
                float letterWidth = widthCache[currentLetter] * scaleData[insertedNonSpaceCharacters];
                
                // add new point
                if (TakeValidPositionForLetterOfWidth(letterWidth))
                {
                    if (!MoveDistanceInSpace(letterWidth, true))
                    {
                        break;
                    }
                    insertedCharacters++;
                    insertedNonSpaceCharacters++;
                    
                    Debug.Log($"Inserted: {currentLetter} insertedChars: {insertedCharacters} insertedNonSpaceChars: {insertedNonSpaceCharacters}");
                }
            }
            // final letter
            Debug.Log($"lengths: {widthCache.Count} {word.Length} {scaleData.Count}");
            
            float lastLetterWidth = (widthCache[word[word.Length - 1]] + alphabet.spaceWidth) * scaleData[insertedNonSpaceCharacters];
            if (TakeValidPositionForLetterOfWidth(lastLetterWidth))
            {
                if (!MoveDistanceInSpace(lastLetterWidth, true))
                {
                    break;
                }
                insertedCharacters++;
                insertedCharacters++;
                insertedNonSpaceCharacters++;
                Debug.Log($"Inserted with space: {word[word.Length - 1]} insertedChars: {insertedCharacters} insertedNonSpaceChars: {insertedNonSpaceCharacters}");

            }
        }

        
        // // enter letters by width, count inserted characters + spaces
        // int insertedCharacters = 0;
        // int insertedNonSpaceCharacters = 0;
        // for (int i = 0; i < words.Length; i++)
        // {
        //     string word = words[i];
        //     
        //     // move according to the width of the inserted letter, leave out last letter
        //     for (int j = 0; j < word.Length - 1; j++)
        //     {
        //         char currentLetter = word[j];
        //         if (!MoveDistanceInSpace(widthCache[currentLetter] * scaleData[insertedNonSpaceCharacters], true))
        //             break; // TODO REMOVE FOLLOWING PATH
        //         insertedCharacters++;
        //         insertedNonSpaceCharacters++;
        //     }
        //     // final letter has additional width of trailing space
        //     float lastLetterWidth = (widthCache[word[word.Length - 1]] + alphabet.spaceWidth) * scaleData[insertedNonSpaceCharacters];
        //     if (!MoveDistanceInSpace(lastLetterWidth, true))
        //         break; // TODO REMOVE FOLLOWING PATH
        //     insertedCharacters++;
        //     insertedCharacters++;
        //     insertedNonSpaceCharacters++;
        // }
        
        // after entering letters: see whats left
        string leftoverText = text.Substring(insertedCharacters - 1, text.Length - (insertedCharacters - 1));
        
        // get position and rotation information
        List<Tuple<Vector3, Quaternion>> positionRotationInformation = PositionRotationFormat(Quaternion.FromToRotation(new Vector3(0,0,1), new Vector3(1,0,0)), true);

        // color the textures of vfx
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> linePositions = new List<Vector3>();
        List<Vector3> rotations = new List<Vector3>();
        List<Vector3> letters = new List<Vector3>();
        List<Vector3> scales = new List<Vector3>();
        
        // total line position is split into three variables, to avoid floating point imprecision
        int splittingFactor = 100;
        int totalLinePositionTenThousands = 0;
        int totalLinePositionHundrets = 0;
        float totalLinePositionDelta = 0f;

        Vector3 prevPosition = positionRotationInformation.ElementAt(0).Item1;
        for (int i = 0; i < positionRotationInformation.Count && i < textWithoutSpace.Length; i++)
        {
            Vector3 position = positionRotationInformation.ElementAt(i).Item1;
            Vector3 rotation = positionRotationInformation.ElementAt(i).Item2.eulerAngles;
            Vector3 scale = new Vector3(scaleData[i], 0, 0);

            float positionDelta = Vector3.Distance(prevPosition, position);
            
            // increment total line position
            totalLinePositionDelta += positionDelta;
            totalLinePositionHundrets += Mathf.FloorToInt(totalLinePositionDelta / splittingFactor);
            totalLinePositionTenThousands += totalLinePositionHundrets / splittingFactor;
            totalLinePositionDelta %= splittingFactor;
            totalLinePositionHundrets %= splittingFactor;
            totalLinePositionTenThousands %= splittingFactor;
            
            // encode total line position to color
            Vector3 linePosition = new Vector3(totalLinePositionTenThousands, totalLinePositionHundrets, totalLinePositionDelta);
            
            int letterInt = TextUtil.CharToInt(textWithoutSpace[i]);
            int letterIndex = letterInt % 4;
            int letterGroup = letterInt / 4;
            Vector3 letterVector = new Vector3(letterIndex, letterGroup);
            
            positions.Add(position);
            linePositions.Add(linePosition);
            rotations.Add(rotation);
            letters.Add(letterVector);
            scales.Add(scale);
            
            // update prev position for next run
            prevPosition = position;
        }

        int textureDimension = Mathf.CeilToInt(Mathf.Sqrt(positions.Count));

        return new TextInsertionResult(textureDimension, positions, linePositions, rotations, letters, scales, leftoverText, false);
    }

    public void SubstituteConnection(int connection, Path originalSubPath, float? overrideAngle=null)
    {
        Path subPath = originalSubPath.Copy();
        
        // pick connection start / end
        int connectionStart = connection;
        int connectionEnd = connection + 1;
        
        // check if sub path has right length
        float connectionLength = Vector3.Distance(GetPoints()[connectionStart].pos, GetPoints()[connectionEnd].pos);
        if (Mathf.Approximately(subPath.GetStartToEndSpaceDistance(), connectionLength))
        {
            float factor = connectionLength / subPath.GetStartToEndSpaceDistance();
            subPath = subPath.Scale(factor);
        }

        // get old and new facing direction of the sub path
        Vector3 formerPathDirection = subPath.GetMainAxis();
        Vector3 substitutedPathPartAxis = (GetPoints()[connectionEnd].pos - GetPoints()[connectionStart].pos).normalized;

        // calculate rotation needed
        Quaternion rot = Quaternion.FromToRotation(formerPathDirection, substitutedPathPartAxis);

        // align subgraph start and end
        subPath = subPath.Rotate(rot);
        subPath = subPath.MoveStartTo(GetPoints()[connectionStart].pos);

        // get relative up of substituted path part
        Vector3 axisRelativeUp = Quaternion.FromToRotation(GetMainAxis(), substitutedPathPartAxis) * _pathUp;
        // align subPath up with relative up
        subPath = subPath.Rotate(Quaternion.FromToRotation(subPath._pathUp, axisRelativeUp), subPath.GetPoints()[0].pos);
        // rotate subPath around its main axis
        float angle;
        if (overrideAngle == null)
            angle = Mathf.Lerp(GetPoints()[connectionEnd].rotIn, GetPoints()[connectionStart].rotOut, 0.5f);
        else
            angle = overrideAngle.Value;    
        subPath = subPath.Rotate(Quaternion.AngleAxis(angle, subPath.GetMainAxis()), subPath.GetPoints()[0].pos);

        // insert subPath, dont use first and last point of subPath
        List<Point> outPoints = new List<Point>(_points);
        List<Point> pointsToInsert = new List<Point>(subPath.GetPoints());
        pointsToInsert.RemoveAt(0);
        pointsToInsert.RemoveAt(pointsToInsert.Count - 1);
        outPoints.InsertRange(connectionStart + 1, pointsToInsert.ToArray());

        // write back new points
        _points = outPoints.ToArray();
    }

    public Path Scale(float factor)
    {
        Path path = Copy();
        path._points = path._points.Select(p => new Point(p.pos * factor, p.rotIn, p.rotOut)).ToArray();
        return path;
    }

    public Vector3 GetZeroAxis(int connStart, int connEnd)
    {
        Tuple<Quaternion, Quaternion> rotations = GetRotationsForConnection(connStart, connEnd);
        Quaternion rotation = Quaternion.Lerp(rotations.Item1, rotations.Item2, 0.5f);

        return (rotation * Vector3.up).normalized;
    }

    public void AddLineAtEnd(float distance)
    {
        List<Point> newPoints = new List<Point>(_points);
        // create new last point in the direction of the main axis
        Vector3 mainAxisDirection = GetMainAxis().normalized;
        Point oldLastPoint = _points[_points.Length - 1];
        Vector3 newLastPos = oldLastPoint.pos + distance * mainAxisDirection;
        newPoints.Add(new Point(newLastPos, oldLastPoint.rotIn, oldLastPoint.rotOut));
        // write back new points
        _points = newPoints.ToArray();
    }
}
