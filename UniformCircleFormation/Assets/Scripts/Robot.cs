using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static RobotLogic;

public class Robot : MonoBehaviour
{
    public int id;
    public State state;
    
    bool move = false;
    bool performStep;
    Vector2 destination;

    public static float speed = 8f;
    public static float spacePrecision = 0.001f;
    public static float anglePrecision = 0.1f;
    public static float slowDownRadius = 0.2f; // [Range(0,1)]

    public static Dictionary<State, Color> colors = new Dictionary<State, Color>(){
        {State.PIVOT, Color.red},
        {State.ANGLE, Color.yellow},
        {State.INTERNAL, Color.magenta},
        {State.INITIAL, Color.blue},
        {State.SEC, Color.cyan},
        {State.PLACEHOLDER, Color.green}
    };

    public enum State
    {
        PIVOT,
        ANGLE,
        INTERNAL,
        INITIAL,
        SEC,
        PLACEHOLDER
    }

    public enum SymmetryCase
    {
        ASYMMETRY,
        SYMMETRY_1_AXIS,
        ROTATIONAL_SYMMETRY
    }

    public enum SymmetryCaseCycle2_3
    {
        A,            // asym (odd n), sym1ax (odd n)
        B,       // asym (even n)
        C,   // sym1ax (even n 0 pivot)
        D,   // sym1ax (even n 2 pivot)
        E,        // rot1min
        F,        // rot2min
    }

    public enum Orientation
    {
        CLOCKWISE,
        COUNTERWISE
    }

    void Start()
    {
        RegisterToClock();
        StartCoroutine(UCFSORL());
    }

    private void Update()
    {
        if (move)
            MoveToward(destination);
    }

    private IEnumerator UCFSORL()
    {
        /* 
        Input C0: Valid Configuration of n robots

        ############# PRELIMINARY PHASE ###############
        C1 = CompleteVisibility(C0)
        Ch = R on the SEC of C1
        if(Ch is a perfect convex hull){
            Execute known algorithm
            exit
        }
        */

        PositionAndColor positionAndColor;
        // ############## EXECUTION OF CYCLE 1 ###################
        yield return new WaitUntil(() => performStep);
        performStep = false;
        Snapshot Ch = Look();

        yield return new WaitUntil(() => performStep);
        performStep = false;
        positionAndColor = ComputeC1(Ch);

        yield return new WaitUntil(() => performStep);
        performStep = false;
        Move(positionAndColor);

        // ############## EXECUTION OF CYCLE 2 ###################
        yield return new WaitUntil(() => performStep);
        performStep = false;
        Snapshot c2 = Look();

        yield return new WaitUntil(() => performStep);
        performStep = false;
        positionAndColor = ComputeC2(c2);

        yield return new WaitUntil(() => performStep);
        performStep = false;
        Move(positionAndColor);

        // ############## EXECUTION OF CYCLE 3 ###################
        yield return new WaitUntil(() => performStep);
        performStep = false;
        Snapshot c3 = Look();

        yield return new WaitUntil(() => performStep);
        performStep = false;
        positionAndColor = ComputeC3(c3);

        yield return new WaitUntil(() => performStep);
        performStep = false;
        Move(positionAndColor);
    }

    private Snapshot Look()
    {
        Robot[] robots = GetAllRobots();
        robots = SeesFilter(robots);
        return new Snapshot(robots);
    }

    private void Move(PositionAndColor positionAndColor)
    {
        destination = positionAndColor.point;
        move = true;
        SetState(positionAndColor.state);
    }

    private PositionAndColor ComputeC1(Snapshot snapshot)
    {
        switch (CalculateSymmetry(snapshot))
        {
            case SymmetryCase.ASYMMETRY:
                return ComputeC1_Asymmetry(snapshot);
            case SymmetryCase.SYMMETRY_1_AXIS:
                return ComputeC1_1Axis(snapshot);
            case SymmetryCase.ROTATIONAL_SYMMETRY:
                return ComputeC1_Rotational(snapshot);
            default:
                throw new KeyNotFoundException();
        }
    }

