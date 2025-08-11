using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class R_WorldParameters
{
    //[][] Block Parameters
    public static readonly int      r_widthPerBlock         = 1;        //[][] This is assumed across most of the program to be 1
    public static readonly float[]  r_blockOffsetToCenter   = new float[] { 0.5f, 0.5f, 0.5f };

    //[][] Chunk Parameters
    public static readonly int[]    r_chunkSize             = new int[] { 24, 24, 24 };
    public static readonly int      r_worldHeightInChunks   = 8;
    public static readonly int      r_chunkLoadRadius       = 8;
    public static readonly int      r_chunkUnloadRadius     = 8;

    //[][] Generation Parameters
    public static readonly float    r_heightSampleBias      = -0.1f;
    public static readonly int      r_defaultElevation      = 40;       //[][] In blocks
    public static readonly int      r_grassDepth            = 1;
    public static readonly int      r_dirtDepth             = 5;

}
