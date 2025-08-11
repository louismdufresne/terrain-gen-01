using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Readonly - Texture Data

//[][] Contains data pertaining to the accessing of textures from texture sheets
//[][] Block-specific texture coordinates are stored in R_BlockData.r_blocks

public static class R_TextureData
{
    //[][] Spritesheet calculation values
    public static readonly int _spritesheetPixelsAcross = 768;
    public static readonly int _spritesheetPixelsPerSprite = 12;    //[][] Innermost 8x8 are the actual texture, rest is padding

    //[][] Texture UV Offsets - values applied to the base texture coordinate (texture space) to generate mesh UVs
    public static Vector2[] r_textureUVOffsets = new Vector2[] {
        new Vector2(0.1667f, 0.1667f),
        new Vector2(0.1667f, 0.8333f),
        new Vector2(0.8333f, 0.8333f),
        new Vector2(0.8333f, 0.1667f),
    };

}
