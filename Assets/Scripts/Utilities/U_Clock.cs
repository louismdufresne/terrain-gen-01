using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Utility - Clock
//[][] Contains events pertaining to the passage of time
public class U_Clock : MonoBehaviour
{
    //[][] Variables
    private static float    _oneSecondTimer = 0;
    private static bool     _runClock       = false;

    //[][] Auto Functions
    private void Update()
    {
        HandleTimers();
    }

    //[][] Public Functions
    public static void ResetTimers()            { _oneSecondTimer = 0; }
    public static void SetToRun(bool runState)  { _runClock = runState; }

    //[][] Private Functions
    private void HandleTimers()
    {
        if (!_runClock) return;

        //[][] One-second tick
        _oneSecondTimer += Time.deltaTime;
        if (_oneSecondTimer >= 1) { _oneSecondTimer -= 1f; OneSecondTickEventGo(); }
    }

    //[][] Event Template
    public static event System.Action<bool> ExampleEvent;
    public static void ExampleEventGo(bool b) { ExampleEvent?.Invoke(b); }

    //[][] Clock Events
    public static event System.Action OneSecondTickEvent;
    public static void OneSecondTickEventGo() { OneSecondTickEvent?.Invoke(); }
}