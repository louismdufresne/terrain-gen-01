using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_BasicCamera : MonoBehaviour
{

    [SerializeField] private Transform _thisCam;
    [SerializeField] private float _camMoveSpeed = 18f;
    [SerializeField] private float _camRotSpeed = 45f;

    private void LateUpdate()
    {
        int vert    = BoolInt(Input.GetKey(KeyCode.W)) - BoolInt(Input.GetKey(KeyCode.S));
        int horiz   = BoolInt(Input.GetKey(KeyCode.D)) - BoolInt(Input.GetKey(KeyCode.A));
        int twist   = BoolInt(Input.GetKey(KeyCode.E)) - BoolInt(Input.GetKey(KeyCode.Q));

        if (Input.GetKey(KeyCode.LeftShift))    CheckRotate(vert, horiz, twist);
        else                                    CheckMove(vert, horiz, twist);
    }
    private void CheckRotate(int vert, int horiz, int twist)
    {
        _thisCam.Rotate(Vector3.right, vert * Time.deltaTime * _camRotSpeed * -1f);
        _thisCam.Rotate(Vector3.forward, twist * Time.deltaTime * _camRotSpeed * -1f);

        float x, z;
        x = _thisCam.rotation.eulerAngles.x;
        z = _thisCam.rotation.eulerAngles.z;

        _thisCam.Rotate(Vector3.up, horiz * Time.deltaTime * _camRotSpeed);
        _thisCam.eulerAngles = new Vector3(x, _thisCam.rotation.eulerAngles.y, z);
    }
    private void CheckMove(int vert, int horiz, int twist)
    {
        _thisCam.Translate(((Vector3.up * vert) + (Vector3.right * horiz) + (Vector3.forward * twist))
            * Time.deltaTime * _camMoveSpeed);
    }
    private int BoolInt(bool b) => b ? 1 : 0;

}