    private PositionAndColor ComputeC2(Snapshot snapshot)
    {
        if (state == State.PIVOT || state == State.ANGLE)
            return new PositionAndColor(transform.position, state);

        Vector2 xAxis;
        switch (CalculateSymmetryCycle2_3(snapshot))
        {
            case SymmetryCaseCycle2_3.A:
                xAxis = GetPivots(snapshot)[0].transform.position;
                return ComputeC2_ABCD(xAxis, snapshot);

            case SymmetryCaseCycle2_3.B:
                xAxis = GetPivots(snapshot)[0].transform.position;
                return ComputeC2_ABCD(xAxis, snapshot);

            case SymmetryCaseCycle2_3.D:
                xAxis = GetPivotOfMainRegularTriple(snapshot).transform.position;
                return ComputeC2_ABCD(xAxis, snapshot);

            case SymmetryCaseCycle2_3.C:
                Robot[] pivots = GetPivots(snapshot);
                xAxis = MiddlePoint(pivots[0].transform.position, pivots[1].transform.position);
                return ComputeC2_ABCD(xAxis, snapshot, BaseAngle(snapshot)/2);

            case SymmetryCaseCycle2_3.E:
                return ComputeC2_Rotational(snapshot);

            case SymmetryCaseCycle2_3.F:
                return ComputeC2_Rotational(snapshot);

            default:
                throw new KeyNotFoundException();
        }
    }

    private PositionAndColor ComputeC3(Snapshot snapshot)
    {
        if (state != State.INTERNAL)
            return new PositionAndColor(transform.position, state);

        Robot[] pivots = GetPivots(snapshot);
        Vector2 xAxis;
        switch (CalculateSymmetryCycle2_3(snapshot))
        {
            case SymmetryCaseCycle2_3.A:
                xAxis = pivots[0].transform.position;
                return ComputeC3_X(snapshot, xAxis);

            case SymmetryCaseCycle2_3.B:
                xAxis = pivots[0].transform.position;
                return ComputeC3_X(snapshot, xAxis);

            case SymmetryCaseCycle2_3.D:
                xAxis = GetPivotOfMainRegularTriple(snapshot).transform.position;
                return ComputeC3_X(snapshot, xAxis);

            case SymmetryCaseCycle2_3.C:
                xAxis = MiddlePoint(pivots[0].transform.position, pivots[1].transform.position);
                return ComputeC3_X(snapshot, xAxis);

            case SymmetryCaseCycle2_3.E:
                xAxis = SafeChord(this, snapshot).GetXAxis();
                return ComputeC3_X(snapshot, xAxis);

            case SymmetryCaseCycle2_3.F:
                xAxis = SafeChord(this, snapshot).GetXAxis();
                return ComputeC3_X(snapshot, xAxis);

            default:
                throw new KeyNotFoundException();
        }
    }

    private PositionAndColor ComputeC1_Asymmetry(Snapshot snapshot)
    {
        Robot pivot = ElectPivotAsymmetricCase(snapshot);

        if (this == pivot)
            return new PositionAndColor(transform.position, State.PIVOT);

        Vector2 xAxis = pivot.transform.position;
        Robot[] myDisk = HalfDiskOfRobot(this, xAxis, snapshot);
        myDisk = RemovedRobot(myDisk, pivot);

        Orientation orientation;
        float imleft;
        if (OnTheLeft(this, xAxis, snapshot))
        {
            imleft = 1;
            orientation = Orientation.COUNTERWISE;
        }
        else
        {
            imleft = -1;
            orientation = Orientation.CLOCKWISE;
        }

        SortByOrientation(myDisk, xAxis, orientation);

        float baseAngle = BaseAngle(snapshot);
        Sec sec = new Sec(snapshot);
        Vector2 a1, a2;
        a1 = RealPointGivenAngle(imleft * baseAngle, xAxis, sec);

        if (this == myDisk[0])
            return new PositionAndColor(a1, State.ANGLE);
        
        if (snapshot.Size() % 2 == 1) // ODD N
        {
            a2 = RealPointGivenAngle(180 - imleft * baseAngle / 2, xAxis, sec);
            if (this == myDisk[myDisk.Length-1])
                return new PositionAndColor(a2, State.ANGLE);
        }
        else // EVEN N
        {
            Tuple<Robot[], Robot[]> disks = GetHalfDisks(xAxis, snapshot);

            if (disks.Item1.Length > disks.Item2.Length)
                myDisk = disks.Item1;
            else
                myDisk = disks.Item2;

            if (this == myDisk[myDisk.Length - 1])
            {
                a2 = RealPointGivenAngle(180, pivot.transform.position, new Sec(snapshot));
                return new PositionAndColor(a2, State.ANGLE);
            }
                
        }

        return new PositionAndColor(transform.position);
    }

