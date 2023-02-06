using System;
using System.Collections.Generic;
using UnityEngine;

public class Snapshot
{
    private readonly Robot[] robots;

    public Snapshot(Robot[] robots){
        this.robots = robots;
    }

    public Robot[] GetRobotsList(){
        return robots;
    }

    public int Size()
    {
        return robots.Length;
    }
}
