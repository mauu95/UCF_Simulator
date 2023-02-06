using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public Button home;
    public Button reset;

    private void Start()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if(gm != null)
        {
            home.onClick.AddListener(gm.LoadHome);
            reset.onClick.AddListener(gm.RealoadScene);
        }
    }
}
