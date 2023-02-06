using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Dropdown selector;
    public string[] optionNames;

    private void Start()
    {
        List<string> options = new List<string>();

        foreach(string opt in optionNames)
            options.Add(opt);

        selector.AddOptions(options);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(selector.value + 1);
    }

    public void Quit()
    {
        Application.Quit();
    }

}