using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTempLoader : MonoBehaviour
{
    [SerializeField] private bool           _load;
    [SerializeField] private H_ScapeKeeper  _scapeKeeper;
    [SerializeField] private GameObject     _objMeshToScape;
    [SerializeField] private float          _scapeScale = 64f;
    [SerializeField] private bool           _scaleIsRelative;

    public void LoadObjectAsScape()
    {
        if (!_load) return;

        if (_objMeshToScape == null) { Debug.Log("ERROR: WorldTempLoader.LoadObjectAsScape(): object is null."); return; }

        MeshFilter meshFilter = _objMeshToScape.GetComponent<MeshFilter>();
        if (meshFilter == null) { Debug.Log("ERROR: WorldTempLoader.LoadObjectAsScape(): object has no mesh filter."); return; }

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) { Debug.Log("ERROR: WorldTempLoader.LoadObjectAsScape(): object has no mesh."); return; }

        var scape = _scapeKeeper.MeshToScape(mesh, _scapeScale, _scaleIsRelative, 512, new Vector3(-256, 0, -256));
        if (scape == null) { Debug.Log("ERROR: WorldTempLoader.LoadObjectAsScape(): scape is null."); return; }

        _scapeKeeper.LogScape(scape);
    }
}
