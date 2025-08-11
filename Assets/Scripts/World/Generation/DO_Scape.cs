using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Data Object - Scape

//[][] Contains triangles and associated points needed for terrain generation
//[][] Additionally contains bins to hopefully expedite the process of finding specific triangles
public class DO_Scape
{
    //[][] Static Variables
    private static readonly int r_smallBinGridSize = 16;    //[][] Number of bins to a side in the small-bin grid
    private static readonly int r_largeBinGridSize = 4;     //[][] Same but for large-bin grid

    //[][] Public Variables
    public readonly float   _gridWorldWidth;    //[][] grid width in world distance
    public readonly float   _gWWInverse;        //[][] 1 / (grid width in world distance)
    public readonly Vector3 _location;          //[][] World coordinate of the "lower left" corner of bin(s) [0, 0]

    //[][] Private Variables
    private DO_SmartBin[][] _smallBinGrid;      //[][] Each bin holds few points, very close to their actual location
    private DO_SmartBin[][] _largeBinGrid;      //[][] Each bin holds many points, has fewer bins to search
    private DO_SmartBin     _wholeScape;        //[][] Contains all points in scape

    //[][] Constructor
    public DO_Scape(float gridWorldWidth, Vector3 location)
    {
        _gridWorldWidth = gridWorldWidth;
        _gWWInverse     = 1f / gridWorldWidth;
        _location       = location;

        InitiateBins();
    }

    //[][] Public Functions
    public float ElevationFromWorldPoint(Vector3 worldPoint)
    {
        DO_SmartTri tri = TriAtWorldPoint(worldPoint);
        if (tri == null) return 0;
        return YFromTriPointDist(worldPoint, tri);
    }
    public bool IsEmpty() => _wholeScape == null ? true : _wholeScape._smartVerts.Count == 0;
    public bool LogSmartTri(DO_SmartTri tri)
    {
        if (LogSmartTriAsCandidate(tri))
        {
            if (!LogSmartTriVerts(tri))
            {
                PurgeGridOfTri(tri, _smallBinGrid);
                PurgeGridOfTri(tri, _largeBinGrid);
                _wholeScape._smartTriCandidates.Remove(tri);
                return false;
            }
            else return true;
        }
        else return false;
    }
    public string ReportScape()
    {
        string s = "SCAPE REPORT:";
        var tris = new List<DO_SmartTri>();
        foreach (var x in _wholeScape._smartVerts) foreach (var y in x._sTriangles) if (!tris.Contains(y)) tris.Add(y);
        foreach (var t in tris)
        {
            s += $"\nTriangle with points: {t.ReportVerts()}.";
        }
        return s;
    }

