using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barker : MonoBehaviour
{
    [SerializeField] private float _period = 1f;
    float _clock = 0f;
    ushort _count = 0;

    private void Update()
    {
        _clock += Time.deltaTime;
        if (_clock > _period)
        {
            _clock -= _period;
            _count++;
            Bark();
        }
    }
    private void Bark()
    {
        string bark = "BARK REPORT:\n";
        bark += $"Count: {_count}\n";
        bark += $"Player actual position: {Vec3ToReport(Camera.main.transform.position)}";
        bark += $"Player logged position: {Vec3ToReport(D_PlayerInfo.ChunkRenderCenterPosition)}";

        Debug.Log( bark );
    }
    private string Vec3ToReport(Vector3 toRep)
        => "X:{" + toRep.x.ToString() + "}; " +
        "Y:{" + toRep.y.ToString() + "}; " +
        "Z:{" + toRep.z.ToString() + "}\n";
}
