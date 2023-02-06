using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleString : MonoBehaviour
{
    public Robot robot;
    public string clockwise;
    public string counterwise;
    
    public AngleString(Robot robot, Snapshot snapshot)
    {
        this.robot = robot;
        Robot[] robots = snapshot.GetRobotsList();

        Robot[] shadowRobots = RobotLogic.SortedByOrientation(robots, robot.transform.position, Robot.Orientation.COUNTERWISE);

        string clockwise = "";
        string counterwise = "";

        Robot[] headedAndTailed = RobotLogic.HeadAndTail(shadowRobots);

        for(int i = 1; i< shadowRobots.Length + 1; i++)
        {
            Vector2 curr = headedAndTailed[i].transform.position;
            Vector2 next = headedAndTailed[i + 1].transform.position;
            float left = AbsAngle(curr, next);
            counterwise += left.ToString();
        }

        for (int i = shadowRobots.Length + 1; i > 1; i--)
        {
            Vector2 curr = headedAndTailed[i].transform.position;
            Vector2 next = headedAndTailed[i - 1].transform.position;
            float right = AbsAngle(curr, next);
            clockwise += right.ToString();
        }

        this.clockwise = clockwise;
        this.counterwise = counterwise;
    }

    private float AbsAngle(Vector2 from, Vector2 to)
    {
        return Mathf.Round(Mathf.Abs(Vector2.SignedAngle(from, to)));
    }

    public string Min()
    {
        if (clockwise.CompareTo(counterwise) > 0)
            return clockwise;
        return counterwise;
    }


}
