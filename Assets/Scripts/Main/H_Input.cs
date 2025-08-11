using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Input : MonoBehaviour
{

    private void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }
}
