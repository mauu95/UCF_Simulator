using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placer : MonoBehaviour
{
    public bool random;
    public float[] angles;

    void Start()
    {
        if (FindObjectOfType<Customizer>())
            return;

        Place();
    }

    public void Place()
    {
        Robot[] robots = FindObjectsOfType<Robot>();
        Snapshot snapshot = new Snapshot(robots);
        Sec sec = new Sec(snapshot);

        RobotLogic.SortByOrientation(robots, Vector2.up, Robot.Orientation.COUNTERWISE);

        for (int i = 0; i < robots.Length; i++)
        {
            float angle;
            if (random)
                angle = Random.Range(0, 360);
            else
                angle = angles[i];

            Vector2 pos = RobotLogic.RealPointGivenAngle(angle, Vector2.up, sec);
            robots[i].transform.position = pos;
        }
    }

}
