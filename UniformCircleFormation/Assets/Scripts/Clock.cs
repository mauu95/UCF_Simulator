using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public delegate void Tick();
    public Tick OnTickCallBack;

    public int currentTick = 0;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && OnTickCallBack != null)
        {
            currentTick++;
            OnTickCallBack.Invoke();
        }
    }
}
