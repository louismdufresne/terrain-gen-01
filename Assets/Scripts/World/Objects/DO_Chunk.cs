using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DO_Chunk
{
    //[][] Chunk
    public int[] _chunkIndex;   //[][] x, y, z
    public (int, int, int) _chunkPosition;

    //[][] Blocks
    public R_BlockData.BlockType[][][] _blockMatrix;

    //[][] Technical
    public bool _isEmpty = false;

    //[][] Constructor, associated functions
    public DO_Chunk(int[] chunkIndex)
    {
        // Order here is important
        _chunkIndex = chunkIndex;

        _chunkPosition = CalcChunkPosition();
        _blockMatrix = SetupBlockMatrix();
    }

    private (int, int, int) CalcChunkPosition() => (_chunkIndex == null) ? (0, 0, 0) : (
            _chunkIndex[0] * R_WorldParameters.r_chunkSize[0],
            _chunkIndex[1] * R_WorldParameters.r_chunkSize[1],
            _chunkIndex[2] * R_WorldParameters.r_chunkSize[2]);

    private R_BlockData.BlockType[][][] SetupBlockMatrix()
    {
        R_BlockData.BlockType[][][] retVal = new R_BlockData.BlockType[R_WorldParameters.r_chunkSize[0]][][];
        for (int i = 0; i < R_WorldParameters.r_chunkSize[0]; i++)
        {
            retVal[i] = new R_BlockData.BlockType[R_WorldParameters.r_chunkSize[1]][];
            for (int j = 0; j < R_WorldParameters.r_chunkSize[1]; j++)
            {
                retVal[i][j] = new R_BlockData.BlockType[R_WorldParameters.r_chunkSize[2]];
                for (int k = 0; k < R_WorldParameters.r_chunkSize[2]; k++)
                {
                    retVal[i][j][k] = R_BlockData.BlockType.Default;
                }
            }
        }
        return retVal;
    }

    //[][] Public Functions
    public bool IsBlockCoordInChunk(int[] blockCoord, bool isChunkCoord)
    {
        if (blockCoord.Length != 3) return false;
        int[] theoreticalCoord = (isChunkCoord) ? blockCoord : BlockWorldCoordToBlockChunkCoord(blockCoord);
        if (theoreticalCoord[0] < 0 || theoreticalCoord[0] >= _blockMatrix.Length) return false;
        if (theoreticalCoord[1] < 0 || theoreticalCoord[1] >= _blockMatrix[0].Length) return false;
        if (theoreticalCoord[2] < 0 || theoreticalCoord[2] >= _blockMatrix[0][0].Length) return false;
        return true;
    }
    public int[] BlockWorldCoordToBlockChunkCoord(int[] blockCoord)
    {
        if (blockCoord.Length != 3) return null;
        return new int[] {
            blockCoord[0] - (_chunkIndex[0] * R_WorldParameters.r_chunkSize[0]),
            blockCoord[1] - (_chunkIndex[1] * R_WorldParameters.r_chunkSize[1]),
            blockCoord[2] - (_chunkIndex[2] * R_WorldParameters.r_chunkSize[2])
            };
    }
    public int[] BlockChunkCoordToBlockWorldCoord(int[] blockCoord)
    {
        if (blockCoord.Length != 3) return null;
        return new int[]
        {
            blockCoord[0] + (_chunkIndex[0] * R_WorldParameters.r_chunkSize[0]),
            blockCoord[1] + (_chunkIndex[1] * R_WorldParameters.r_chunkSize[1]),
            blockCoord[2] + (_chunkIndex[2] * R_WorldParameters.r_chunkSize[2])
        };
    }
    public Vector3 BlockChunkCoordToBlockWorldPos(int[] blockChunkCoord)
    {
        if (blockChunkCoord.Length != 3) return Vector3.zero;
        return new Vector3(
            blockChunkCoord[0] + _chunkPosition.Item1,
            blockChunkCoord[1] + _chunkPosition.Item2,
            blockChunkCoord[2] + _chunkPosition.Item3
            );
    }
    public void FloodWithType(R_BlockData.BlockType type)
    {
        for (int i = 0; i < _blockMatrix.Length; i++)
        {
            for (int j = 0; j < _blockMatrix[0].Length; j++)
            {
                for (int k = 0; k < _blockMatrix[0][0].Length; k++)
                {
                    _blockMatrix[i][j][k] = type;
                }
            }
        }
    }

}