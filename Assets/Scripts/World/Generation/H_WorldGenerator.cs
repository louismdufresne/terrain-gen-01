using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class H_WorldGenerator : MonoBehaviour
{
    //[][] Resources
    [SerializeField] private H_ChunkKeeper _chunkKeeper;
    [SerializeField] private H_ScapeKeeper _scapeKeeper;

    //[][] Technical
    private int[]                   _centerChunkCoord = new int[] { 0, 0, 0 };
    private List<DO_ChunkElevation> _chunkElevations = new List<DO_ChunkElevation>();
    private List<int[]>             _chunkCoordsToLoad;
    private int                     _chunkCoordIndex = 0;
    private bool                    _doCCL = false;

    //[][] Parameters
    private static readonly int r_chunkElevationsListCap = 400;

    //[][] Auto Functions
    private void OnEnable()
    {
        U_Clock.OneSecondTickEvent += CCL_CheckForNeedsReset;
        U_Clock.OneSecondTickEvent += CCL_CleanLists;
    }
    private void OnDisable()
    {
        U_Clock.OneSecondTickEvent -= CCL_CheckForNeedsReset;
        U_Clock.OneSecondTickEvent -= CCL_CleanLists;
    }
    private void Update()
    {
        CCL_Continue();
    }

    //[][] Public Functions
    public void GenerateWorld()
    {
        CCL_Reset();
    }

    //[][] Private Functions
    private void TestGeneration()
    {
        DO_Chunk loneChunk = new DO_Chunk(new int[] { 0, 0, 0 });
        loneChunk.FloodWithType(R_BlockData.BlockType.Air);
        for (int i = 0; i < loneChunk._blockMatrix.Length; i++)
        {
            for (int j = 0; j < loneChunk._blockMatrix[0].Length; j++)
            {
                for (int k = 0; k < loneChunk._blockMatrix[0][0].Length; k++)
                {
                    if (Random.value < 0.10f) loneChunk._blockMatrix[i][j][k] = R_BlockData.BlockType.Grass;
                    if (Random.value < 0.10f) loneChunk._blockMatrix[i][j][k] = R_BlockData.BlockType.Dirt;
                    if (Random.value < 0.10f) loneChunk._blockMatrix[i][j][k] = R_BlockData.BlockType.Stone;
                }
            }
        }
        _chunkKeeper.LogLoadedChunkFromData(loneChunk);
    }
    /// <summary>
    /// This function was written prior to certain changes made to the functionalities of SmartTri and SmartVert.
    /// It will still work, but performs redundant operations.
    /// </summary>
    private void TestGeneration2()
    {
        float temp = Time.realtimeSinceStartup;
        //[][] Oh man - setting the triangles and vertices manually
        DO_Scape testScape = new DO_Scape(32, Vector3.zero);
        List<DO_SmartVert> verts = new List<DO_SmartVert>();
        List<DO_SmartTri> tris = new List<DO_SmartTri>();

        verts.Add(new DO_SmartVert(new Vector3(0, 0f, 0)));
        verts.Add(new DO_SmartVert(new Vector3(0, 24f, 32)));
        verts.Add(new DO_SmartVert(new Vector3(32, 36f, 32)));
        verts.Add(new DO_SmartVert(new Vector3(32, 24f, 0)));

        tris.Add(new DO_SmartTri());
        tris.Add(new DO_SmartTri());

        tris[0].SetVerts(new List<DO_SmartVert> { verts[0], verts[1], verts[2] });
        tris[1].SetVerts(new List<DO_SmartVert> { verts[2], verts[3], verts[0] });

        verts[0]._sTriangles.Add(tris[0]);
        verts[1]._sTriangles.Add(tris[0]);
        verts[2]._sTriangles.Add(tris[0]);
        verts[2]._sTriangles.Add(tris[1]);
        verts[3]._sTriangles.Add(tris[1]);
        verts[0]._sTriangles.Add(tris[1]);

        testScape.LogSmartTri(tris[0]);
        testScape.LogSmartTri(tris[1]);

        //[][] Log scape - tests the scape logging / retrieval system
        _scapeKeeper.LogScape(testScape);
        List<DO_Scape> retrievedScapes = _scapeKeeper.ScapesAt(Vector3.zero);

        List<DO_Chunk> chunks = VerticalColumnAlpha(0, 0, retrievedScapes);
        for (int i = 0; i < chunks.Count; i++)
        {
            _chunkKeeper.LogLoadedChunkFromData(chunks[i]);
        }

        temp = Time.realtimeSinceStartup - temp;
        Debug.Log($"Chunk generation in {temp} seconds.");
    }
    /// <summary>
    /// This function was written prior to certain changes made to the functionalities of SmartTri and SmartVert.
    /// It will still work, but performs redundant operations.
    /// </summary>
    private void TestCCLGenerationSetup1()
    {
        //[][] Setting the triangles and vertices manually... again
        DO_Scape testScape = new DO_Scape(512, new Vector3(-256, 0, -256));
        List<DO_SmartVert> verts = new List<DO_SmartVert>();
        List<DO_SmartTri> tris = new List<DO_SmartTri>();

        verts.Add(new DO_SmartVert(new Vector3(-255, 0f, -255)));
        verts.Add(new DO_SmartVert(new Vector3(-255, 0f, 255)));
        verts.Add(new DO_SmartVert(new Vector3(255, 0f, 255)));
        verts.Add(new DO_SmartVert(new Vector3(255, 0f, -255)));
        verts.Add(new DO_SmartVert(new Vector3(0, 40f, 0)));

        tris.Add(new DO_SmartTri());
        tris.Add(new DO_SmartTri());
        tris.Add(new DO_SmartTri());
        tris.Add(new DO_SmartTri());

        tris[0].SetVerts(new List<DO_SmartVert> { verts[0], verts[1], verts[4] });
        tris[1].SetVerts(new List<DO_SmartVert> { verts[1], verts[2], verts[4] });
        tris[2].SetVerts(new List<DO_SmartVert> { verts[2], verts[3], verts[4] });
        tris[3].SetVerts(new List<DO_SmartVert> { verts[3], verts[0], verts[4] });

        for (int i = 0; i < tris.Count; i++)
        {
            for (int j = 0; j < tris[i]._sVertices.Length; j++)
            {
                tris[i]._sVertices[j]._sTriangles.Add(tris[i]);
            }
            testScape.LogSmartTri(tris[i]);
        }

        _scapeKeeper.LogScape(testScape);
    }
    private List<DO_Chunk> VerticalColumnAlpha(int xChunkCoord, int zChunkCoord, List<DO_Scape> scapes)
    {
        //[][] Returns a basic vertical column of chunks for a single chunk [x, z] coord

        //[][] Set location in world coordinates
        Vector3 location = new Vector3(
            xChunkCoord * R_WorldParameters.r_chunkSize[0],
            0,
            zChunkCoord * R_WorldParameters.r_chunkSize[2]
            );
        Vector3 curPoint;
        List<DO_Chunk> retVal = new List<DO_Chunk>();

        //[][] Create chunks
        for (int i = 0; i < R_WorldParameters.r_worldHeightInChunks; i++)
        {
            retVal.Add(new DO_Chunk(new int[] { xChunkCoord, i, zChunkCoord }));
        }

        //[][] Create heights for all block [x, z] coordinates
        int[][] heights = new int[R_WorldParameters.r_chunkSize[0]][];  //[][] X-axis, therefore index 0
        for (int i = 0; i < R_WorldParameters.r_chunkSize[0]; i++)
        {
            heights[i] = new int[R_WorldParameters.r_chunkSize[2]];     //[][] Z-axis, therefore index 2
            for (int j = 0; j < R_WorldParameters.r_chunkSize[2]; j++)
            {
                heights[i][j] = R_WorldParameters.r_defaultElevation;
                for (int k = 0; k < scapes.Count; k++)
                {
                    curPoint = location;
                    curPoint.x += i + R_WorldParameters.r_blockOffsetToCenter[0];
                    curPoint.z += j + R_WorldParameters.r_blockOffsetToCenter[2];
                    heights[i][j] += (int)(scapes[k].ElevationFromWorldPoint(curPoint) + R_WorldParameters.r_heightSampleBias);
                }
            }
        }

        //[][] Determine block type at each point of chunks' block matrices
        int baseChunkHeight;
        int thisHeight;
        int scapeHeight;
        R_BlockData.BlockType typeHere;
        for (int k = 0; k < retVal.Count; k++) 
        {
            baseChunkHeight = k * R_WorldParameters.r_chunkSize[1];

            for (int i = 0; i < heights.Length; i++) 
            {
                for (int j = 0; j < heights[0].Length; j++)
                {
                    scapeHeight = heights[i][j];
                    for (int q = 0; q < R_WorldParameters.r_chunkSize[1]; q++)
                    {
                        thisHeight = baseChunkHeight + q;
                        if (thisHeight > scapeHeight) typeHere = R_BlockData.BlockType.Air;
                        else if (thisHeight > scapeHeight - R_WorldParameters.r_grassDepth)
                            typeHere = R_BlockData.BlockType.Grass;
                        else if (thisHeight > scapeHeight - R_WorldParameters.r_dirtDepth)
                            typeHere = R_BlockData.BlockType.WhiteSand;
                        else typeHere = R_BlockData.BlockType.RedSand;
                        retVal[k]._blockMatrix[i][q][j] = typeHere;
                    }
                }
            }
        }
        return retVal;
    }

    //[][] Continuous Chunk Loading
    private void CCL_Reset()
    {
        CCL_LoadAtNewPosition();

        _doCCL = true;
        _chunkKeeper.RecenterOn(_centerChunkCoord);
    }
    private void CCL_LoadAtNewPosition()
    {
        _centerChunkCoord = CCL_GetCenterChunkCoord();
        _chunkCoordsToLoad?.Clear();
        _chunkCoordsToLoad = CCL_ChunkPositionsToLoad(_centerChunkCoord);
        _chunkCoordIndex = 0;
    }
    private void CCL_CheckForNeedsReset()
    {
        if (!U_Calculator.Int3sEqual(CCL_GetCenterChunkCoord(), _centerChunkCoord)) CCL_Reset();
    }
    private void CCL_Continue()
    {
        if (!_doCCL || _chunkCoordsToLoad == null) return;
        int[] chunkCoordinateToGenerateOrLoad;
        while (_chunkCoordIndex < _chunkCoordsToLoad.Count)
        {
            chunkCoordinateToGenerateOrLoad = _chunkCoordsToLoad[_chunkCoordIndex];
            if ((_chunkKeeper.IsChunkPositionLoadable(chunkCoordinateToGenerateOrLoad))
                && (_chunkKeeper.ChunkAt(chunkCoordinateToGenerateOrLoad) == null))
            {
                CCL_GenerateOrLoadNextChunk(chunkCoordinateToGenerateOrLoad);
                return;
            }
            _chunkCoordIndex++;
        }
        Debug.Log("Falses!");
        _doCCL = false;
    }
    private void CCL_GenerateOrLoadNextChunk(int[] chunkCoord)
    {
        //[][] Will eventually check if chunk is already saved, and load that instead of generating from scratch
        DO_Chunk chunkData = CCL_GenerateNextChunk(chunkCoord);
        _chunkKeeper.LogLoadedChunkFromData(chunkData);
    }
    private DO_Chunk CCL_GenerateNextChunk(int[] chunkCoord)
    {
        Vector3 location = new Vector3(
            chunkCoord[0] * R_WorldParameters.r_chunkSize[0],
            chunkCoord[1] * R_WorldParameters.r_chunkSize[1],
            chunkCoord[2] * R_WorldParameters.r_chunkSize[2]
            );
        DO_Chunk retVal = new DO_Chunk(chunkCoord);

        //[][] Scapes
        if (location.y >= R_WorldParameters.r_defaultElevation - R_WorldParameters.r_chunkSize[1])
        {
            //[][] This chunk is subject to world surface scaping, within a tolerance of 1 chunk
            int[][] elevations = CCL_GetElevationsForChunk(location, chunkCoord);
            CCL_DetermineSurfaceChunk(elevations, retVal);
        }
        else retVal.FloodWithType(R_BlockData.BlockType.Stone);

        return retVal;
    }
    private List<int[]> CCL_ChunkPositionsToLoad(int[] centerChunkCoord)
    {
        //[][] Generates coordinates of a cube of side length 2 * (load radius) + 1; the "1" is the center
        //[][] Generates coordinates in a spiral around the center, thereby generating closer chunks first

        List<int[]> retVal = new List<int[]>();

        /*
        int curIndex;
        int numOfLoops;
        
        for (int i = R_WorldParameters.r_chunkLoadRadius; i >= -R_WorldParameters.r_chunkLoadRadius; i--)
        {
            curIndex = 0;
            for (int m = 0; m <= R_WorldParameters.r_chunkLoadRadius; m++)
            {
                numOfLoops = 2 * curIndex + 1;
                for (int j = 0; j < numOfLoops; j++)
                {
                    for (int k = 0; k < numOfLoops; k++)
                    {
                        //[][] Think of the Cube (the Cube knows all)
                        retVal.Add(new int[] {
                        k - curIndex + centerChunkCoord[0],
                        i + centerChunkCoord[1],
                        j - curIndex + centerChunkCoord[2]
                    });
                    }
                }
                curIndex++;
            }
        }
        */

        
        int t1;
        for (int outRad = 0; outRad <= R_WorldParameters.r_chunkLoadRadius; outRad++)
        {
            t1 = outRad * 2;
            for (int y = 0; y <= t1; y++)
            {
                for (int x = 0; x <= t1; x++)
                {
                    for (int z = 0; z <= t1; z++)
                    {
                        if (x == 0 || x == t1) goto Load;
                        if (y == 0 || y == t1) goto Load;
                        if (z == 0 || z == t1) goto Load;
                        continue;
                    Load:
                        retVal.Add(new int[]{
                            x - outRad + centerChunkCoord[0],
                            y - outRad + centerChunkCoord[1],
                            z - outRad + centerChunkCoord[2]});
                    }
                }
            }
        }
        

        return retVal;
    }
    private int[] CCL_GetCenterChunkCoord()
    {
        return new int[] {
            (int)(D_PlayerInfo.ChunkRenderCenterPosition.x / R_WorldParameters.r_chunkSize[0]),
            (int)(D_PlayerInfo.ChunkRenderCenterPosition.y / R_WorldParameters.r_chunkSize[1]),
            (int)(D_PlayerInfo.ChunkRenderCenterPosition.z / R_WorldParameters.r_chunkSize[2]),
        };
    }
    private int[][] CCL_GetElevationsForChunk(Vector3 location, int[] chunkCoord)
    {
        DO_ChunkElevation curEl = _chunkElevations.FirstOrDefault(x => U_Calculator.Int3sEqual(x._chunkCoord, chunkCoord));
        if (curEl == null)
        {
            curEl = new DO_ChunkElevation(chunkCoord, CCL_CalculateElevationsForChunk(location));
            _chunkElevations.Add(curEl);
        }
        return curEl._chunkElevations;
    }
    private int[][] CCL_CalculateElevationsForChunk(Vector3 location)
    {
        int[][] retVal = new int[R_WorldParameters.r_chunkSize[0]][];
        for (int i = 0; i < R_WorldParameters.r_chunkSize[0]; i++)
        {
            retVal[i] = new int[R_WorldParameters.r_chunkSize[1]];
            for (int j = 0; j < R_WorldParameters.r_chunkSize[1]; j++)
            {
                retVal[i][j] = R_WorldParameters.r_defaultElevation;
            }
        }
        List<DO_Scape> scapes = _scapeKeeper.ScapesAt(location);

        //[][] Simple scaping - just adds all values together for now
        Vector3 curPosition;
        for (int i = 0; i < R_WorldParameters.r_chunkSize[0]; i++)
        {
            for (int j = 0; j < R_WorldParameters.r_chunkSize[1]; j++)
            {
                curPosition = location;
                curPosition.x += (R_WorldParameters.r_widthPerBlock * i) + R_WorldParameters.r_blockOffsetToCenter[0];
                curPosition.z += (R_WorldParameters.r_widthPerBlock * j) + R_WorldParameters.r_blockOffsetToCenter[2];
                for (int k = 0; k < scapes.Count; k++)
                {
                    retVal[i][j] += (int)(
                        scapes[k].ElevationFromWorldPoint(curPosition)
                        + R_WorldParameters.r_heightSampleBias
                        );
                }
            }
        }
        return retVal;
    }
    private void CCL_DetermineSurfaceChunk(int[][] surfaceElevations, DO_Chunk chunk)
    {
        //[][] Determine if chunk is empty (i.e. above all elevations); if so, flood with air and return
        bool isEmpty = true;
        int chunkYPos = chunk._chunkPosition.Item2;
        for (int i = 0; i < surfaceElevations.Length; i++)
        {
            for (int j = 0; j < surfaceElevations.Length; j++)
            {
                isEmpty &= (chunkYPos > surfaceElevations[i][j]);
            }
        }
        chunk._isEmpty = isEmpty;
        if (isEmpty)
        {
            chunk.FloodWithType(R_BlockData.BlockType.Air);
            return;
        }

        //[][] Continue for non-empty chunks
        int yOffset = chunk._chunkPosition.Item2;
        for (int i = 0; i < R_WorldParameters.r_chunkSize[0]; i++)
        {
            for (int j = 0; j < R_WorldParameters.r_chunkSize[1]; j++)
            {
                for (int k = 0; k < R_WorldParameters.r_chunkSize[2]; k++)
                {
                    chunk._blockMatrix[i][j][k] = CCL_DetermineBlock(
                        (j * R_WorldParameters.r_widthPerBlock) + yOffset,
                        surfaceElevations[i][k]);
                }
            }
        }
    }
    private R_BlockData.BlockType CCL_DetermineBlock(int blockY, int surfaceY)
    {
        //[][] Simple, consistent surface generation
        if (blockY > surfaceY) return R_BlockData.BlockType.Air;
        if (blockY > surfaceY - R_WorldParameters.r_grassDepth) return R_BlockData.BlockType.Grass;
        if (blockY > surfaceY - R_WorldParameters.r_dirtDepth) return R_BlockData.BlockType.Dirt;
        return R_BlockData.BlockType.Stone;
    }
    private void CCL_CleanLists()
    {
        for (int i = 0; i < _chunkElevations.Count - r_chunkElevationsListCap; i++)
        {
            _chunkElevations.RemoveAt(0);
        }
    }
}
public class DO_ChunkElevation
{
    public int[] _chunkCoord;
    public int[][] _chunkElevations;

    public DO_ChunkElevation(int[] chunkCoord, int[][] chunkElevations)
    {
        _chunkCoord = chunkCoord;
        _chunkElevations = chunkElevations;
    }
}