    private PositionAndColor ComputeC1_1Axis(Snapshot snapshot)
    {
        Vector2 p, a1, a2;

        float baseAngle = BaseAngle(snapshot);
        Sec sec = new Sec(snapshot);

        Robot[] pivots = PivotsSym1Axis(snapshot);

        Vector2 xAxis;
        float imleft;
        Orientation orientation;

        if (pivots.Contains(this))
            return new PositionAndColor(transform.position, State.PIVOT);

        if (pivots.Length == 0)
        {
            xAxis = Sym1AxUpperSide(snapshot);

            if (OnTheLeft(this, xAxis, snapshot))
            {
                imleft = 1;
                orientation = Orientation.COUNTERWISE;
            }
            else
            {
                imleft = -1;
                orientation = Orientation.CLOCKWISE;
            }

            Robot[] disk = HalfDiskOfRobot(this, xAxis, snapshot);
            SortByOrientation(disk, xAxis, orientation);

            float half = baseAngle / 2;
            p = RealPointGivenAngle(imleft * half, xAxis, sec);
            a1 = RealPointGivenAngle(imleft * (baseAngle + half), xAxis, sec);
            a2 = RealPointGivenAngle(180 - (imleft * half), xAxis, sec);

            if (this == disk[0])
                return new PositionAndColor(p, State.PIVOT);
            if (this == disk[1])
                return new PositionAndColor(a1, State.ANGLE);
            if (this == disk[disk.Length - 1])
                return new PositionAndColor(a2, State.ANGLE);
        }

        if(pivots.Length == 1)
        {
            Robot pivot = pivots[0];
            xAxis = pivots[0].transform.position;

            if (OnTheLeft(this, xAxis, snapshot))
            {
                imleft = 1;
                orientation = Orientation.COUNTERWISE;
            }
            else
            {
                imleft = -1;
                orientation = Orientation.CLOCKWISE;
            }

            Robot[] disk = HalfDiskOfRobot(this, xAxis, snapshot);
            disk = RemovedRobot(disk, pivot);
            SortByOrientation(disk, xAxis, orientation);

            a1 = RealPointGivenAngle(imleft * baseAngle, xAxis, sec);
            a2 = RealPointGivenAngle(180 - (imleft * baseAngle / 2), xAxis, sec);
            
            if (this == disk[0])
                return new PositionAndColor(a1, State.ANGLE);
            if (this == disk[1])
                return new PositionAndColor(a2, State.ANGLE);
        }

        if (pivots.Length == 2)
        {
            xAxis = GetUpperSide(pivots[0].transform.position, snapshot);

            if (OnTheLeft(this, xAxis, snapshot))
            {
                imleft = 1;
                orientation = Orientation.COUNTERWISE;
            }
            else
            {
                imleft = -1;
                orientation = Orientation.CLOCKWISE;
            }

            Robot[] disk = HalfDiskOfRobot(this, xAxis, snapshot);
            disk = RemovedRobot(disk, pivots);
            SortByOrientation(disk, xAxis, orientation);

            a1 = RealPointGivenAngle(imleft * baseAngle, xAxis, sec);

            if (this == disk[0])
                return new PositionAndColor(a1, State.ANGLE);
        }

        return new PositionAndColor(transform.position);
    }

