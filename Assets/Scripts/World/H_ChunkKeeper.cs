using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Handler - Chunk Keeper

//[][] Responsible for keeping and allowing the intercommunication between chunks, and interfacing with the larger program

public class H_ChunkKeeper : MonoBehaviour
{
    //[][] Prefabs
    [SerializeField] private GO_Chunk       p_chunkGameObjectPrefab;
    [SerializeField] private GO_MeshDraw    p_meshDrawPrefab;

    //[][] Parameters
    [SerializeField] private Material       _blockTextureMaterial;
    [SerializeField] private ushort         _startingPooledChunks       = 2000;
    [SerializeField] private ushort         _startingPooledMeshDraws    = 12000;

    //[][] Utilities
    private DO_ChunkStore       _chunks;
    private List<GO_Chunk>      _chunkPool;
    private List<GO_MeshDraw>   _meshDrawPool;

    //[][] Technical
    [SerializeField] private GameObject _chunkPoolParent;
    [SerializeField] private GameObject _chunkParent;
    public List<bool[][][]>             _doDraws;

    //[][] Auto Functions
    private void Awake()
    {
        Setup();
    }
    private void Update()
    {
        CheckForUnloadChunks();
    }

    //[][] Public Functions
    public void LogLoadedChunkFromUnit(DO_ChunkStoreUnit chunk)
    {
        if (chunk == null)
        {
            Debug.Log("Chunk load failed at keeper: chunk parameter null");
            return;
        }
        if (chunk._chunkDataObject != null)
        {
            if (chunk._chunkGameObject == null) return;
            _chunks.LogChunk(chunk);

            //[] If chunk is empty, turn it off for now
            chunk._chunkGameObject.gameObject.SetActive(!chunk._chunkDataObject._isEmpty);

            return;
        }
        Debug.Log("Chunk load failed at keeper: data object field of chunk parameter null");
    }
    public void LogLoadedChunkFromData(DO_Chunk chunkData)
    {
        DO_ChunkStoreUnit chunk = new DO_ChunkStoreUnit
        {
            _chunkDataObject = chunkData
        };

        GO_Chunk chunkGameObject = GetPooledChunk();
        chunkGameObject.Setup(this);

        chunkGameObject.LoadChunk(chunkData);
        chunk._chunkGameObject = chunkGameObject;

        LogLoadedChunkFromUnit(chunk);
    }
    public DO_ChunkStoreUnit ChunkAt(int[] chunkPos) => _chunks.ChunkAt(chunkPos);
    public R_BlockData.BlockType BlockInChunk(int[] blockWorldCoord, int[] chunkCoord)
    {
        DO_ChunkStoreUnit chunk = ChunkAt(chunkCoord);
        if (chunk == null) return R_BlockData.BlockType.Default;
        return chunk._chunkGameObject.BlockAtWorldCoord(blockWorldCoord);

    }
    public bool AreSurroundingChunksLoaded(int[] chunkCoord)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (i != 0 && j != 0 && k != 0)
                    {
                        if (ChunkAt(new int[] { chunkCoord[0] + i, chunkCoord[1] + j, chunkCoord[2] + k }) == null) return false;
                    }
                }
            }
        }
        return true;
    }
    public void RecenterOn(int[] chunkCoord)
    {
        _chunks.RecenterOn(chunkCoord);
    }
    public bool IsChunkPositionLoadable(int[] chunkCoord) => _chunks.IsChunkCoordInMatrixBounds(chunkCoord);
    public GO_MeshDraw GetPooledMeshDraw()
    {
        GO_MeshDraw retVal;
        if (_meshDrawPool.Count == 0) retVal = Instantiate(p_meshDrawPrefab);
        else
        {
            retVal = _meshDrawPool[_meshDrawPool.Count - 1];
            _meshDrawPool.RemoveAt(_meshDrawPool.Count - 1);
        }
        retVal.gameObject.SetActive(true);
        retVal._meshRenderer.material = _blockTextureMaterial;
        return retVal;
    }
    public void PoolMeshDraw(GO_MeshDraw toPool)
    {
        toPool.Clear();
        toPool.gameObject.SetActive(false);
        _meshDrawPool.Add(toPool);
        toPool.transform.parent = _chunkPoolParent.transform;
    }

    //[][] Private Functions
    private void Setup()
    {
        _chunks = new DO_ChunkStore();
        _chunkPool = new List<GO_Chunk>();
        _meshDrawPool = new List<GO_MeshDraw>();

        //[][] Fill pools
        for (int i = 0; i < _startingPooledChunks; i++)
        {
            PoolChunk(Instantiate(p_chunkGameObjectPrefab));
        }
        for (int i = 0; i < _startingPooledMeshDraws; i++)
        {
            PoolMeshDraw(Instantiate(p_meshDrawPrefab));
        }

        //[][] Initialize this thing
        _doDraws = new List<bool[][][]>();
        for (int q = 0; q < 6; q++)
        {
            _doDraws.Add(new bool[R_WorldParameters.r_chunkSize[0]][][]);
            for (int i = 0; i < R_WorldParameters.r_chunkSize[0]; i++)
            {
                _doDraws[q][i] = new bool[R_WorldParameters.r_chunkSize[1]][];
                for (int j = 0; j < R_WorldParameters.r_chunkSize[1]; j++)
                {
                    _doDraws[q][i][j] = new bool[R_WorldParameters.r_chunkSize[2]];
                }
            }
        }
    }
    private GO_Chunk GetPooledChunk()
    {
        GO_Chunk retVal;
        if (_chunkPool.Count == 0) retVal = Instantiate(p_chunkGameObjectPrefab);
        else
        {
            retVal = _chunkPool[_chunkPool.Count - 1];
            _chunkPool.RemoveAt(_chunkPool.Count - 1);
        }
        retVal.gameObject.SetActive(true);
        retVal.transform.parent = _chunkParent.transform;
        return retVal;
    }
    private void PoolChunk(GO_Chunk toPool)
    {
        toPool.UnloadChunk();
        toPool.gameObject.SetActive(false);
        _chunkPool.Add(toPool);
        toPool.transform.parent = _chunkPoolParent.transform;
    }
    private void CheckForUnloadChunks()
    {
        //[][] For now, just get rid of all chunks set to be unloaded
        for (int i = 0; i < _chunks._chunksToUnload.Count; i++)
        {
            DO_ChunkStoreUnit curChunk = _chunks._chunksToUnload[i];
            PoolChunk(curChunk._chunkGameObject);
        }
        _chunks._chunksToUnload.Clear();
    }
}

