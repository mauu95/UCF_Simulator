using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static Robot;

public class RobotLogic:MonoBehaviour
{
    class LambdaSortAngleString : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            Tuple<string, Robot> a = (Tuple<string, Robot>)x;
            Tuple<string, Robot> b = (Tuple<string, Robot>)y;
            return a.Item1.CompareTo(b.Item1);
        }
    }

    public static void SortByOrientation(Robot[] robots, Vector2 xAxis, Orientation orientation)
    {
        Array.Sort(robots, new LambdaCircleRobotOrder(xAxis,orientation));
    }

    public static Robot[] SortedByOrientation(Robot[] robots, Vector2 xAxis)
    {
        Robot[] result = new Robot[robots.Length];
        for (int i = 0; i < robots.Length; i++)
            result[i] = robots[i];

        Array.Sort(result, new LambdaCircleRobotOrder(xAxis, Orientation.COUNTERWISE));
        return result;
    }

    public static Robot[] SortedByOrientation(Robot[] robots, Vector2 xAxis, Orientation orientation)
    {
        Robot[] result = new Robot[robots.Length];
        for (int i = 0; i < robots.Length; i++)
            result[i] = robots[i];

        Array.Sort(result, new LambdaCircleRobotOrder(xAxis, orientation));
        return result;
    }
    public static Robot[] SortedCounterWise(Robot[] robots)
    {
        Robot[] temp = SortedByOrientation(robots, Vector2.left, Orientation.COUNTERWISE);
        Vector2 xAxis = MiddlePoint(temp[0].transform.position, temp[temp.Length-1].transform.position);
        return SortedByOrientation(robots, xAxis, Orientation.COUNTERWISE);
    }

    class LambdaCircleRobotOrder : IComparer<Robot>
    {
        Vector2 xAxis;
        Orientation orientation;
        public LambdaCircleRobotOrder(Vector2 xAxis, Orientation orientation)
        {
            this.xAxis = xAxis;
            this.orientation = orientation;
        }

        public int Compare(Robot x, Robot y)
        {
            float angle1 = Angle(xAxis, x.transform.position);
            float angle2 = Angle(xAxis, y.transform.position);
            float angle = angle1 - angle2;

            int result;
            if (angle > 0)
                result = 1;
            else if (angle < 0)
                result = -1;
            else
                result = 0;

            if (orientation == Orientation.COUNTERWISE)
                return result;
            else
                return -result;

        }
    }

    class LambdaYVirtualAbsValue : IComparer
    {
        Vector2 xAxis;
        public LambdaYVirtualAbsValue(Vector2 xAxis)
        {
            this.xAxis = xAxis;
        }

        public int Compare(object x, object y)
        {
            Robot a = (Robot)x;
            Robot b = (Robot)y;
            float y1 = Mathf.Abs(ToVirtualPoint(xAxis, a.transform.position).y);
            float y2 = Mathf.Abs(ToVirtualPoint(xAxis, b.transform.position).y);
            return Mathf.RoundToInt(y1 - y2);
        }
    }

    class LambdaXVirtualValue : IComparer<Robot>
    {
        Vector2 xAxis;
        public LambdaXVirtualValue(Vector2 xAxis)
        {
            this.xAxis = xAxis;
        }

        public int Compare(Robot a, Robot b)
        {
            float x1 = ToVirtualPoint(xAxis, a.transform.position).x;
            float x2 = ToVirtualPoint(xAxis, b.transform.position).x;

            if (x1 - x2 > 0)
                return 1;
            else if (x1 - x2 < 0)
                return -1;
            else return 0;
        }
    }

    class LambdaClosestRobot : IComparer<Robot>
    {
        Vector2 point;
        public LambdaClosestRobot(Vector2 point)
        {
            this.point = point;
        }

        public int Compare(Robot x, Robot y)
        {
            float d1 = Vector3.Distance(x.transform.position, point);
            float d2 = Vector3.Distance(y.transform.position, point);
            return Mathf.RoundToInt(d1 - d2);
        }
    }

    class LambdaClosestPoint : IComparer<Vector2>
    {
        Vector2 point;
        public LambdaClosestPoint(Vector2 point)
        {
            this.point = point;
        }

        public int Compare(Vector2 x, Vector2 y)
        {
            float d1 = Vector3.Distance(x, point);
            float d2 = Vector3.Distance(y, point);
            return Mathf.RoundToInt(d1 - d2);
        }
    }

    internal static string MinAngleString(Robot[] robots, Snapshot snapshot)
    {
        string result = null;

        foreach(Robot r in robots)
        {
            string minAg = new AngleString(r, snapshot).Min();
            if (result == null || minAg.CompareTo(result) < 1)
                result = minAg;
        }
        return result;
    }

    internal static float BaseAngle(Snapshot snapshot)
    {
        return 360f / snapshot.Size();
    }

    internal static Robot[] FilterDuplicateXvalues(Robot[] robots, Vector2 xAxis)
    {
        List<Robot> result = new List<Robot>();
        for(int i=0; i < robots.Length-1; i++)
        {
            if (HasSameXVirtualValue(robots[i], robots[i + 1], xAxis))
                i++;
            result.Add(robots[i]);
        }

        Robot last = robots[robots.Length - 1];
        Robot secondLast = robots[robots.Length - 2];

        if (!HasSameXVirtualValue(last, secondLast, xAxis))
            result.Add(last);

        return result.ToArray();
    }

    internal static Robot[] SortedByAngleString(Robot[] sector, Snapshot snapshot)
    {
        Tuple<string, Robot>[] a = GetAngleStringSequence(snapshot);

        List<Robot> result = new List<Robot>();

        for(int i = 0; i < a.Length; i++)
        {
            Robot r = a[i].Item2;
            if (sector.Contains(r) && !result.Contains(r))
                result.Add(r);
        }

        return result.ToArray();
    }

    internal static Robot[][] GetSectors(Snapshot snapshot)
    {
        Robot[] robots = snapshot.GetRobotsList();
        SortByOrientation(robots, robots[0].transform.position, Orientation.CLOCKWISE);
        List<Vector2> edges = new List<Vector2>();

        List<Robot> temp = robots.ToList();
        temp.Add(robots[0]);
        Robot[] tailed = temp.ToArray();

        for (int i = 0; i < tailed.Length - 1; i++)
        {
            Robot curr = tailed[i];
            Robot next = tailed[i + 1];
            Vector2 edge = MiddlePoint(curr.transform.position, next.transform.position);

            if (IsSymmetric1Axis0Pivot(edge, snapshot))
                edges.Add(edge);
        }

        Vector2 xAxis = edges[0]; //It doesnt matter which one
        float[] edgesAngles = new float[edges.Count + 1];
        edgesAngles[edgesAngles.Length - 1] = 360;

        for (int i = 0; i < edges.Count; i++)
            edgesAngles[i] = Angle(xAxis, edges[i]);

        Array.Sort(edgesAngles);

         List<Robot>[] result = new List<Robot>[edges.Count];
        for (int i = 0; i < result.Length; i++)
            result[i] = new List<Robot>();

        foreach (Robot robot in robots)
        {
            float angle = Angle(xAxis, robot.transform.position);
            int sectorIndex = GetIndexOfRage(angle, edgesAngles);
            result[sectorIndex].Add(robot);
        }

        Robot[][] res = new Robot[edges.Count][];
        for (int i = 0; i < edges.Count; i++)
            res[i] = result[i].ToArray();

        return res.ToArray();
    }

    private static int GetIndexOfRage(float val, float[] ranges)
    {
        int result = 0;
        while (val > ranges[result])
            result++;
        return result-1;
    }

    internal static bool HasSameXVirtualValue(Robot robot1, Robot robot2, Vector2 xAxis)
    {
        float x1 = ToVirtualPoint(xAxis, robot1.transform.position).x;
        float x2 = ToVirtualPoint(xAxis, robot2.transform.position).x;

        return IsEqual(x1, x2);
    }

    internal static Robot[] FilterFromPivotAndAngles(Robot[] robots)
    {
        List<Robot> list = new List<Robot>();
        foreach(Robot robot in robots)
            if (!(robot.state == Robot.State.PIVOT || robot.state == Robot.State.ANGLE))
                list.Add(robot);

        return list.ToArray();
    }

    internal static Vector2 GetVirtualCoordinate(Robot robot, Vector2 xAxis)
    {
        float y = GetVirtualY(robot, xAxis);
        float x = GetVirtualX(robot, xAxis);

        return new Vector2(x, y);
    }

    internal static float GetVirtualY(Robot robot, Vector2 xAxis)
    {
        return ToVirtualPoint(xAxis, robot.transform.position).y;
    }

    internal static float GetVirtualX(Robot robot, Vector2 xAxis)
    {
        return ToVirtualPoint(xAxis, robot.transform.position).x;
    }

    internal static float GetVirtualX(Vector2 p, Vector2 xAxis)
    {
        return ToVirtualPoint(xAxis, p).x;
    }

    internal static Robot[] GetPivotAndAngles(Snapshot snapshot)
    {
        List<Robot> result = new List<Robot>();
        foreach (Robot r in snapshot.GetRobotsList())
            if (r.state == Robot.State.PIVOT || r.state == Robot.State.ANGLE)
                result.Add(r);
        return result.ToArray();
    }

    internal static SymmetryCase CalculateSymmetry(Snapshot snapshot)
    {
        int symCount = GetSymmetryAxisCount(snapshot);

        if (symCount == 1)
            return SymmetryCase.SYMMETRY_1_AXIS;
        else if (IsRotational(snapshot))
            return SymmetryCase.ROTATIONAL_SYMMETRY;
        else
            return SymmetryCase.ASYMMETRY;
    }

    internal static SymmetryCaseCycle2_3 CalculateSymmetryCycle2_3(Snapshot snapshot)
    {
        Robot[] pivots = GetPivots(snapshot);
        
        if (pivots.Length == 1)
        {
            return SymmetryCaseCycle2_3.A;
        }
        else if (pivots.Length == 2)
        {
            float angle = Vector2.Angle(pivots[0].transform.position, pivots[1].transform.position);
            if (angle - BaseAngle(snapshot) < anglePrecision)
                return SymmetryCaseCycle2_3.C;
            else
                return SymmetryCaseCycle2_3.D;
        }
        else
        {
            if (snapshot.GetRobotsList().Any(r => r.state == State.PLACEHOLDER))
                return SymmetryCaseCycle2_3.F;
            else
                return SymmetryCaseCycle2_3.E;
        }
    }

    internal static bool IsRotational(Snapshot snapshot)
    {
        Robot[] robots = snapshot.GetRobotsList();
        string s = MinAngleString(robots, snapshot);

        int count = 0;

        foreach(Robot r in robots)
        {
            if (new AngleString(r, snapshot).Min().Equals(s))
                count++;

            if (count > 1)
                return true;
        }
        return false;
    }

    internal static int GetSymmetryAxisCount(Snapshot snapshot)
    {
        Robot[] robots = snapshot.GetRobotsList();
        int symCount = 0;

        foreach (Robot robot in robots)
        {
            Vector2 xAxis = robot.transform.position;
            Tuple<Robot[], Robot[]> disks = GetHalfDisks(xAxis, snapshot);
            if (Mirror(disks.Item1, disks.Item2, xAxis))
                symCount++;
        }

        if (robots.Length % 2 == 0)
        {
            //If even N means the symAxis counted pass through 2 Robots
            //so we counted the same axis twice.
            symCount = symCount / 2;

            //simmetric1axis0pivot
            Robot[] tailed = Tail(SortedCounterWise(robots));
            List<Vector2> SymAxes = new List<Vector2>();

            for (int i = 0; i < tailed.Length - 1; i++)
            {
                Robot curr = tailed[i];
                Robot next = tailed[i + 1];
                Vector2 xAxis = MiddlePoint(curr.transform.position, next.transform.position).normalized;

                Tuple<Robot[], Robot[]> disks = GetHalfDisks(xAxis, snapshot);

                // We can have two diffrent symmetry axes which are actually the same
                // when all robots lie only in one half of the SEC.
                if (Mirror(disks.Item1, disks.Item2, xAxis))
                {
                    bool duplicate = false;
                    foreach(Vector2 ax in SymAxes)
                    {
                        if (SameAxis(ax, xAxis))
                            duplicate = true;
                    }

                    if(!duplicate)
                        SymAxes.Add(xAxis);
                }
            }
            symCount += SymAxes.Count;
        }

        return symCount;
    }

    public static bool SameAxis(Vector2 axis1, Vector2 axis2)
    {
        Line l1 = new Line(Vector2.zero, axis1);
        Line l2 = new Line(Vector2.zero, axis2);

        if (float.IsPositiveInfinity(l1.m))
            return float.IsPositiveInfinity(l1.m);

        if (float.IsNegativeInfinity(l1.m))
            return float.IsNegativeInfinity(l1.m);

        if (float.IsPositiveInfinity(l2.m))
            return float.IsPositiveInfinity(l2.m);

        if (float.IsNegativeInfinity(l2.m))
            return float.IsNegativeInfinity(l2.m);


        bool result = IsEqual(l1.m, l2.m);
        return result;
    }

    internal static Vector2 Sym1AxUpperSide(Snapshot snapshot)
    {
        Robot[] robots = SortedCounterWise(snapshot.GetRobotsList());

        for (int i = 0; i < robots.Length - 1; i++)
        {
            Robot curr = robots[i];
            Robot next = robots[i + 1];
            Vector2 mid = MiddlePoint(curr.transform.position, next.transform.position);

            if (IsSymmetric1Axis0Pivot(mid, snapshot))
            {
                Vector2 result = GetUpperSide(mid, snapshot);
                return result.normalized * (new Sec(snapshot)).radius;
            }
        }

        throw new Exception("No Symmetry Axis found!");
    }

    internal static Vector2 MiddlePoint(Vector3 p1, Vector3 p2)
    {
        return new Vector2((p1.x + p2.x) / 2, (p1.y + p2.y) / 2);
    }

    public static Robot GetPivotOfMainRegularTriple(Snapshot snapshot)
    {
        // Used in the case of 2 pivot but only 1 regular triple
        Robot[] pivots = GetPivots(snapshot);
        Robot[] angles = GetAngles(snapshot);

        return SortedByDistanceFromPoint(angles[0].transform.position, pivots)[0];
    }

    internal static Vector2 GetUpperSide(Vector2 xAxis, Snapshot snapshot)
    {
        Tuple<Robot[], Robot[]> disks = GetHalfDisksGivenXAxis(xAxis, snapshot);
        Vector2 xAxis2 = RealPointGivenAngle(180, xAxis, new Sec(snapshot));
        Robot[] disk1 = disks.Item1;

        List<Robot> quarter1 = new List<Robot>();
        List<Robot> quarter2 = new List<Robot>();

        foreach (Robot r in disk1)
        {
            if (Angle(xAxis, r.transform.position) <= 90)
                quarter1.Add(r);

            if (Angle(xAxis, r.transform.position) >= 90)
                quarter2.Add(r);
        }

        string min1 = MinAngleString(quarter1.ToArray(), snapshot);
        string min2 = MinAngleString(quarter2.ToArray(), snapshot);

        if (min1 == null)
            return xAxis2;

        if (min2 == null)
            return xAxis;

        if (min1.CompareTo(min2) >= 0)
            return xAxis2;
        else
            return xAxis;
    }

    internal static bool IsSymmetric1Axis0Pivot(Vector2 xAxis, Snapshot snapshot)
    {
        Tuple<Robot[], Robot[]> disks = GetHalfDisksGivenXAxis(xAxis, snapshot);
        Robot[] disk1 = disks.Item1;
        Robot[] disk2 = disks.Item2;

        SortByXVirtualValue(disk1, xAxis);
        SortByXVirtualValue(disk2, xAxis);

        return Mirror(disk1, disk2, xAxis);
    }

    internal static Tuple<Robot[], Robot[]> GetHalfDisksGivenXAxis(Vector2 xAxis, Snapshot snapshot)
    {
        List<Robot> disk1 = new List<Robot>();
        List<Robot> disk2 = new List<Robot>();

        foreach (Robot r in snapshot.GetRobotsList())
        {
            if (Angle(xAxis, r.transform.position) > 180)
                disk2.Add(r);
            else
                disk1.Add(r);
        }

        return new Tuple<Robot[], Robot[]>(disk1.ToArray(), disk2.ToArray());
    }

    private static bool IsSymmetric1Axis2Pivot(Robot robot, Snapshot snapshot)
    {
        Vector2 xAxis = robot.transform.position;
        Vector2 oppositePoint = RealPointGivenAngle(180, xAxis, new Sec(snapshot));
        Tuple<Robot[], Robot[]> disks;
        Robot[] disk1, disk2;
        Robot[] robots = snapshot.GetRobotsList();
        Robot opposite = GetOppositeRobot(robot, snapshot);

        if (opposite == null)
            return false;

        robots = robots.Where(val => val != opposite).ToArray();

        disks = GetHalfDisks(xAxis, snapshot);
        disk1 = disks.Item1;
        disk2 = disks.Item2;
        if (!Mirror(disk1, disk2, xAxis))
            return false;

        return true;
    }

    internal static Robot[] GetPivots(Snapshot snapshot)
    {
        return GetRobotInState(snapshot, State.PIVOT);
    }

    internal static Robot[] GetAngles(Snapshot snapshot)
    {
        return GetRobotInState(snapshot, State.ANGLE);
    }

    internal static Robot[] GetRobotInState(Snapshot snapshot, State state)
    {
        List<Robot> res = new List<Robot>();

        foreach (Robot robot in snapshot.GetRobotsList())
            if (robot.state == state)
                res.Add(robot);

        return res.ToArray();
    }

    public static bool Mirror(Robot[] disk1, Robot[] disk2, Vector2 xAxis)
    {
        if (disk1.Length != disk2.Length)
            return false;

        SortByXVirtualValue(disk1, xAxis);
        SortByXVirtualValue(disk2, xAxis);

        for (int i = 0; i < disk1.Length; i++)
            if (!HasSameXVirtualValue(disk1[i], disk2[i], xAxis))
                return false;

        return true;
    }

    public static Robot ElectRobot(Snapshot snapshot)
    {
        return GetAngleStringSequence(snapshot)[0].Item2;
    }

    public static Tuple<string, Robot>[] GetAngleStringSequence(Snapshot snapshot)
    {
        Tuple<string, Robot>[] result = new Tuple<string, Robot>[snapshot.Size() * 2];
        AngleString[] a = GetAngleStrings(snapshot);

        for(int i = 0; i < snapshot.Size(); i++)
        {
            result[2 * i] = Tuple.Create(a[i].clockwise, a[i].robot);
            result[2 * i + 1] = Tuple.Create(a[i].counterwise, a[i].robot);
        }

        //TODO da controllare che non ce ne siano 2 minime uguali
        Array.Sort(result, new LambdaSortAngleString());

        return result;
    }

    
    public static AngleString[] GetAngleStrings(Snapshot snapshot)
    {
        int n = snapshot.Size();
        AngleString[] result = new AngleString[n];
        Robot[] robots = snapshot.GetRobotsList();

        for(int i=0; i<n; i++)
            result[i] = new AngleString(robots[i], snapshot);

        return result;
    }

    public static Robot ElectPivotAsymmetricCase(Snapshot snapshot)
    {;
        Robot[] robots = snapshot.GetRobotsList();
        Robot elected = ElectRobot(snapshot);
        Orientation orientation = GetOrientation(snapshot);
        SortByOrientation(robots, elected.transform.position, orientation);

        foreach (Robot robot in robots)
            if (CanBePivot(robot, snapshot, SymmetryCase.ASYMMETRY))
                return robot;

        throw new Exception("No pivot found!");
    }

    public static bool CanBePivot(Robot robot, Snapshot snapshot, SymmetryCase symmetryCase)
    {
        switch (symmetryCase)
        {
            case SymmetryCase.ASYMMETRY:
                Tuple<Robot[], Robot[]> halfDisks = GetHalfDisks(robot.transform.position, snapshot);
                return Math.Abs(halfDisks.Item1.Length - halfDisks.Item2.Length) <= 1;

            case SymmetryCase.SYMMETRY_1_AXIS:
                Tuple<Robot[], Robot[]> disks;
                Robot[] disk1, disk2;

                Vector2 xAxis = robot.transform.position;
                disks = GetHalfDisks(xAxis, snapshot, true);
                disk1 = disks.Item1;
                disk2 = disks.Item2;
                

                if (disk1.Length == disk2.Length && Mirror(disk1, disk2, xAxis))
                    return true;
                else
                    return false;

            default:
                throw new Exception("Symmetry case undefined: " + symmetryCase);
        }


    }

    private static Robot GetOppositeRobot(Robot robot, Snapshot snapshot)
    {
        Robot[] robots = snapshot.GetRobotsList();
        foreach (Robot r in robots)
            if (Mathf.Abs(Angle(robot.transform.position, r.transform.position) - 180) < 0.01)
                return r;

        return null;
    }

    public static Tuple<Robot[], Robot[]> GetHalfDisks(Vector2 xAxis, Snapshot snapshot, bool ignoreRobotOnAxis = false)
    {
        // if xAxis is Vector2.up then disk1 is the left disk.
        List<Robot> disk1 = new List<Robot>();
        List<Robot> disk2 = new List<Robot>();

        foreach (Robot r in snapshot.GetRobotsList())
        {
            bool itsMe = Vector2.Angle(xAxis, r.transform.position) < anglePrecision;
            bool itsMyOpposite = Math.Abs(Vector2.Angle(xAxis, r.transform.position) - 180) < anglePrecision;
            
            if (itsMe || itsMyOpposite)
            {
                if (!ignoreRobotOnAxis)
                {
                    disk1.Add(r);
                    disk2.Add(r);
                }
                continue;
            }

            if (Angle(xAxis, r.transform.position) > 180)
                disk2.Add(r);
            else
                disk1.Add(r);
        }

        return new Tuple<Robot[], Robot[]>(disk1.ToArray(), disk2.ToArray());
    }





    public static Robot GetClosestRobot(Vector2 from, Robot[] robots)
    {
        Array.Sort(robots, new LambdaClosestRobot(from));
        return robots[1];
    }

    internal static Robot[] SortedByDistanceFromPoint(Vector2 from, Robot[] robots)
    {
        Robot[] shallowCopy = new Robot[robots.Length];
        for (int i = 0; i < robots.Length; i++)
            shallowCopy[i] = robots[i];

        Array.Sort(shallowCopy, new LambdaClosestRobot(from));
        return shallowCopy;
    }

    internal static Vector2[] SortedByDistanceFromPoint(Vector2 from, Vector2[] points)
    {
        Vector2[] shallowCopy = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
            shallowCopy[i] = points[i];

        Array.Sort(shallowCopy, new LambdaClosestPoint(from));
        return shallowCopy;
    }

    internal static void SortByDistanceFromPoint(Vector2 from, Robot[] robots)
    {
        Array.Sort(robots, new LambdaClosestRobot(from));
    }

    public static Tuple<Robot[], Robot[]> GetNearestAndFurthestCupleRobots(Vector2 from, Robot[] robots)
    {
        SortByDistanceFromPoint(from, robots);
        Robot[] closest = new Robot[] { robots[0], robots[1] };
        Robot[] furthest = new Robot[] { robots[robots.Length - 2], robots[robots.Length - 1] };
        return new Tuple<Robot[], Robot[]>(closest, furthest) ;
    }

    public static Orientation GetOrientation(Snapshot snapshot)
    {
        Tuple<string, Robot>[] angleStrs = GetAngleStringSequence(snapshot);
        Vector2 p1 = (Vector2)angleStrs[0].Item2.transform.position;
        Vector2 p2 = (Vector2)angleStrs[1].Item2.transform.position;
        
        float angle = Vector2.SignedAngle(p1, p2);
        
        if(angle > 0)
            return Orientation.CLOCKWISE;
        else
            return Orientation.COUNTERWISE;
    }

    public static float Angle(Vector2 xAxis, Vector2 to)
    {
        // Vector2.SignedAngle uses the Counterwise orientation. I stick to it.
        float angle = Vector2.SignedAngle(xAxis, to);

        if (angle < 0)
            angle = 360 + angle;

        return angle;
    }

    public static Vector2 RealPointGivenAngle(float angle, Vector2 xAxis, Sec sec)
    {
        float offsetAngle = Vector2.SignedAngle(Vector2.right, xAxis);
        float realRadiantAngle = (offsetAngle + angle) * (float)(Mathf.PI / 180.0);
        return new Vector2(sec.radius * Mathf.Cos(realRadiantAngle), sec.radius * Mathf.Sin(realRadiantAngle));
    }

    public static Vector2 VirtualPointGivenAngle(float angle, Vector2 xAxis, Sec sec)
    {
        float radiantAngle = angle * (float)(Mathf.PI / 180.0);
        return new Vector2(sec.radius * Mathf.Cos(radiantAngle), sec.radius * Mathf.Sin(radiantAngle));
    }

    public static Vector2 GetAxisGivenAngle(float angle, float radius)
    {
        float rad = angle * (float)(Mathf.PI / 180.0);
        return new Vector2(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad));
    }

    public static Vector2 ToVirtualPoint(Vector2 xAxis, Vector2 point)
    {
        float offsetAngle = Vector2.SignedAngle(Vector2.right, xAxis);
        float pointAngle = Vector2.SignedAngle(Vector2.right, point);
        float angle = pointAngle - offsetAngle;
        float hypotenuse = Mathf.Sqrt(Mathf.Pow(point.x, 2) + Mathf.Pow(point.y,2));
        float realRadiantAngle = angle * (float)(Mathf.PI / 180.0);
        return new Vector2(hypotenuse * Mathf.Cos(realRadiantAngle), hypotenuse * Mathf.Sin(realRadiantAngle));
    }

    internal static Robot Next(Robot robot, Snapshot snapshot, Orientation orientation)
    {
        Robot[] robots = snapshot.GetRobotsList();
        robots = SortedByOrientation(robots, Vector2.up, orientation);

        //Ci sono problemi se prendi un xAxis su cui giace un robot.
        Vector2 xAxis = MiddlePoint(robots[0].transform.position, robots[1].transform.position);
        robots = SortedByOrientation(robots, xAxis, orientation);

        int index = Array.IndexOf(robots, robot);
        return HeadAndTail(robots)[index + 2];
    }

    public static Vector2 ToRealPoint(Vector2 xAxis, Vector2 point)
    {
        float offsetAngle = Vector2.SignedAngle(Vector2.right, xAxis);
        float pointAngle = Vector2.SignedAngle(Vector2.right, point);
        float angle = pointAngle + offsetAngle;
        float hypotenuse = Mathf.Sqrt(Mathf.Pow(point.x, 2) + Mathf.Pow(point.y, 2));
        float realRadiantAngle = angle * (float)(Mathf.PI / 180.0);
        return new Vector2(hypotenuse * Mathf.Cos(realRadiantAngle), hypotenuse * Mathf.Sin(realRadiantAngle));
    }
    
    public static void SortByXVirtualValue(Robot[] robots, Vector2 xAxis)
    {
        Array.Sort(robots, new LambdaXVirtualValue(xAxis));
    }

    public static float SafeDiametersOffset(Vector2 xAxis, Snapshot snapshot)
    {
        List<float> values = new List<float>();
        Robot[] robots = snapshot.GetRobotsList();

        foreach (Robot r in robots)
        {
            float b = Mathf.Abs(ToVirtualPoint(xAxis, r.transform.position).y);

            //In realta ci sarebbe da controllare che
            //r non sia il pivot o il robot opposto al pivot
            //nel controllare che sia l'opposto c'è sempre un pb di precisione
            //quindi mi semplifico la vita e faccio solo questo controllo
            if (b < 0.1)
                continue;

            values.Add(b);
        }

        if(robots.Length % 2 == 0)
            values.Add(Mathf.Abs(VirtualPointGivenAngle(BaseAngle(snapshot), xAxis, new Sec(snapshot)).y));
        else
            values.Add(Mathf.Abs(VirtualPointGivenAngle(BaseAngle(snapshot) * (robots.Length/2), xAxis, new Sec(snapshot)).y));

        return values.Min()/2;
    }

    public static Vector2 SameSide(Robot robot, Vector2 a1, Vector2 a2, Vector2 xAxis)
    {
        float robotAngle = Angle(xAxis, robot.transform.position);
        float angle1 = Angle(xAxis, a1);

        if (robotAngle >= 180)
            if (angle1 >= 180)
                return a1;
            else
                return a2;
        else
            if (angle1 >= 180)
            return a2;
        else
            return a1;

    }

    internal static Robot[] HeadAndTail(Robot[] robots)
    {
        Robot head = robots[robots.Length - 1];
        Robot tail = robots[0];

        List<Robot> result = robots.ToList();
        result.Insert(0, head);
        result.Add(tail);

        return result.ToArray();
    }

    public static Line SafeDiameter(Robot r, Vector2 xAxis, Snapshot snapshot)
    {
        // Returns the safe diameter of the Robot r
        float offset = SafeDiametersOffset(xAxis, snapshot);
        Tuple<Robot[], Robot[]> disks = GetHalfDisks(xAxis, snapshot);
        Line result = new Line(Vector2.zero, xAxis);

        if (disks.Item1.Contains(r))
            result.q = offset;
        else
            result.q = -offset;

        return result;
    }

    public static int GetOrderedIndex(Robot r, Vector2 xAxis, Snapshot snapshot)
    {
        // CAN USE THIS WHEN NO ROBOTS LIE ON xAxis!
        // It's hard to handle the problem is the Robot on 0 or 360?
        Tuple<Robot[], Robot[]> disks = GetHalfDisks(xAxis, snapshot);
        Orientation orientation;
        Robot[] disk;
        if (disks.Item1.Contains(r))
        {
            disk = disks.Item1;
            orientation = Orientation.COUNTERWISE;
        }
        else
        {
            disk = disks.Item2;
            orientation = Orientation.CLOCKWISE;
        }

        SortByOrientation(disk, xAxis, orientation);
        return Array.IndexOf(disk, r);
    }

    internal static Line SafeChord(Robot r, Snapshot snapshot)
    {
        Tuple<Robot, Robot> pivots = LateralPivot(r, snapshot);
        Vector2 p1 = pivots.Item1.transform.position;
        Vector2 p2 = pivots.Item2.transform.position;
        Line safeChord = new Line(p1, p2);
        return safeChord;
    }

    internal static Tuple<Robot, Robot> LateralPivot(Robot r, Snapshot snapshot)
    {
        Robot[] pivots = GetPivots(snapshot);
        Robot[] pivsAndMe = new Robot[pivots.Length + 1];
        Array.Copy(pivots, pivsAndMe, pivots.Length);
        pivsAndMe[pivsAndMe.Length - 1] = r;
        pivsAndMe = SortedByOrientation(pivsAndMe, r.transform.position);
        return new Tuple<Robot, Robot>(pivsAndMe[1], pivsAndMe[pivsAndMe.Length - 1]);
    }

    internal static Robot[] FilterFromPivotOutsideRange(Robot[] robots, Vector2 xAxis, float range)
    {
        List<Robot> list = new List<Robot>();
        foreach (Robot robot in robots)
        {
            float ang = Angle(xAxis, robot.transform.position);
            if (ang < range)
                list.Add(robot);
        }

        return list.ToArray();
    }

    public static bool IsEqual(float x, float y)
    {
        return Math.Abs(x - y) < spacePrecision;
    }

    public static bool IsEqualAngle(float angle1, float angle2)
    {
        return Math.Abs(angle1 - angle2) < anglePrecision;
    }

    public static Robot[] HalfDiskOfRobot(Robot r, Vector2 xAxis, Snapshot snapshot, bool ignoreRobotOnAxis = false)
    {
        Tuple<Robot[], Robot[]> disks = GetHalfDisks(xAxis, snapshot, ignoreRobotOnAxis);
        if (disks.Item1.Contains(r))
            return disks.Item1;
        else
            return disks.Item2;
    }

    public static Robot[] RemovedRobot(Robot[] robots, Robot toRemove)
    {
        return robots.Where(val => val != toRemove).ToArray();
    }

    public static Robot[] RemovedRobot(Robot[] robots, Robot[] toRemove)
    {
        List<Robot> result = new List<Robot>();

        foreach (Robot robot in robots)
            if (!toRemove.Contains(robot))
                result.Add(robot);

        return result.ToArray();
    }

    public static bool OnTheLeft(Robot r, Vector2 xAxis, Snapshot snapshot)
    {
        Tuple<Robot[], Robot[]> disks = GetHalfDisks(xAxis, snapshot);
        return disks.Item1.Contains(r);
    }

    public static Robot[] PivotsSym1Axis(Snapshot snapshot)
    {
        Robot[] robots = snapshot.GetRobotsList();

        List<Robot> result = new List<Robot>();

        foreach (Robot robot in robots)
            if (CanBePivot(robot, snapshot, SymmetryCase.SYMMETRY_1_AXIS))
                result.Add(robot);

        return result.ToArray();
    }


    public static Robot[] Tail(Robot[] robots)
    {
        List<Robot> result = new List<Robot>();

        foreach (Robot robot in robots)
                result.Add(robot);

        result.Add(robots[0]);
        return result.ToArray();
    }

    public static Robot[] Obstructors(Robot r1, Robot r2, Robot[] robots)
    {
        Vector2 xAxis = r2.transform.position - r1.transform.position;
        Vector2 r2VPos = ToVirtualPoint(xAxis, r2.transform.position - r1.transform.position);
        Robot[] others = (Robot[])robots.Clone();
        others = RemovedRobot(others, r1);
        others = RemovedRobot(others, r2);

        List<Robot> obstructors = new List<Robot>();
        foreach (Robot r3 in others)
        {
            Vector2 r3VPos = ToVirtualPoint(xAxis, r3.transform.position - r1.transform.position);

            if (r3VPos.y == 0 && r3VPos.x < r2VPos.x)
                obstructors.Add(r3);
        }

        return obstructors.ToArray();
    }

    public static Robot[] GetAllRobots()
    {
        return FindObjectsOfType<Robot>();
    }

    public static Clock GetClock()
    {
        return FindObjectOfType<Clock>();
    }
}
