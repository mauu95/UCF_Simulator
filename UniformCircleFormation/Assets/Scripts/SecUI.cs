using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static RobotLogic;
using System.Globalization;

public class SecUI : MonoBehaviour
{
    public LineRenderer circleRenderer;

    Clock clock;
    Snapshot snapshot;
    Robot.SymmetryCase symCase;

    void Start()
    {
        clock = GetClock();

    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Z))
            print(CaseInfo());

        if (Input.GetKeyDown(KeyCode.X))
            print(AnglesInfo());
    }

    string CaseInfo()
    {
        snapshot = new Snapshot(GetAllRobots());
        symCase = CalculateSymmetry(snapshot);
        return "Symmetry Case: " + symCase + "";
    }

    string AnglesInfo()
    {
        string result = "";
        Robot[] robots = SortedByOrientation(GetAllRobots(), Vector2.up);
        foreach (Robot r in robots)
        {
            float angle = Angle(Vector2.up, r.transform.position);
            result += angle.ToString("F1", CultureInfo.InvariantCulture) + ", ";
        }
        return result;
    }

    void DrawCircle(int steps, float radius){
        circleRenderer.positionCount = steps;

        for(int currentStep = 0; currentStep<steps; currentStep++){
            float circumferenceProgress = (float)currentStep/steps;
            float currentRadian = circumferenceProgress * 2 * Mathf.PI;
            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);
            float x = xScaled * radius;
            float y = yScaled * radius;
            Vector3 currentPosition = new Vector3(x,y,0);
            circleRenderer.SetPosition(currentStep,currentPosition);
        }
    }
}