    private PositionAndColor ComputeC1_Rotational(Snapshot snapshot)
    {
        //Main class of symmetry
        Robot[] robots = snapshot.GetRobotsList();
        string minAG = MinAngleString(robots, snapshot);
        Robot[] mainClassOfSymmetry = robots.Where(val => new AngleString(val, snapshot).Min() == minAG).ToArray();

        //One minimal if:
        //  The angles between consecutive the Robots in the main 
        //  class of symmetry have the same angle among them (360/|mainclassofsymm|)
        mainClassOfSymmetry = SortedCounterWise(mainClassOfSymmetry);
        float baseAngle = BaseAngle(snapshot);
        bool oneMinimal = true;

        Vector2 p1 = mainClassOfSymmetry[0].transform.position;
        Vector2 p2 = mainClassOfSymmetry[mainClassOfSymmetry.Length - 1].transform.position;
        float lastAng = Math.Abs(Vector2.SignedAngle(p1, p2));
        float minAngle = lastAng;
        float maxAngle = lastAng;
        float mainClassOfSymAngle = 360 / mainClassOfSymmetry.Length;

        for (int i = 0; i < mainClassOfSymmetry.Length - 1; i++)
        {
            p1 = mainClassOfSymmetry[i].transform.position;
            p2 = mainClassOfSymmetry[i + 1].transform.position;
            float angle = Vector2.Angle(p1, p2);

            if (!IsEqualAngle(lastAng, angle))
            {
                oneMinimal = false;
                minAngle = Math.Min(lastAng, angle);
                maxAngle = Math.Max(lastAng, angle);
            }

            if (!IsEqualAngle(mainClassOfSymAngle, angle))
                oneMinimal = false;
        }

        if (oneMinimal)
        {
            if (mainClassOfSymmetry.Contains(this))
                return new PositionAndColor(transform.position, State.PIVOT);

            foreach (Robot pivot in mainClassOfSymmetry)
            {
                Vector2 xAxis = pivot.transform.position;

                //sign is to Consider only the angle toward my side
                float sign = Math.Sign(Vector2.SignedAngle(xAxis, transform.position));
                Vector2 a = RealPointGivenAngle(sign * baseAngle, xAxis, new Sec(snapshot));
                Robot[] sorted = SortedByDistanceFromPoint(a, robots);

                //The closest to a
                Robot closest = sorted[0];

                Vector2 pos1 = sorted[0].transform.position;
                Vector2 pos2 = sorted[1].transform.position;

                //If 2 closest have the same distance then
                //  we consider the closest to the pivot
                if (IsEqual(Vector2.Distance(a, pos1), Vector2.Distance(a, pos2)))
                    if (Vector2.Distance(xAxis, pos1) > Vector2.Distance(xAxis, pos2))
                        closest = sorted[1];

                if (this == closest)
                    return new PositionAndColor(a, State.ANGLE);
            }
        }
        else //TWO MINIMAL STRING CASE
        {
            if (mainClassOfSymmetry.Contains(this))
                return new PositionAndColor(transform.position, State.PLACEHOLDER);

            List<Vector2> externals_list = new List<Vector2>();

            Robot[] tailed = Tail(mainClassOfSymmetry);
            for (int i = 0; i < tailed.Length - 1; i++)
            {
                p1 = tailed[i].transform.position;
                p2 = tailed[i + 1].transform.position;
                float angle = Vector2.Angle(p1, p2);

                if (IsEqualAngle(maxAngle, angle))
                    externals_list.Add(MiddlePoint(p1, p2));
            }

            Vector2[] externals = SortedByDistanceFromPoint(transform.position, externals_list.ToArray());
            Vector2 external = externals[0];

            float myAngle = Vector2.SignedAngle(external, transform.position);

            float imleft;
            Orientation orientation;
            if (myAngle > anglePrecision)
            {
                imleft = 1;
                orientation = Orientation.COUNTERWISE;
            }
            else
            {
                imleft = -1;
                orientation = Orientation.CLOCKWISE;
            }

            robots = SortedByOrientation(robots, external, orientation);

            if (this == robots[0])
            {
                Vector2 pos = RealPointGivenAngle(imleft * baseAngle * 0.5f, external, new Sec(snapshot));
                return new PositionAndColor(pos, State.PIVOT);
            }

            if (this == robots[1])
            {
                Vector2 pos = RealPointGivenAngle(imleft * baseAngle * 1.5f, external, new Sec(snapshot));
                return new PositionAndColor(pos, State.ANGLE);
            }
        }

        return new PositionAndColor(transform.position);
    }

    private PositionAndColor ComputeC2_ABCD(Vector2 xAxis, Snapshot snapshot, float offsetAngle = 0)
    {
        float safeDiamOffset = SafeDiameter(this, xAxis, snapshot).q;

        Orientation orientation;
        float imleft;
        if (OnTheLeft(this, xAxis, snapshot))
        {
            orientation = Orientation.COUNTERWISE;
            imleft = 1;
        }

        else
        {
            imleft = -1;
            orientation = Orientation.CLOCKWISE;
        }


        Robot[] disk = HalfDiskOfRobot(this, xAxis, snapshot);
        disk = FilterFromPivotAndAngles(disk);
        SortByOrientation(disk, xAxis, orientation);

        int myIndex = Array.IndexOf(disk, this) + 2; // +2 Because we ignored r,a1,a2

        float angle = BaseAngle(snapshot) * myIndex + offsetAngle;

        Vector2 virtualPosition = GetAxisGivenAngle(imleft * angle, new Sec(snapshot).radius);
        virtualPosition.y = safeDiamOffset;
        Vector2 realPosition = ToRealPoint(xAxis, virtualPosition);

        return new PositionAndColor(realPosition, State.INTERNAL);
    }

