using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Executor : MonoBehaviour
{
    //[][] Resources
    [SerializeField] H_WorldGenerator   _worldGenerator;
    [SerializeField] WorldTempLoader    _worldTempLoader;

    //[][] Auto Functions
    private void Start()
    {
        PerformTempActions();
        BeginWorld();
    }

    //[][] Temp Functions
    private void PerformTempActions()
    {
        D_WorldParams._worldSeed = 67896;
    }
    private void BeginWorld()
    {
        if (_worldTempLoader != null) _worldTempLoader.LoadObjectAsScape();
        if (_worldGenerator != null) _worldGenerator.GenerateWorld();
        U_Clock.SetToRun(true);
    }
}