    //[][] Private Functions
    private DO_SmartTri TriAtWorldPoint(Vector3 worldPoint)
    {
        //[][] This function became way more efficient after implementing the logging of candidate triangles in bins
        List<DO_SmartTri> retValCandidates = new List<DO_SmartTri>();

        if (!IsWorldCoordInThisScape(worldPoint)) return null;          //[][] Point is outside scape
        if (_wholeScape._smartVerts.Count == 0) return null;            //[][] No triangles here at all

        bool isLargeGrid = false;

        int[] binCoord = WorldToBinCoord(worldPoint, false);
        DO_SmartBin bin = GetBin(binCoord, isLargeGrid);
        if (bin == null) { Debug.Log("Bin is null!"); return null; }    //[][] Not totally sure why this would ever happen

        for (int i = 0; i < bin._smartTriCandidates.Count; i++)
        {
            if (IsInTriLague(worldPoint, bin._smartTriCandidates[i]._sVertices)) retValCandidates.Add(bin._smartTriCandidates[i]);
        }

        //[] The chosen candidate is either a) the only one, or b) the one physically on top
        if (retValCandidates.Count == 0) return null;
        if (retValCandidates.Count == 1) return retValCandidates[0];

        //Debug.Log(retValCandidates.Count);
        DO_SmartTri retVal = retValCandidates[0];
        
        float curElev = YFromTriPointDist(worldPoint, retVal);
        float nextElev;
        for (int i = 1; i < retValCandidates.Count; i++)
        {
            nextElev = YFromTriPointDist(worldPoint, retValCandidates[i]);
            if (nextElev > curElev) {curElev = nextElev; retVal = retValCandidates[i]; }
        }
        
        return retVal;
    }
    private List<DO_SmartVert> SearchGrid(bool isLargeGrid, int[] start)
    {
        //[][] Returns all vertices in the closest "ring" of bins around the starting index that contains any
        List<DO_SmartVert> retVal = new List<DO_SmartVert>();
        DO_SmartBin curBin;
        bool allBinsNull;

        for (int i = 1; i < (isLargeGrid ? r_largeBinGridSize : r_smallBinGridSize); i++)
        {
            allBinsNull = true;             //[][] If this is not set false, likely all indices exceeds grid bounds
            for (int j = 0; j < 2 * i; j++)
            {
                //[][] Hope this is right
                curBin = GetBin(new int[] { start[0] - i, start[1] - i + j }, isLargeGrid);
                if (curBin != null) { retVal.AddRange(curBin._smartVerts); allBinsNull = false; }
                curBin = GetBin(new int[] { start[0] - i + j, start[1] + i }, isLargeGrid);
                if (curBin != null) { retVal.AddRange(curBin._smartVerts); allBinsNull = false; }
                curBin = GetBin(new int[] { start[0] + i, start[1] + i - j }, isLargeGrid);
                if (curBin != null) { retVal.AddRange(curBin._smartVerts); allBinsNull = false; }
                curBin = GetBin(new int[] { start[0] + i - j, start[1] - i }, isLargeGrid);
                if (curBin != null) { retVal.AddRange(curBin._smartVerts); allBinsNull = false; }
            }
            if (allBinsNull || retVal.Count != 0) break;
        }
        return retVal;
    }
    private DO_SmartBin GetBin(int[] binCoords, bool isLargeGrid)
    {
        if (!IsLegalBinCoord(binCoords, isLargeGrid)) return null;
        return (isLargeGrid) ?
            _largeBinGrid[binCoords[0]][binCoords[1]]
            : _smallBinGrid[binCoords[0]][binCoords[1]];
    }
    private bool IsLegalBinCoord(int[] coords, bool isLargeGrid)
    {
        if (coords.Length != 2) return false;
        if (coords[0] < 0 || coords[1] < 0) return false;
        if (coords[0] >= (isLargeGrid ? r_largeBinGridSize : r_smallBinGridSize)
            || coords[1] >= (isLargeGrid ? r_largeBinGridSize : r_smallBinGridSize))
            return false;
        return true;
    }
    private int[] WorldToBinCoord(Vector3 point, bool isLargeGrid)
    {
        return new int[] { (int)((point.x - _location.x) * _gWWInverse /
                (isLargeGrid ? r_largeBinGridSize : r_smallBinGridSize)),
            (int)((point.z - _location.z) * _gWWInverse /
                (isLargeGrid ? r_largeBinGridSize : r_smallBinGridSize)),
            };
    }
    private bool IsWorldCoordInThisScape(Vector3 worldCoord)
    {
        if (worldCoord.x < _location.x || worldCoord.z < _location.z) return false;
        if (worldCoord.x > _location.x + _gridWorldWidth || worldCoord.z > _location.z + _gridWorldWidth) return false;
        return true;
    }
    private bool LogSmartTriAsCandidate(DO_SmartTri tri)
    {
        //[][] Logs the smart triangle in a rectangular section of each grid; triangle may not actually exist in all bins
        Vector3 a = tri._sVertices[0]._vertex;
        Vector3 b = tri._sVertices[1]._vertex;
        Vector3 c = tri._sVertices[2]._vertex;
        float maxX = (a.x > b.x) ? (a.x > c.x ? a.x : c.x) : (b.x > c.x ? b.x : c.x);
        float minX = (a.x < b.x) ? (a.x < c.x ? a.x : c.x) : (b.x < c.x ? b.x : c.x);
        float maxZ = (a.z > b.z) ? (a.z > c.z ? a.z : c.z) : (b.z > c.z ? b.z : c.z);
        float minZ = (a.z < b.z) ? (a.z < c.z ? a.z : c.z) : (b.z < c.z ? b.z : c.z);
        a.x = maxX; a.z = maxZ;
        b.x = minX; b.z = minZ;
        if (!IsWorldCoordInThisScape(a) || !IsWorldCoordInThisScape(b)) return false;

        //[][] Small bin grid
        int[] maxIndex = WorldToBinCoord(a, false);
        int[] minIndex = WorldToBinCoord(b, false);
        DO_SmartBin curBin;
        for (int i = minIndex[0]; i <= maxIndex[0]; i++)
        {
            for (int j = minIndex[1]; j <= maxIndex[1]; j++)
            {
                curBin = GetBin(new int[] { i, j }, false);
                if (curBin == null)                             //[][] Oopsous Doopsous
                {
                    PurgeGridOfTri(tri, _smallBinGrid);
                    return false;
                }
                curBin._smartTriCandidates.Add(tri);
            }
        }

        //[][] Large bin grid
        maxIndex = WorldToBinCoord(a, true);
        minIndex = WorldToBinCoord(b, true);
        for (int i = minIndex[0]; i <= maxIndex[0]; i++)
        {
            for (int j = minIndex[1]; j <= maxIndex[1]; j++)
            {
                curBin = GetBin(new int[] { i, j }, true);
                if (curBin == null)                             //[][] Oopsous Doopsous Twopsous
                {
                    PurgeGridOfTri(tri, _largeBinGrid);
                    PurgeGridOfTri(tri, _smallBinGrid);
                    return false;
                }
                curBin._smartTriCandidates.Add(tri);
            }
        }

        //[][] Whole scape bin
        _wholeScape._smartTriCandidates.Add(tri);
        return true;
    }
    private bool LogSmartTriVerts(DO_SmartTri tri)
    {
        //[][] Assumes smart tri and any associated smart vertices are all legal values and are set up correctly
        if (!LogSmartVert(tri._sVertices[0])) return false;
        if (!LogSmartVert(tri._sVertices[1]))
        {
            UnlogSmartVert(tri._sVertices[0]);
            return false;
        }
        if (!LogSmartVert(tri._sVertices[2]))
        {
            UnlogSmartVert(tri._sVertices[1]);
            UnlogSmartVert(tri._sVertices[0]);
            return false;
        }
        return true;
    }
    private bool LogSmartVert(DO_SmartVert vert)
    {
        if (!IsWorldCoordInThisScape(vert._vertex)) return false;
        if (_wholeScape._smartVerts.Contains(vert)) return true;    //[][] This vert already exists in the system

        DO_SmartBin binSmall, binLarge;
        int[] curIndex;

        curIndex = WorldToBinCoord(vert._vertex, false);
        binSmall = GetBin(curIndex, false);
        if (binSmall == null) return false;
        binSmall._smartVerts.Add(vert);

        curIndex = WorldToBinCoord(vert._vertex, true);
        binLarge = GetBin(curIndex, true);
        if (binLarge == null) { binSmall._smartVerts.Remove(vert); return false; }
        binLarge._smartVerts.Add(vert);

        _wholeScape._smartVerts.Add(vert);
        return true;
    }
    private void UnlogSmartVert(DO_SmartVert vert)
    {
        //[][] Uh oh! Did your triangle partially fall outside the scape?
        Debug.Log("Smart vertex unlogged from scape!");

        DO_SmartBin curBin;
        int[] curIndex;

        curIndex = WorldToBinCoord(vert._vertex, false);
        curBin = GetBin(curIndex, false);
        if (curBin == null) goto UnlogSmartVert_TheFan;       //[][] Something is very wrong if this fires
        curBin._smartVerts.Remove(vert);

    UnlogSmartVert_TheFan:
        curIndex = WorldToBinCoord(vert._vertex, true);
        curBin = GetBin(curIndex, true);
        if (curBin == null) goto UnlogSmartVert_TheOtherFan;  //[][] May as well just give up at this point
        curBin._smartVerts.Remove(vert);

    UnlogSmartVert_TheOtherFan:
        _wholeScape._smartVerts.Remove(vert);
    }
    private void PurgeGridOfTri(DO_SmartTri tri, DO_SmartBin[][] grid)
    {
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid[0].Length; j++)
            {
                grid[i][j]._smartTriCandidates.Remove(tri);
            }
        }
    }
    private void InitiateBins()
    {
        _smallBinGrid   = new DO_SmartBin[r_smallBinGridSize][];
        _largeBinGrid   = new DO_SmartBin[r_largeBinGridSize][];

        //[][] Populate small grid
        for (int i = 0; i < r_smallBinGridSize; i++)
        {
            _smallBinGrid[i]    = new DO_SmartBin[r_smallBinGridSize];

            for (int j = 0; j < r_smallBinGridSize; j++)
            {
                _smallBinGrid[i][j] = new DO_SmartBin(null, null);
            }
        }

        //[][] Populate large grid
        for (int i = 0; i < r_largeBinGridSize; i++)
        {
            _largeBinGrid[i] = new DO_SmartBin[r_largeBinGridSize];

            for (int j = 0; j < r_largeBinGridSize; j++)
            {
                _largeBinGrid[i][j] = new DO_SmartBin(null, null);
            }
        }

        //[][] Create whole scape bin
        _wholeScape = new DO_SmartBin(null, null);
    }

    //[][] Funcions to determine if point is in triangle
    #region IsInTriAlphaBeta
    public static bool IsInTriAlpha(Vector3 subject, DO_SmartVert[] triVerts)
    {
        //[][] Original attempt at calculating if a subject point lies within a given triangle
        Vector3 vertA, vertB, vertC;
        float lineM, lineB;
        for (int i = 0; i < 3; i++)
        {
            vertA = triVerts[i]._vertex;
            vertB = triVerts[(i + 1) % 3]._vertex;
            vertC = triVerts[(i + 2) % 3]._vertex;
            if (vertB.x == vertA.x) vertB.x += 0.00001f;            //[][] Because this function needs to be even slower
            lineM = (vertB.z - vertA.z) / (vertB.x - vertA.x);      //[][] I am crying because of this division :'(
            lineB = vertA.z - (vertA.x * lineM);
            if (isAboveLine(lineM, lineB, subject) != isAboveLine(lineM, lineB, vertC)) return false;
        }
        return true;
    }
    public static bool IsInTriBeta(Vector3 subject, DO_SmartVert[] triVerts)
    {
        //[][] Operates like IsInTriAlpha; slower, but includes zero divisions
        //[][] Optimal for triangles with perfectly vertical and/or horizontal sides
        float term, tSlope, tInter;
        Vector3 vertA, vertB, vertC;
        for (int i = 0; i < 3; i++)
        {
            vertA = triVerts[i]._vertex;
            vertB = triVerts[(i + 1) % 3]._vertex;
            vertC = triVerts[(i + 2) % 3]._vertex;
            term = vertB.x - vertA.x;
            tSlope = vertB.z - vertA.z;
            tInter = (vertA.z * term) - (vertA.x * tSlope);
            if (isAboveLineT(term, tSlope, tInter, subject) != isAboveLineT(term, tSlope, tInter, vertC)) return false;
        }
        return true;
    }
    private static bool isAboveLineT(float term, float m, float b, Vector3 vQ) => ((m * vQ.x) + b) < vQ.z * term;
    private static bool isAboveLine(float m, float b, Vector3 vQ) => ((m * vQ.x) + b) < vQ.z;
    #endregion
    #region IsInTriLague
    public static bool IsInTriLague(Vector3 p, DO_SmartVert[] triVerts)
    {
        //[][] This function is the fastest implementation (here) for determining if p is in the given triangle
        Vector3 a = triVerts[0]._vertex;
        Vector3 b = triVerts[1]._vertex;
        Vector3 c = triVerts[2]._vertex;
        float czaz = c.z - a.z;
        float w1divisor = ((b.z - a.z) * (c.x - a.x)) - ((b.x - a.x) * czaz);

        //[][] To prevent dividing by zero, simply call a more stable function where such condition isn't possible
        if (w1divisor == 0 || czaz == 0) return IsInTriBeta(p, triVerts);

        float w1 = ((a.x * czaz) + ((p.z - a.z) * (c.x - a.x)) - (p.x * czaz)) / w1divisor;
        if (w1 < 0) return false;
        float w2 = (p.z - a.z - (w1 * (b.z - a.z))) / czaz;
        if (w2 < 0 || (w1 + w2 > 1)) return false;
        return true;
    }
    #endregion
    #region IsInTriArea

    //[][] This function will probably never work
    //[][] Hello, it is later Me, this function works great
    public static bool IsInTriArea(Vector3 p, DO_SmartVert[] triVerts)
    {
        Vector3 a = triVerts[0]._vertex;
        Vector3 b = triVerts[1]._vertex;
        Vector3 c = triVerts[2]._vertex;
        if (ArH(p, a, b) + ArH(p, b, c) + ArH(p, a, c) <= ArH(a, b, c) + 0.0000001f) return true; return false;
    }
    private static float QA(float f) => f < 0 ? -f : f;

    //[][] I derived the following equation BY HAND.  B Y   H A N D
    private static float ArH(Vector3 p, Vector3 q, Vector3 r)
        => QA((((q.z - p.z) * (r.x - p.x)) - ((q.x - p.x) * (r.z - p.z))) * 0.5f);
    #endregion

    //[][] Functions to calculate y-value at triangle xz
    #region YFromTriPointAlpha
    //[][] Calculates the elevation from a given 3D triangle at a given world point
    //[][] Note: this function does not check whether the given point is inside or outside the triangle
    public static float YFromTriPointAlpha(Vector3 p, DO_SmartTri tri)
    {
        //[][] Setup; done outside of goto loop to prevent double-allocation
        int oopsCount = 0;
        int indexMult;
        float cxaxInv, alpha, beta, gamma;
        Vector3 a, b, c;
        bool oopsZero = true;

        //[][] Oops loop - loop is performed if any potential divisor equates to zero
    Oops:
        //[][] Give up; all permutations of triangle points yield uncalculable terms (are all points the same?)
        if (oopsCount >= 6) { Debug.Log("Max attempts reached; y-value cannot be calculated."); return 0; }

        //[][] Properly set a, b, and c
        indexMult = (oopsCount < 3) ? 1 : -1;
        a = tri._sVertices[(oopsZero) ? 0 : ((0 + oopsCount) * indexMult) % 3]._vertex;  //[][] 0, 1, 2, 0, 2, 1
        b = tri._sVertices[(oopsZero) ? 1 : ((1 + oopsCount) * indexMult) % 3]._vertex;  //[][] 1, 2, 0, 2, 1, 0
        c = tri._sVertices[(oopsZero) ? 2 : ((2 + oopsCount) * indexMult) % 3]._vertex;  //[][] 2, 0, 1, 1, 0, 2

        //[][] Main calculations begin
        if (c.x == a.x) { oopsCount++; oopsZero = false; goto Oops; }   //[][] Start over to prevent division by 0
        cxaxInv =  1 / (c.x - a.x);

        //[][] "Condense" triangle
        gamma = (c.z - a.z) * cxaxInv;
        c.z = a.z;
        b.z -= gamma * (b.x - a.x);
        if (b.z == a.z) { oopsCount++; oopsZero = false; goto Oops; }   //[][] Start over to prevent division by 0
        p.z -= gamma * (p.x - a.x);

        //[][] Calculate dual slope formula
        alpha = (c.y - a.y) * cxaxInv;
        beta = (b.y - a.y - (alpha * (b.x - a.x))) / (b.z - a.z);
        return (alpha * (p.x - a.x)) + (beta * (p.z - a.z)) + a.y;
    }
    #endregion
    #region YFromTriPointPlane
    public static float YFromTriPointPlane(Vector3 p, DO_SmartTri tri)
    {
        int oopsCount = 0;
        int indexMult;
        bool oopsZero = true;
        Vector3 a, ab, ac, n;
    Oops:
        if (oopsCount >= 6) return 0;   //[][] Give up
        indexMult = (oopsCount < 3) ? 1 : -1;

        //[][] Main calculations & indexing
        a   = tri._sVertices[(oopsZero) ? 0 : ((0 + oopsCount) * indexMult) % 3]._vertex;
        ab  = tri._sVertices[(oopsZero) ? 1 : ((1 + oopsCount) * indexMult) % 3]._vertex - a;
        ac  = tri._sVertices[(oopsZero) ? 2 : ((2 + oopsCount) * indexMult) % 3]._vertex - a;
        n   = new Vector3(
            (ab.y * ac.z) - (ac.y * ab.z),
            (ac.x * ab.z) - (ab.x * ac.z),
            (ab.x * ac.y) - (ac.x * ab.y));
        if (n.y == 0) { oopsCount++; oopsZero = false; goto Oops; }     //[][] Start over to prevent division by 0
        return ((n.x * a.x + n.y * a.y + n.z * a.z - n.x * p.x - n.z * p.z) / n.y);
    }
    #endregion
    #region YFromTriPointDist
    //[][] New version, based on the Sebastian Lague point-in-triangle formula
    public static float YFromTriPointDist(Vector3 p, DO_SmartTri tri)
    {
        int vtCt = tri._sVertices.Length;
        int oopsCount = 0;
        int indexMult;
        bool oopsZero = true;
        float czaz, w1divisor, w1, w2;
        try
        {

        Oops:
            if (oopsCount >= 6) return 0;   //[][] Give up
            indexMult = (oopsCount < 3) ? 1 : -1;

            //[][] Main calculations & indexing
            Vector3 a = tri._sVertices[(oopsZero) ? 0 : ((0 + oopsCount) * indexMult) % 3]._vertex;
            Vector3 b = tri._sVertices[(oopsZero) ? 1 : ((1 + oopsCount) * indexMult) % 3]._vertex;
            Vector3 c = tri._sVertices[(oopsZero) ? 2 : ((2 + oopsCount) * indexMult) % 3]._vertex;
            czaz = c.z - a.z;
            w1divisor = ((b.z - a.z) * (c.x - a.x)) - ((b.x - a.x) * czaz);

            if (w1divisor == 0 || czaz == 0) { oopsCount++; oopsZero = false; goto Oops; }  //[][] Prevent 0 division

            w1 = ((a.x * czaz) + ((p.z - a.z) * (c.x - a.x)) - (p.x * czaz)) / w1divisor;
            w2 = (p.z - a.z - (w1 * (b.z - a.z))) / czaz;
            return (w1 * (b.y - a.y) + w2 * (c.y - a.y) + a.y);
        } catch (System.Exception e) { Debug.Log(e + $"\nVertex count {vtCt} with oops {oopsCount}." +
            $"\nTriangle has points: {tri.ReportVerts()}; point tested is x={p.x:0.00}, y={p.y:0.00}, z={p.z:0.00}."); return 0; }
    }
    #endregion

}

