using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Sec
{
    public Vector2 origin;
    public float radius;

    public Sec(Snapshot snapshot)
    {
        Robot[] robots = snapshot.GetRobotsList();
        Vector2 p1 = robots[0].transform.position;
        Vector2 p2 = robots[1].transform.position;
        Vector2 p3 = robots[2].transform.position;
        Initialize(p1, p2, p3);
    }

    public Sec(Robot r1, Robot r2, Robot r3)
    {
        Vector2 p1 = r1.transform.position;
        Vector2 p2 = r2.transform.position;
        Vector2 p3 = r3.transform.position;
        Initialize(p1, p2, p3);
    }

    public Sec(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Initialize(p1, p2, p3);
    }

    private void Initialize(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Tuple<Vector2, float> secData = FindCircle(p1, p2, p3);
        origin = secData.Item1;
        radius = secData.Item2;

        origin = Vector2.zero;
        radius = 7.5f;
    }

    public static Tuple<Vector2, float> FindCircle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float x1 = p1.x;
        float y1 = p1.y;
        float x2 = p2.x;
        float y2 = p2.y;
        float x3 = p3.x;
        float y3 = p3.y;

        float x12 = x1 - x2;
        float x13 = x1 - x3;

        float y12 = y1 - y2;
        float y13 = y1 - y3;

        float y31 = y3 - y1;
        float y21 = y2 - y1;

        float x31 = x3 - x1;
        float x21 = x2 - x1;

        // x1^2 - x3^2
        float sx13 = (float)(Math.Pow(x1, 2) -
                        Math.Pow(x3, 2));

        // y1^2 - y3^2
        float sy13 = (float)(Math.Pow(y1, 2) -
                        Math.Pow(y3, 2));

        float sx21 = (float)(Math.Pow(x2, 2) -
                        Math.Pow(x1, 2));

        float sy21 = (float)(Math.Pow(y2, 2) -
                        Math.Pow(y1, 2));

        float f = ((sx13) * (x12)
                + (sy13) * (x12)
                + (sx21) * (x13)
                + (sy21) * (x13))
                / (2 * ((y31) * (x12) - (y21) * (x13)));
        float g = ((sx13) * (y12)
                + (sy13) * (y12)
                + (sx21) * (y13)
                + (sy21) * (y13))
                / (2 * ((x31) * (y12) - (x21) * (y13)));

        float c = -(float)Math.Pow(x1, 2) - (float)Math.Pow(y1, 2) -
                                    2 * g * x1 - 2 * f * y1;

        // eqn of circle be x^2 + y^2 + 2*g*x + 2*f*y + c = 0
        // where centre is (h = -g, k = -f) and radius r
        // as r^2 = h^2 + k^2 - c
        float h = -g;
        float k = -f;
        float sqr_of_r = h * h + k * k - c;

        // r is the radius
        double r = Math.Round(Math.Sqrt(sqr_of_r), 5);
        return new Tuple<Vector2, float>(new Vector2(h, k), (float)r);
    }
}
