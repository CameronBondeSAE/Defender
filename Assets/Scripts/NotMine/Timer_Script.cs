using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Timer_Script : MonoBehaviour
{
    // Declaration 
    public float timePassed;
    public float time = 5f;
    public bool done;
    
    public delegate void TimerSignature();
    public event TimerSignature TimerDone; 

    void Start()
    {
        timePassed = 0;
        bool timesUp = false;
    }
    
    // Update is called once per frame
    private void Update()
    {
        if (done == true)
        {
            return;
        }

        timePassed += Time.deltaTime; 
        if (timePassed > time)
        {
            TimerDone.Invoke();
            timePassed = 0;
            done = true;
            Debug.Log("Times Up!");
        }
    }
    


    

    
}

// note re-replicate the repo and the script to avoid problems //