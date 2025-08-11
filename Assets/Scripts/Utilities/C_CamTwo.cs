using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_CamTwo : MonoBehaviour
{
    [SerializeField] private float      _moveSpeed          = 25f;
    [SerializeField] private float      _moveAdhesion       = 1f;
    [SerializeField] private float      _rotateSpeed        = 75f;
    [SerializeField] private float      _rotateAdhesion     = 1f;
    
    [SerializeField] private Camera     _cam;

    [SerializeField] private Transform  _baseTransform;

    [SerializeField] private bool       _report             = false;

    private Transform                   _camTransform;

    private Vector3                     _posApch;
    private Vector2                     _rotApch;

    private float                       _clock;

    //[]    Auto Functions
    private void Awake()
    {
        Setup();
    }
    private void Update()
    {
        RunFrame();
        UpdateClock();
        FixAngles();
    }

    //[]    Private Functions
    private void Setup()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;
        _camTransform = _cam.transform;

        if (_baseTransform == null)
        {
            var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObj.GetComponent<MeshFilter>().mesh = null;
            baseObj.GetComponent<MeshCollider>().sharedMesh = null;
            baseObj.name = "CamBase";
            _baseTransform = baseObj.transform;
        }

        _camTransform.parent = _baseTransform;
        _camTransform.localPosition = Vector3.zero;
        _camTransform.eulerAngles = new Vector3(_camTransform.eulerAngles.x, _baseTransform.eulerAngles.y, _baseTransform.eulerAngles.z);

        _posApch = _baseTransform.position;
        _rotApch = new Vector2(_camTransform.eulerAngles.x, _baseTransform.eulerAngles.y);
    }
    private void UpdateClock()
    {
        _clock += Time.deltaTime;
        if (_clock > 1 )
        {
            _clock -= 1;
            if (_report) Debug.Log($"STATUS:\nGoto location: " +
                $"x={_posApch.x} " +
                $"y={_posApch.y} " +
                $"z={_posApch.z}\nGoto rotation: " +
                $"x={_rotApch.x}" +
                $"y={_rotApch.y}");
        }
    }
    private void RunFrame()
    {
        SetApproaches();
        Approach();
    }
    private void SetApproaches()
    {
        var moveSpeed = _moveSpeed * Time.deltaTime;
        var rotateSpeed = _rotateSpeed * Time.deltaTime;

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (shift)
        {
            _rotApch += new Vector2(
                rotateSpeed * LRVal(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S)),
                rotateSpeed * LRVal(Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D)));
            _rotApch = new Vector2(Mathf.Clamp(_rotApch.x, -88, 88), _rotApch.y);
            return;
        }
        if (ctrl)
        {
            _posApch += moveSpeed * LRVal(Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D)) * _camTransform.forward.normalized;
            _posApch += new Vector3(0, moveSpeed * LRVal(Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.W)), 0);
            return;
        }
        _posApch += moveSpeed * LRVal(Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D)) * _baseTransform.right.normalized;
        _posApch += moveSpeed * LRVal(Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.W)) * _baseTransform.forward.normalized;
    }
    private void Approach()
    {
        var here = _baseTransform.position;

        _baseTransform.position += Move3(here, _posApch);
        _baseTransform.eulerAngles += new Vector3(0, Move(_baseTransform.eulerAngles.y, _rotApch.y, true), 0);
        _camTransform.eulerAngles += new Vector3(Move(_camTransform.eulerAngles.x, _rotApch.x, true), 0, 0);
    }
    private float Move(float start, float end, bool isAngle)
    {
        float dif = isAngle ? AngleDif(end, start) : (end - start);
        float adhesion = Time.deltaTime * (isAngle ? _rotateAdhesion : _moveAdhesion);
        if (adhesion > 1) adhesion = 1;
        return adhesion * dif;
    }
    private float AngleDif(float a, float b)
    {
        var retVal = a - b;
        if (retVal * Sign(retVal) > 180) retVal = (retVal < 0 ? retVal + 360 : retVal - 360);
        return retVal;
    }
    private Vector3 Move3(Vector3 start, Vector3 end)
    {
        var moveV = end - start;
        float adhesion = Time.deltaTime * _moveAdhesion;
        if (adhesion > 1) adhesion = 1;
        return adhesion * moveV;
    }
    /// <summary>
    /// LRVal returns -1 if 'L' is pressed, 1 if 'R' is pressed, and 0 if both or neither are pressed.
    /// </summary>
    /// <returns></returns>
    private int LRVal(bool L, bool R) => (L ? -1 : 0) + (R ? 1 : 0);
    private int Sign(float i) => (i < 0) ? -1 : 1;
    private void FixAngles()
    {
        var xSol = new Vector2(360, 0);
        var ySol = new Vector2(0, 360);

        if (_camTransform.eulerAngles.x < 0 || _rotApch.x < 0)    { _camTransform.eulerAngles += (Vector3)xSol; _rotApch += xSol; }
        if (_camTransform.eulerAngles.x > 360 || _rotApch.x > 360)      { _camTransform.eulerAngles -= (Vector3)xSol; _rotApch -= xSol; }

        if (_baseTransform.eulerAngles.y < 0 || _rotApch.y < 0) { _baseTransform.eulerAngles += (Vector3)ySol; _rotApch += ySol; }
        if (_baseTransform.eulerAngles.y > 360 || _rotApch.y > 360) { _baseTransform.eulerAngles -= (Vector3)ySol; _rotApch -= ySol; }
    }
}