//[][] Data Object - Chunk Store

//[][] Contains, and performs functions related to, chunks

public class DO_ChunkStore
{
    //[][] Data
    public int[]                        _centerChunkCoord;  // In chunk coordinate space
    public DO_ChunkStoreUnit[][][][]    _chunks;
    public List<DO_ChunkStoreUnit>      _chunksToUnload;

    //[][] Technical
    private int     _chunkMatrixInUse = 0;
    private int[]   _indexingBias;

    //[][] Constructor
    public DO_ChunkStore()
    {
        CreateChunkGrid();
    }

    //[][] Public Functions
    public void RecenterOn(int[] chunkCoordinate)
    {
        //[][] This function alters the indices of each chunk store unit inside of the chunk 3D array
        //[][] The chunk with the given coordinates becomes the new central chunk
        //[][] The currently used chunk matrix is swapped because chunks are written into whichever matrix is hitherto unused

        //[][] If no center yet defined, make this coordinate center and return (if no center is defined, no chunks are present)
        if (_centerChunkCoord == null)
        {
            _centerChunkCoord = chunkCoordinate;
            return;
        }

        int[] slide = new int[] {
            _centerChunkCoord[0] - chunkCoordinate[0],
            _centerChunkCoord[1] - chunkCoordinate[1],
            _centerChunkCoord[2] - chunkCoordinate[2]};
        _centerChunkCoord = chunkCoordinate;

        for (int i = 0; i < _chunks.Length; i++)
        {
            for (int j = 0; j < _chunks[i].Length; j++)
            {
                for (int k = 0; k < _chunks[i][j].Length; k++)
                {
                    if (_chunks[i][j][k][These()] == null) goto Skip;

                    //[][] Check for in range
                    if (i + slide[0] < 0 || i + slide[0] >= _chunks.Length
                        || j + slide[1] < 0 || j + slide[1] >= _chunks.Length
                        || k + slide[2] < 0 || k + slide[2] >= _chunks.Length)
                    {
                        //[][] Chunk new position is OUT OF RANGE
                        _chunksToUnload.Add(_chunks[i][j][k][These()]);
                    }
                    else
                    {
                        //[][] Chunk new position is IN RANGE
                        _chunks[i + slide[0]][j + slide[1]][k + slide[2]][Other()]
                             = _chunks[i][j][k][These()];
                    }

                    _chunks[i][j][k][These()] = null;

                Skip:;
                }
            }
        }

        _chunkMatrixInUse = Other();
    }
    public DO_ChunkStoreUnit ChunkAt(int[] chunkCoord)
    {
        if (_centerChunkCoord == null) return null;     //[][] This happens when no chunks have loaded yet
        if (!IsChunkCoordInMatrixBounds(chunkCoord)) return null;

        int[] matrixCoord = ChunkCoordToMatrixCoord(chunkCoord);
        return _chunks[matrixCoord[0]][matrixCoord[1]][matrixCoord[2]][These()];
    }
    public void LogChunk(DO_ChunkStoreUnit toLog)
    {
        //[][] If first, make center
        if (_centerChunkCoord == null)
        {
            _centerChunkCoord = toLog._chunkDataObject._chunkIndex;
        }

        int[] matrixCoord = ChunkCoordToMatrixCoord(toLog._chunkDataObject._chunkIndex);

        if (IsChunkCoordInMatrixBounds(toLog._chunkDataObject._chunkIndex))
        {
            _chunks[matrixCoord[0]][matrixCoord[1]][matrixCoord[2]][These()] = toLog;
        }

    }
    public bool IsChunkCoordInMatrixBounds(int[] chunkCoord)
    {
        //[][] Checks if the given chunk coordinate would fit in the loaded chunk matrix

        int[] theoreticalMatrixCoord = ChunkCoordToMatrixCoord(chunkCoord);
        if (theoreticalMatrixCoord[0] < 0 || theoreticalMatrixCoord[0] >= _chunks.Length) return false;
        if (theoreticalMatrixCoord[1] < 0 || theoreticalMatrixCoord[1] >= _chunks[0].Length) return false;
        if (theoreticalMatrixCoord[2] < 0 || theoreticalMatrixCoord[2] >= _chunks[0][0].Length) return false;
        return true;
    }

