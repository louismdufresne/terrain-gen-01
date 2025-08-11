using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Utility - Calculator

//[][] Provides static functions usable throughout the main thread

public class U_Calculator : MonoBehaviour
{
    //[][] Special math operations
    public static int[] AddInt3s(int[] a, int[] b)
    {
        try { return new int[] { a[0] + b[0], a[1] + b[1], a[2] + b[2] }; }
        catch (System.IndexOutOfRangeException) { return new int[] { 0, 0, 0 }; }
    }
    public static bool Int3sEqual(int[] a, int[] b)
    {
        if (a.Length != 3 || b.Length != 3) return false;
        return (a[0] == b[0] && a[1] == b[1] && a[2] == b[2]);
    }

    //[][] Pseudorandom calculation
    //[][] I nabbed these first few functions straight from my space game
    private static readonly uint pr_mult = 1664525;
    private static readonly uint pr_add = 1013904223;
    private static uint pr_seed = 987654321;
    public static void SeedPseudoRandom(uint seed) { pr_seed = seed; }
    public static uint PseudoRandom()
    {
        pr_seed = (uint)(((ulong)pr_seed * pr_mult + pr_add) % (uint.MaxValue + (ulong)1));
        return pr_seed;
    }
    public static float PseudoValue() => PseudoRandom() / (float)uint.MaxValue;

    //[][] Convert world coordinates to fairly unique seed values
    public static uint Int3ToSeed(int[] int3)
    {
        if (int3.Length != 3) return 657967786;     //[][] Why not
        return (uint)(
            ((ulong)int3[0] + 6569) * 105671761
            + (ulong)(int3[1] + 1439) * 216901701
            + (ulong)(int3[2] + 3613) * 392157321
            * (D_WorldParams._worldSeed + 1817));
    }
    public static uint Int2ToSeed(int[] int2)
    {
        if (int2.Length != 2) return 578907438;     //[][] Could be good?
        return (uint)((int2[0] * 1055501 + 7603) + (int2[1] * 1000187 + 5417) * (D_WorldParams._worldSeed + 2621));
    }
}
