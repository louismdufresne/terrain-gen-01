using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManip : MonoBehaviour
{
    private TheObj objy;

    private void Awake()
    {
        objy = new TheObj();
        Vector3 localPoint = objy.thePoint;
        localPoint.y = 200;
        Debug.Log($"Points are: {objy.thePoint.x} {objy.thePoint.y} {objy.thePoint.z} in the object; " +
            $"{localPoint.x} {localPoint.y} {localPoint.z} in the local variable.");
    }
}
public class TheObj
{
    public Vector3 thePoint;

    public TheObj()
    {
        thePoint = new Vector3(3, 5, 7);
    }
}
