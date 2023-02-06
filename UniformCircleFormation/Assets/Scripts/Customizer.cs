using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class Customizer : MonoBehaviour
{
    public GameObject robotPrefab;
    public GameObject robotParent;
    public TMP_InputField input;
    public Placer placer;

    public void StartLevel()
    {
        if (placer.random)
        {
            int n_robot = int.Parse(input.text);
            for(int i = 0; i< n_robot; i++)
            {
                Instantiate(robotPrefab, robotParent.transform);
            }
        }
        else
        {
            Regex rg = new Regex(@"\d+");
            List<float> angles = new List<float>();
            MatchCollection matchedAngles = rg.Matches(input.text);
            for (int count = 0; count < matchedAngles.Count; count++)
            {
                Instantiate(robotPrefab, robotParent.transform);
                angles.Add(float.Parse(matchedAngles[count].Value));
            }
            placer.angles = angles.ToArray();
        }

        placer.Place();
        gameObject.SetActive(false);
    }


}
