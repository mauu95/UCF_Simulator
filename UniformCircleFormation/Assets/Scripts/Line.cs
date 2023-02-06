using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public float m,q;

    public Line(Vector2 p1, Vector2 p2)
    {
        float x1, x2, y1, y2;

        x1 = p1.x;
        y1 = p1.y;

        x2 = p2.x;
        y2 = p2.y;

        m = (y1 - y2) / (x1 - x2);
        q = (x1 * y2 - x2 * y1) / (x1 - x2);
    }

    internal Vector2 GetXAxis()
    {
        if (float.IsPositiveInfinity(m))
            return Vector2.up;

        if (float.IsNegativeInfinity(m))
            return Vector2.down;

        return new Vector2(1, m).normalized;
    }
}
