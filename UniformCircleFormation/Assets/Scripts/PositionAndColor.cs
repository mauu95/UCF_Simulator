using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionAndColor
{
    public float x,y;
    public Robot.State state;
    public Vector2 point;
    public PositionAndColor(Vector2 p){
        x = p[0];
        y = p[1];
        state = Robot.State.INITIAL;
        point = p;
    }
    public PositionAndColor(Vector2 p, Robot.State state)
    {
        x = p[0];
        y = p[1];
        this.state = state;
        point = p;
    }
}
