using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slowtate : MonoBehaviour
{
    Transform thisT;
    Vector3 eulers = Vector3.up * 40;
    private void Awake()
    {
        thisT = transform;
    }
    private void Update()
    {
        thisT.Rotate(Time.deltaTime * eulers);
    }
}
