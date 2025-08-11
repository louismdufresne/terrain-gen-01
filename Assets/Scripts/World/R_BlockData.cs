using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Readonly - Block Data

//[][] Contains parameters for all blocks

public static class R_BlockData
{
    //[][] Block information

    //[][] Block Dictionary - contains references to block data given block type
    //[][] This should always be the starting point for implementing new blocks
    //[][] This dictionary now stores texture data

    public static Dictionary<BlockType, RO_BlockInfo> r_blocks
        = new Dictionary<BlockType, RO_BlockInfo>
        {
            { BlockType.Default,        new RO_BlockInfo(
                BlockRenderType.Default,
                BlockSolidityType.Default,
                new Vector2[]{new Vector2(63, 00), },
                false
                )},
            { BlockType.Air,            new RO_BlockInfo(
                BlockRenderType.Default,
                BlockSolidityType.SeeThrough,
                new Vector2[]{new Vector2(63, 63) },
                true
                )},
            { BlockType.Grass,          new RO_BlockInfo(
                BlockRenderType.FullRandom,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(00, 63) },
                false
                )},
            { BlockType.DryGrass,       new RO_BlockInfo(
                BlockRenderType.FullRandom,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(01, 63) },
                false
                )},
            { BlockType.DeadGrass,      new RO_BlockInfo(
                BlockRenderType.FullRandom,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(02, 63) },
                false
                )},
            { BlockType.Dirt,           new RO_BlockInfo(
                BlockRenderType.FullRandom,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(00, 62) },
                false
                )},
            { BlockType.RedSand,        new RO_BlockInfo(
                BlockRenderType.Rotate180,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(00, 61) },
                false
                )},
            { BlockType.WhiteSand,      new RO_BlockInfo(
                BlockRenderType.Rotate180,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(01, 61) },
                false
                )},
            { BlockType.Gravel,         new RO_BlockInfo(
                BlockRenderType.FullRandom,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(00, 60) },
                false
                )},
            { BlockType.RedClay,        new RO_BlockInfo(
                BlockRenderType.FullRandom,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(00, 59) },
                false
                )},
            { BlockType.BlueClay,       new RO_BlockInfo(
                BlockRenderType.FullRandom,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(01, 59) },
                false
                )},
            { BlockType.Stone,          new RO_BlockInfo(
                BlockRenderType.FlipHorizOrVert,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(08, 63) },
                false
                )},
            { BlockType.Limestone,      new RO_BlockInfo(
                BlockRenderType.FlipHorizOnly,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(08, 62) },
                false
                )},
            { BlockType.StoneBrick,     new RO_BlockInfo(
                BlockRenderType.Default,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(24, 63) },
                false
                )},
            { BlockType.StoneSlate,     new RO_BlockInfo(
                BlockRenderType.FlipHorizOrVert,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(25, 63) },
                false
                )},
            { BlockType.StoneCobble,    new RO_BlockInfo(
                BlockRenderType.FullRandom,
                BlockSolidityType.Solid,
                new Vector2[]{new Vector2(26, 63) },
                false
                )},
        };

    //[][] Enums
    public enum BlockType : ushort
    {
        //[][] i.e. Block ID

        Default     = 0,
        Air         = 1,
        Grass       = 2,
        DryGrass    = 3,
        DeadGrass   = 4,
        Dirt        = 10,
        RedSand     = 20,
        WhiteSand   = 21,
        Gravel      = 22,
        RedClay     = 25,
        BlueClay    = 26,
        Stone       = 30,
        Limestone   = 32,
        StoneBrick  = 50,
        StoneSlate  = 51,
        StoneCobble = 52,
    }
    public enum BlockRenderType : ushort
    {
        //[][] Determines what sorts of rotations or flips the texture will be drawn with

        Default             = 0,    //[][] Also serves as "do not alter"
        FullRandom          = 1,
        RotateOnly          = 2,
        Rotate180           = 3,
        FlipHorizOrVert     = 4,
        FlipHorizOnly       = 5,
        FlipOnZ             = 6,
        FullySpecified      = 100,  //[][] Will accompany a "specification" structure of some sort
    }
    public enum BlockSolidityType : ushort
    {
        //[][] Pertains to a number of things about the block, e.g. whether other blocks facing it need to be rendered

        Default             = 0,
        Solid               = 1,
        SeeThrough          = 2,
    }

    //[][] Technical

    //[][] Faces, given looking down w/ x horiz, z vert, y inout: bottom, left, top, right, close, far; all clockw.
    private static float n0 = -0.000f; private static float n1 = 1.000f;
    public static readonly Vector3[][] r_verticesPerFace = new Vector3[][] {
        new Vector3[]{ new Vector3(n0, n0, 0), new Vector3(n0, n1, 0), new Vector3(n1, n1, 0), new Vector3(n1, n0, 0)},
        new Vector3[]{ new Vector3(0, n0, n1), new Vector3(0, n1, n1), new Vector3(0, n1, n0), new Vector3(0, n0, n0)},
        new Vector3[]{ new Vector3(n1, n0, 1), new Vector3(n1, n1, 1), new Vector3(n0, n1, 1), new Vector3(n0, n0, 1)},
        new Vector3[]{ new Vector3(1, n0, n0), new Vector3(1, n1, n0), new Vector3(1, n1, n1), new Vector3(1, n0, n1)},
        new Vector3[]{ new Vector3(n0, 1, n0), new Vector3(n0, 1, n1), new Vector3(n1, 1, n1), new Vector3(n1, 1, n0)},
        new Vector3[]{ new Vector3(n1, 0, n0), new Vector3(n1, 0, n1), new Vector3(n0, 0, n1), new Vector3(n0, 0, n0)},
    };

    //[][] The index of Block B that a given side of Block A faces, relative to Block A
    public static readonly int[][] r_indexOffsetOfBlockTextureBorders = new int[][] {
        new int[] { 0, 0, -1 },
        new int[] { -1, 0, 0 },
        new int[] { 0, 0, 1 },
        new int[] { 1, 0, 0 },
        new int[] { 0, 1, 0 },
        new int[] { 0, -1, 0 },
        };
}

public class RO_BlockInfo
{
    //[][] BlockType is not stored here because it is assumed this object is accessed via dictionary with key BlockType

    //[][] Parameters
    //[][] Private to prevent manipulation
    private R_BlockData.BlockRenderType     _renderType;
    private R_BlockData.BlockSolidityType   _solidityType;
    private Vector2[]                       _textureCoords;
    private bool                            _skipRendering;

    //[][] Constructor
    public RO_BlockInfo(
        R_BlockData.BlockRenderType     renderType,
        R_BlockData.BlockSolidityType   solidityType,
        Vector2[]                       textureCoords,
        bool                            skipRendering
        )
    {
        _renderType     = renderType;
        _solidityType   = solidityType;
        _textureCoords  = textureCoords;
        _skipRendering  = skipRendering;
    }

    //[][] Accessors
    public R_BlockData.BlockRenderType RenderType()     => _renderType;
    public R_BlockData.BlockSolidityType SolidityType() => _solidityType;
    public Vector2[] TextureCoords()                    => _textureCoords;
    public bool IsSkipRendering()                       => _skipRendering;
}
