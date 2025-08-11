using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GO_Chunk : MonoBehaviour
{
    //[][] Keeping
    [HideInInspector] public H_ChunkKeeper  _keeper;

    //[][] Data
    private DO_Chunk            _chunkData;

    //[][] Rendering
    private List<GO_MeshDraw>   _meshFaces;
    private bool                _meshDirty = false;
    private float               _uvMultiplier;

    //[][] 
    private int[] _leaveOffs = new int[6];

    //[][] Auto Functions
    private void LateUpdate()
    {
        CheckRedrawMeshes();
    }

    //[][] Public Functions

    public void Setup(H_ChunkKeeper keeper)
    {
        _keeper = keeper;
        _uvMultiplier = R_TextureData._spritesheetPixelsPerSprite / (float)R_TextureData._spritesheetPixelsAcross;
    }
    public void LoadChunk(DO_Chunk chunkData)
    {
        _chunkData = chunkData;
        BeginMeshes();
        _meshDirty = true;
    }
    public DO_Chunk UnloadChunk()
    {
        DO_Chunk retVal = _chunkData;
        _chunkData = null;
        EndMeshes();
        return retVal;
    }
    public R_BlockData.BlockType BlockAtWorldCoord(int[] blockPos)
    {
        int[] blockChunkCoord = _chunkData.BlockWorldCoordToBlockChunkCoord(blockPos);
        if (blockChunkCoord == null) return R_BlockData.BlockType.Default;
        return _chunkData._blockMatrix[blockChunkCoord[0]][blockChunkCoord[1]][blockChunkCoord[2]];
    }

    //[][] Private Functions
    private void BeginMeshes()
    {
        //[][] Creates mesh rendering objects
        //[][] There is one mesh for each possible direction (6 total: -z, -x, +z, +x, +y, -y)

        if (_meshFaces != null)
        {
            if (_meshFaces.Count == 6)
            {
                for (int i = 0; i < _meshFaces.Count; i++)
                {
                    _meshFaces[i].gameObject.SetActive(true);
                }
                return;
            }
            else
            {
                for (int i = 0; i < _meshFaces.Count; i++)
                {
                    _meshFaces[i].Clear();
                    _keeper.PoolMeshDraw(_meshFaces[i]);
                }
                _meshFaces.Clear();
            }
        }

        _meshFaces = new List<GO_MeshDraw>();
        GO_MeshDraw current;

        for (int i = 0; i < 6; i++)
        {
            current = _keeper.GetPooledMeshDraw();
            current.name = $"MeshDrawer {i}";
            current.transform.parent = this.transform;
            _meshFaces.Add(current);
        }
    }
    private void EndMeshes()
    {
        if (_meshFaces == null) return;
        if (_meshFaces.Count == 0) return;
        for (int i = 0; i < _meshFaces.Count; i++)
        {
            _meshFaces[i].Clear();
            _meshFaces[i].gameObject.SetActive(false);
        }
    }
    private void CheckRedrawMeshes()
    {
        if (_meshDirty && _keeper.AreSurroundingChunksLoaded(_chunkData._chunkIndex))
        {
            RedrawMeshes();
            _meshDirty = false;
        }
    }
    private bool ShouldDrawBlockFace(int[] blockChunkCoord, int[] nextBlockOffset)
    {
        //[][] Do not draw if block is a non-drawn type
        if (!_chunkData.IsBlockCoordInChunk(blockChunkCoord, true)) return false;
        else if (_chunkData._blockMatrix[blockChunkCoord[0]][blockChunkCoord[1]][blockChunkCoord[2]]
             == R_BlockData.BlockType.Air)
            return false;

        //[][] Do not draw if the block faces a solid block
        int[] nextBlockChunkCoord = U_Calculator.AddInt3s(blockChunkCoord, nextBlockOffset);
        if (_chunkData.IsBlockCoordInChunk(nextBlockChunkCoord, true))
        {
            return _chunkData._blockMatrix[nextBlockChunkCoord[0]][nextBlockChunkCoord[1]][nextBlockChunkCoord[2]]
                 == R_BlockData.BlockType.Air;
        }
        else
        {
            DO_ChunkStoreUnit nextChunk = _keeper.ChunkAt(U_Calculator.AddInt3s(nextBlockOffset, _chunkData._chunkIndex));
            if (nextChunk == null) return true;
            else
            {
                int[] nextBlockWorldCoord = _chunkData.BlockChunkCoordToBlockWorldCoord(nextBlockChunkCoord);
                if (nextBlockWorldCoord == null) return true;
                if (!nextChunk._chunkDataObject.IsBlockCoordInChunk(nextBlockWorldCoord, false)) return true;
                int[] blockInNextChunkCoord = nextChunk._chunkDataObject.BlockWorldCoordToBlockChunkCoord(nextBlockWorldCoord);
                if (blockInNextChunkCoord == null) return true;

                return nextChunk._chunkDataObject._blockMatrix[
                    blockInNextChunkCoord[0]][blockInNextChunkCoord[1]][blockInNextChunkCoord[2]]
                     == R_BlockData.BlockType.Air;

            }
        }
    }
    private bool UnsafeShouldDrawBlockFace(int[] blockChunkCoord, int[] nextBlockOffset)
    {
        //[][] Do not draw if block is a non-drawn type
        if (R_BlockData.r_blocks[_chunkData._blockMatrix[blockChunkCoord[0]][blockChunkCoord[1]][blockChunkCoord[2]]]
             .IsSkipRendering())
            return false;

        //[][] Do not draw if the block faces a solid block
        int[] nextBlockChunkCoord = U_Calculator.AddInt3s(blockChunkCoord, nextBlockOffset);
        if (_chunkData.IsBlockCoordInChunk(nextBlockChunkCoord, true))
        {
            return R_BlockData.r_blocks[
                _chunkData._blockMatrix[nextBlockChunkCoord[0]][nextBlockChunkCoord[1]][nextBlockChunkCoord[2]]]
                .SolidityType() == R_BlockData.BlockSolidityType.SeeThrough;
        }
        else
        {
            DO_ChunkStoreUnit nextChunk = _keeper.ChunkAt(U_Calculator.AddInt3s(nextBlockOffset, _chunkData._chunkIndex));
            if (nextChunk == null) return true;
            else
            {
                int[] nextBlockWorldCoord = _chunkData.BlockChunkCoordToBlockWorldCoord(nextBlockChunkCoord);
                int[] blockInNextChunkCoord = nextChunk._chunkDataObject.BlockWorldCoordToBlockChunkCoord(nextBlockWorldCoord);

                return nextChunk._chunkDataObject._blockMatrix[
                    blockInNextChunkCoord[0]][blockInNextChunkCoord[1]][blockInNextChunkCoord[2]]
                     == R_BlockData.BlockType.Air;

            }
        }
    }
    private bool LightShouldDrawBlockFace(int[] blockChunkCoord, int[] nextBlockOffset)
    {
        //[][] Get type of this block; return if unrendered
        R_BlockData.BlockType thisBlock = _chunkData._blockMatrix[blockChunkCoord[0]][blockChunkCoord[1]][blockChunkCoord[2]];
        if (R_BlockData.r_blocks[thisBlock].IsSkipRendering()) return false;

        //[][] Get type of next block
        int[] nextBlockChunkCoord = U_Calculator.AddInt3s(blockChunkCoord, nextBlockOffset);
        R_BlockData.BlockType nextBlock;
        if (_chunkData.IsBlockCoordInChunk(nextBlockChunkCoord, true))
            nextBlock = _chunkData._blockMatrix[nextBlockChunkCoord[0]][nextBlockChunkCoord[1]][nextBlockChunkCoord[2]];
        else
        {
            DO_ChunkStoreUnit nextChunk = _keeper.ChunkAt(U_Calculator.AddInt3s(nextBlockOffset, _chunkData._chunkIndex));
            if (nextChunk == null) return true;

            int[] offsets = new int[3];
            int cur;
            for (int i = 0; i < 3; i++)
            {
                cur = nextBlockOffset[i];
                offsets[i] = (cur == 0) ? 0 : (
                    (cur == -1) ? R_WorldParameters.r_chunkSize[i] : -R_WorldParameters.r_chunkSize[i]);
            }
            nextBlockChunkCoord = U_Calculator.AddInt3s(nextBlockChunkCoord, offsets);
            nextBlock
                = nextChunk._chunkDataObject._blockMatrix[nextBlockChunkCoord[0]][nextBlockChunkCoord[1]][nextBlockChunkCoord[2]];
        }
        return (R_BlockData.r_blocks[nextBlock].SolidityType() == R_BlockData.BlockSolidityType.SeeThrough);
    }
    private R_BlockData.BlockType BlockAtChunkCoord(int[] blockPos)
    {
        if (_chunkData.IsBlockCoordInChunk(blockPos, true))
        {
            return _chunkData._blockMatrix[blockPos[0]][blockPos[1]][blockPos[2]];
        }
        return R_BlockData.BlockType.Default;
    }

    #region ListBasedRendering
    private void RedrawMeshes()
    {
        List<List<Vector3>> vertices = new List<List<Vector3>>();
        List<List<Vector2>> uvs = new List<List<Vector2>>();
        for (int i = 0; i < 6; i++)
        {
            vertices.Add(new List<Vector3>());
            uvs.Add(new List<Vector2>());
        }

        for (int i = 0; i < _chunkData._blockMatrix.Length; i++)
        {
            for (int j = 0; j < _chunkData._blockMatrix[0].Length; j++)
            {
                for (int k = 0; k < _chunkData._blockMatrix[0][0].Length; k++)
                {
                    AddBlockMesh(new int[] { i, j, k }, vertices, uvs);
                }
            }
        }

        for (int i = 0; i < 6; i++)
        {
            Mesh mesh = new Mesh();
            int c;
            List<int> triangles = new List<int>();
            for (int j = 0; j < vertices[i].Count / 4; j++)
            {
                c = 4 * j;
                triangles.AddRange(new List<int> { c, c + 1, c + 2, c, c + 2, c + 3 });
            }

            mesh.vertices = vertices[i].ToArray();
            mesh.uv = uvs[i].ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            _meshFaces[i]._meshFilter.mesh = mesh;
        }

    }
    private void AddBlockMesh(int[] blockChunkCoord, List<List<Vector3>> vertices,
        List<List<Vector2>> uvs)
    {
        R_BlockData.BlockType block = BlockAtChunkCoord(blockChunkCoord);

        Vector3 blockVertexBasePos = _chunkData.BlockChunkCoordToBlockWorldPos(blockChunkCoord);
        U_Calculator.SeedPseudoRandom(U_Calculator.Int3ToSeed(
            _chunkData.BlockChunkCoordToBlockWorldCoord(blockChunkCoord)));   //[][] Necessary to not scramble chunk on redraw

        //[][] The remainder of this function assumes simple blocks, i.e. 6 identical faces
        for (int i = 0; i < 6; i++)
        {
            if (LightShouldDrawBlockFace(blockChunkCoord, R_BlockData.r_indexOffsetOfBlockTextureBorders[i]))
            {
                List<Vector3> theseVertices = new List<Vector3>
                {
                    blockVertexBasePos + R_BlockData.r_verticesPerFace[i][0],
                    blockVertexBasePos + R_BlockData.r_verticesPerFace[i][1],
                    blockVertexBasePos + R_BlockData.r_verticesPerFace[i][2],
                    blockVertexBasePos + R_BlockData.r_verticesPerFace[i][3],
                };
                List<Vector2> theseUVs = GetUVs(block, i);

                vertices[i].AddRange(theseVertices);
                uvs[i].AddRange(theseUVs);
            }
        }
    }
    private List<Vector2> GetUVs(R_BlockData.BlockType blockType, int side)
    {
        //[][] Returns a set of UVs, given a block to render and its render type parameters
        List<Vector2> all = new List<Vector2>();

        //[][] Rotation calculation
        int rotation = 0;
        if (R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FullRandom
            || R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.RotateOnly)
        {
            rotation = (int)(U_Calculator.PseudoValue() * 4) % 4;
        }
        else if (R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.Rotate180)
        {
            rotation = (int)(U_Calculator.PseudoValue() * 2) * 2;   //[][] Should result in either 0 or 2
        }

        //[][] Coordinate obtaining
        for (int i = 0; i < 4; i++)
        {
            all.Add((R_BlockData.r_blocks[blockType].TextureCoords()[0] + R_TextureData.r_textureUVOffsets[(i + rotation) % 4])
            * _uvMultiplier);
        }

        //[][] Swappy Time (if do)
        //[][] Slight yandereity

        if ((R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FlipHorizOnly
            || R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FlipHorizOrVert
            || R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FullRandom)
            && U_Calculator.PseudoValue() < 0.5f)
        {
            all = Flip(all, true);
        }
        if ((R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FlipHorizOrVert
            || R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FullRandom)
            && U_Calculator.PseudoValue() < 0.5f)
        {
            all = Flip(all, false);
        }
        if (R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FlipOnZ && (side == 1 || side == 3))
        {
            all = Flip(all, true);
        }

        return all;
    }
    private List<Vector2> Flip(List<Vector2> toSwap, bool isHoriz)
    {
        Vector2 temp;
        if (isHoriz)
        {
            temp = toSwap[0];
            toSwap[0] = toSwap[3];
            toSwap[3] = temp;
            temp = toSwap[1];
            toSwap[1] = toSwap[2];
            toSwap[2] = temp;
        }
        else
        {
            temp = toSwap[0];
            toSwap[0] = toSwap[1];
            toSwap[1] = temp;
            temp = toSwap[3];
            toSwap[3] = toSwap[2];
            toSwap[2] = temp;
        }
        return toSwap;
    }
    #endregion

    #region ArrayBasedRendering
    private void RedrawMeshes2()
    {
        int[] blockFacesPerMesh = new int[6];
        bool cur;
        for (int i = 0; i < _chunkData._blockMatrix.Length; i++)
        {
            for (int j = 0; j < _chunkData._blockMatrix[0].Length; j++)
            {
                for (int k = 0; k < _chunkData._blockMatrix[0][0].Length; k++)
                {
                    for (int q = 0; q < 6; q++)
                    {
                        cur = ShouldDrawBlockFace(
                            new int[] { i, j, k },
                            R_BlockData.r_indexOffsetOfBlockTextureBorders[q]);
                        _keeper._doDraws[q][i][j][k] = cur;
                        if (cur) blockFacesPerMesh[q]++;
                    }
                }
            }
        }
        List<Vector3[]> allVerts = new List<Vector3[]>();
        List<Vector2[]> allUVs = new List<Vector2[]>();
        List<int[]> allTris = new List<int[]>();
        for (int i = 0; i < 6; i++)
        {
            allVerts.Add(new Vector3[blockFacesPerMesh[i] * 4]);
            allUVs.Add(new Vector2[blockFacesPerMesh[i] * 4]);
            allTris.Add(new int[blockFacesPerMesh[i] * 6]);
        }

        for (int i = 0; i < _chunkData._blockMatrix.Length; i++)
        {
            for (int j = 0; j < _chunkData._blockMatrix[0].Length; j++)
            {
                for (int k = 0; k < _chunkData._blockMatrix[0][0].Length; k++)
                {
                    AddBlockMeshToArrays(new int[] { i, j, k }, allVerts, allUVs);
                }
            }
        }
        for (int i = 0; i < 6; i++)
        {
            Mesh mesh = new Mesh();
            int c, d;
            for (int j = 0; j < allVerts[i].Length / 4; j++)
            {
                c = 4 * j;
                d = 6 * j;
                allTris[i][d] = c;
                allTris[i][d + 1] = c + 1;
                allTris[i][d + 2] = c + 2;
                allTris[i][d + 3] = c;
                allTris[i][d + 4] = c + 2;
                allTris[i][d + 5] = c + 3;
            }

            mesh.vertices = allVerts[i];
            mesh.uv = allUVs[i];
            mesh.triangles = allTris[i];
            mesh.RecalculateNormals();
            _meshFaces[i].GetComponent<MeshFilter>().mesh = mesh;
        }
    }
    private void AddBlockMeshToArrays(int[] blockChunkCoord, List<Vector3[]> allVertices,
        List<Vector2[]> allUVs)
    {
        R_BlockData.BlockType block = BlockAtChunkCoord(blockChunkCoord);
        Vector3 blockVertexBasePos = _chunkData.BlockChunkCoordToBlockWorldPos(blockChunkCoord);
        U_Calculator.SeedPseudoRandom(U_Calculator.Int3ToSeed(
            _chunkData.BlockChunkCoordToBlockWorldCoord(blockChunkCoord)));   //[][] Necessary to not scramble chunk on redraw

        //[][] The remainder of this function assumes simple blocks, i.e. 6 identical faces
        for (int i = 0; i < 6; i++)
        {
            if (_keeper._doDraws[i][blockChunkCoord[0]][blockChunkCoord[1]][blockChunkCoord[2]])
            {
                allVertices[i][_leaveOffs[i]] = blockVertexBasePos + R_BlockData.r_verticesPerFace[i][0];
                allVertices[i][_leaveOffs[i] + 1] = blockVertexBasePos + R_BlockData.r_verticesPerFace[i][1];
                allVertices[i][_leaveOffs[i] + 2] = blockVertexBasePos + R_BlockData.r_verticesPerFace[i][2];
                allVertices[i][_leaveOffs[i] + 3] = blockVertexBasePos + R_BlockData.r_verticesPerFace[i][3];

                GetUVsToArray(block, i, allUVs[i], _leaveOffs[i]);

                _leaveOffs[i] += 4;
            }
        }
    }
    private void GetUVsToArray(R_BlockData.BlockType blockType, int side, Vector2[] uvs, int h0)
    {
        //[][] Rotation calculation
        int rotation = 0;
        if (R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FullRandom
            || R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.RotateOnly)
        {
            rotation = (int)(U_Calculator.PseudoValue() * 4) % 4;
        }
        else if (R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.Rotate180)
        {
            rotation = (int)(U_Calculator.PseudoValue() * 2) * 2;   //[][] Should result in either 0 or 2
        }

        //[][] Coordinate obtaining
        for (int i = 0; i < 4; i++)
        {
            uvs[i + h0] = (
                R_BlockData.r_blocks[blockType].TextureCoords()[0]
                + R_TextureData.r_textureUVOffsets[(i + rotation) % 4])
                * _uvMultiplier;
        }

        //[][] Swappy Time (if do)
        //[][] Slight yandereity

        if ((R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FlipHorizOnly
            || R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FlipHorizOrVert
            || R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FullRandom)
            && U_Calculator.PseudoValue() < 0.5f)
        {
            Flip2(uvs, true, h0);
        }
        if ((R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FlipHorizOrVert
            || R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FullRandom)
            && U_Calculator.PseudoValue() < 0.5f)
        {
            Flip2(uvs, false, h0);
        }
        if (R_BlockData.r_blocks[blockType].RenderType() == R_BlockData.BlockRenderType.FlipOnZ && (side == 1 || side == 3))
        {
            Flip2(uvs, true, h0);
        }
    }
    private void Flip2(Vector2[] arr, bool isHoriz, int h0)
    {
        Vector2 temp;
        int h1 = h0 + 1;
        int h2 = h0 + 2;
        int h3 = h0 + 3;
        if (isHoriz)
        {
            temp = arr[h0];
            arr[h0] = arr[h3];
            arr[h3] = temp;
            temp = arr[h1];
            arr[h1] = arr[h2];
            arr[h2] = temp;
        }
        else
        {
            temp = arr[h0];
            arr[h0] = arr[h1];
            arr[h1] = temp;
            temp = arr[h3];
            arr[h3] = arr[h2];
            arr[h2] = temp;
        }
    }
    #endregion
}