//[][] Bin for smart objects
public class DO_SmartBin
{
    public List<DO_SmartVert>   _smartVerts;
    public List<DO_SmartTri>    _smartTriCandidates;

    public DO_SmartBin(List<DO_SmartVert> initialVerts, List<DO_SmartTri> initialTris)
    {
        _smartVerts         = initialVerts ?? new List<DO_SmartVert>();
        _smartTriCandidates = initialTris ?? new List<DO_SmartTri>();
    }
}

//[][] Smart Vertex
public class DO_SmartVert
{
    public Vector3 _vertex;
    public List<DO_SmartTri> _sTriangles;

    public DO_SmartVert(Vector3 vertex)
    {
        _sTriangles = new List<DO_SmartTri>();
        _vertex = vertex;
    }
    public bool TriContainsThis(DO_SmartTri tri)
    {
        if (tri._sVertices.Length != 3) return false;
        for (int i = 0; i < tri._sVertices.Length; i++)
        {
            if (tri._sVertices[i] == this)
            {
                return true;
            }
        }
        return false;
    }
    public bool AddTri(DO_SmartTri tri)
    {
        if (TriContainsThis(tri))
        {
            if (!_sTriangles.Contains(tri)) _sTriangles.Add(tri);
            return true;
        }
        return false;
    }

}

//[][] Smart Triangle
public class DO_SmartTri
{
    public DO_SmartVert[] _sVertices;

    public DO_SmartTri()
    {
        _sVertices = new DO_SmartVert[3];
    }
    public bool SetVerts(List<DO_SmartVert> verts)
    {
        if (verts == null) return false;
        if (verts.Count != 3) return false;
        for (int i = 0; i < 3; i++)
        {
            if (verts[i] == null) return false;
        }
        for (int i = 0; i < 3; i++)
        {
            _sVertices[i] = verts[i];
        }
        for (int i = 0; i < 3; i++) if (!_sVertices[i].TriContainsThis(this)) return false;
        for (int i = 0; i < 3; i++) _sVertices[i].AddTri(this);
        return true;
    }
    public string ReportVerts()
    {
        string s = "";
        foreach (var v in _sVertices) s += $"{v._vertex.x:0.00}, {v._vertex.y:0.00}, {v._vertex.z:0.00}; ";
        return s;
    }
}