    //[][] Private Functions
    private void CreateChunkGrid()
    {
        //[][] 2R + 1 because the grid must have a center chunk and R chunks in each direction
        //[][] Uses "Unload Radius" because chunks are to be unloaded as they fall outside the array
        int sideLength = 2 * R_WorldParameters.r_chunkUnloadRadius + 1;

        _chunks = new DO_ChunkStoreUnit[sideLength][][][];
        for (int i = 0; i < sideLength; i++)
        {
            _chunks[i] = new DO_ChunkStoreUnit[sideLength][][];
            for (int j = 0; j < sideLength; j++)
            {
                _chunks[i][j] = new DO_ChunkStoreUnit[sideLength][];
                for (int k = 0; k < sideLength; k++)
                {
                    _chunks[i][j][k] = new DO_ChunkStoreUnit[2];
                }
            }
        }

        _chunksToUnload = new List<DO_ChunkStoreUnit>();

        _indexingBias = new int[] {
            R_WorldParameters.r_chunkUnloadRadius,
            R_WorldParameters.r_chunkUnloadRadius,
            R_WorldParameters.r_chunkUnloadRadius };
    }
    private int These() => _chunkMatrixInUse;
    private int Other() => (_chunkMatrixInUse + 1) % 2;
    private int[] ChunkCoordToMatrixCoord(int[] chunkCoord) => new int[]
        {
            chunkCoord[0] - _centerChunkCoord[0] + _indexingBias[0],
            chunkCoord[1] - _centerChunkCoord[1] + _indexingBias[1],
            chunkCoord[2] - _centerChunkCoord[2] + _indexingBias[2]
        };
}

public class DO_ChunkStoreUnit
{
    public GO_Chunk _chunkGameObject;
    public DO_Chunk _chunkDataObject;
}