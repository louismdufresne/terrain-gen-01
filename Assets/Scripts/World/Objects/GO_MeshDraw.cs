using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] GameObject - MeshDraw
//[][] Draws a portion of a chunk when given a mesh
public class GO_MeshDraw : MonoBehaviour
{
    public MeshFilter _meshFilter;
    public MeshRenderer _meshRenderer;
    public MeshCollider _meshCollider;
    private void Awake()
    {
        _meshFilter = this.gameObject.GetComponent<MeshFilter>();
        if (_meshFilter == null) _meshFilter = this.gameObject.AddComponent<MeshFilter>();
        _meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        if (_meshRenderer == null) _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        _meshCollider = this.gameObject.GetComponent<MeshCollider>();
        if (_meshCollider == null) _meshCollider = this.gameObject.AddComponent<MeshCollider>();
    }
    public void SetMesh(Mesh mesh)
    {
        _meshFilter.mesh = mesh;
        _meshRenderer.enabled = true;
    }
    public void Clear() { _meshRenderer.enabled = false; }
}