    private PositionAndColor ComputeC2_Rotational(Snapshot snapshot)
    {
        //Calculate my final position on the sec
        //  Consider the robots sitting in the arc delimited by my pivot
        //  Calculate my position ignoring pivots and angles (just add +2 at the and)
        //  Note. the position is calculated on the default orientation (COUNTERWISE)
        //Then the position on the safe chord is:
        //  X = x of the final position
        //  Y = y of one of the lateral pivots
        Vector2 xAxis = SafeChord(this, snapshot).GetXAxis();
        Robot[] robots = SortedByOrientation(snapshot.GetRobotsList(), xAxis, Orientation.COUNTERWISE);

        Tuple<Robot, Robot> pivots = LateralPivot(this, snapshot);
        
        //pivot1 is the one with lower angle
        Robot pivot1, pivot2;
        if (Array.IndexOf(robots, pivots.Item1) < Array.IndexOf(robots, pivots.Item2))
        {
            pivot1 = pivots.Item1;
            pivot2 = pivots.Item2;
        }
        else
        {
            pivot1 = pivots.Item2;
            pivot2 = pivots.Item1;
        }

        float range = Angle(pivot1.transform.position, pivot2.transform.position);
        Robot[] filtered = FilterFromPivotOutsideRange(robots, pivot1.transform.position, range);
        filtered = FilterFromPivotAndAngles(filtered);
        int myPos = Array.IndexOf(filtered, this) + 2;


        Vector2 finalPosition = RealPointGivenAngle(BaseAngle(snapshot) * myPos, pivot1.transform.position, new Sec(snapshot));


        float virtualX = GetVirtualX(finalPosition, xAxis);
        float virtualY = GetVirtualY(pivot1, xAxis);
        Vector2 virtualPos = new Vector2(virtualX, virtualY);
        Vector2 realPos = ToRealPoint(xAxis, virtualPos);

        return new PositionAndColor(realPos, State.INTERNAL);
    }

    private PositionAndColor ComputeC3_X(Snapshot snapshot, Vector2 xAxis)
    {
        Robot[] mains = GetPivotAndAngles(snapshot);
        Sec sec = new Sec(mains[0], mains[1], mains[2]);
        float radius = sec.radius;

        Vector2 currVirtualPoint = GetVirtualCoordinate(this, xAxis);
        float ySign = currVirtualPoint.y / Mathf.Abs(currVirtualPoint.y);

        float virtualX = currVirtualPoint.x;
        float virtualY = ySign * Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(virtualX, 2));

        Vector2 virtualPoint = new Vector2(virtualX, virtualY);
        Vector2 p = ToRealPoint(xAxis, virtualPoint);
        return new PositionAndColor(p, State.SEC);
    }

    void Tick()
    {
        performStep = true;
    }

    private void SetState(State state){
        this.state = state;
        GetComponent<SpriteRenderer>().color = colors[state];
    }

    private void RegisterToClock()
    {
        Clock clock = GetClock();
        clock.OnTickCallBack += Tick;
    }

    private Robot[] SeesFilter(Robot[] robots)
    {
        Robot[] others = RemovedRobot(robots, this);
        Robot[] result = (Robot[])robots.Clone();

        foreach (Robot r in others)
        {
            Robot[] obstructors = Obstructors(this, r, robots);
            if (obstructors.Length > 0)
                result = RemovedRobot(result, r);
        }

        return result;
    }

    private void MoveToward(Vector2 point)
    {
        if (HasArrived(point))
        {
            move = false;
            return;
        }
        float speedScaler;
        float distance = (point - (Vector2)transform.position).magnitude;
        if (distance < slowDownRadius)
            speedScaler = distance;
        else
            speedScaler = 1;
        Vector2 direction = (point - (Vector2)transform.position).normalized;
        transform.Translate(direction * Time.deltaTime * speed * speedScaler);
    }

    public bool HasArrived(Vector2 point)
    {
        return (point - (Vector2)transform.position).magnitude < spacePrecision;
    }
}
