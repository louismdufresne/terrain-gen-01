using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class U_PlayerTracker : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;

    private void Update()
    {
        D_PlayerInfo.ChunkRenderCenterPosition = _playerTransform.position;
    }
}
