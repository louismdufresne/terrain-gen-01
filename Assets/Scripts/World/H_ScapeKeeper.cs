using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


//[][] Handler - Scape Keeper
//[][] Contains, but does not generate, all scapes
public class H_ScapeKeeper : MonoBehaviour
{
    [SerializeField] private bool _reportScapes;
    //[][] Parameters
    private static readonly int r_scapeGridWidthInBins  = 8;
    private static readonly int r_scapeGridBinSize      = 512;  //[][] Bin width in blocks

    //[][] Technical
    private static readonly float r_binPadding = 0.001f;    //[][] Prevents positions at exact increments from falling outside grid

    //[][] Variables
    private float               _inverseBlocksPerBin;
    private float               _inverseBinsPerGrid;
    private List<DO_ScapeGrid>  _allScapes;

    //[][] Auto functions and associated
    private void Awake()
    {
        Setup();
    }
    private void Setup()
    {
        _allScapes = new List<DO_ScapeGrid>();
        _inverseBlocksPerBin = 1f / r_scapeGridBinSize;
        _inverseBinsPerGrid = 1f / r_scapeGridWidthInBins;
    }

    //[][] Public Functions
    public List<DO_Scape> ScapesAt(Vector3 worldCoord)
    {
        worldCoord.x += r_binPadding;
        worldCoord.z += r_binPadding;

        int[] gridCoord = WorldCoordToScapeGridCoord(worldCoord);
        DO_ScapeGrid grid = GetOrCreateGrid(gridCoord, false);
        int[] binCoord = WorldCoordToScapeBinCoord(worldCoord, gridCoord);
        return grid._scapeGrid[binCoord[0]][binCoord[1]];
    }
    public DO_Scape MeshToScape(Mesh mesh, float scale, bool scaleIsRelative, float gridWorldWidth, Vector3 location)
    {
        DO_Scape            retVal  = new DO_Scape(gridWorldWidth, location);
        List<DO_SmartVert>  verts   = new List<DO_SmartVert>();
        List<DO_SmartTri>   tris    = new List<DO_SmartTri>();

        float xMin = 0, xMax = 0, yMin = 0, yMax = 0, zMin = 0, zMax = 0, cur, maxDif;
        foreach (var vert in mesh.vertices)
        {
            cur = vert.x;
            xMin = (xMin > cur ? cur : xMin);
            xMax = (xMax < cur ? cur : xMax);
            cur = vert.y;
            yMin = (yMin > cur ? cur : yMin);
            yMax = (yMax < cur ? cur : yMax);
            cur = vert.z;
            zMin = (zMin > cur ? cur : zMin);
            zMax = (zMax < cur ? cur : zMax);
        }

        maxDif = xMax - xMin;
        cur = yMax - yMin;
        maxDif = maxDif > cur ? maxDif : cur;
        cur = zMax - zMin;
        maxDif = maxDif > cur ? maxDif : cur;
        Debug.Log(maxDif);

        if (scale <= 0) scale = 1;
        if (maxDif != 0) scale = scaleIsRelative ? scale : scale / maxDif;

        foreach (var pt in mesh.vertices) verts.Add(new DO_SmartVert(scale * pt));

        int curInt;
        int vertCt = verts.Count;
        int[] meshTris = mesh.triangles;
        DO_SmartTri curTri;
        for (int i = 0; i < meshTris.Length / 3; i++)
        {
            curInt = i * 3;
            if (vertCt <= meshTris[curInt + 2]) break;

            curTri = new DO_SmartTri();
            if (!curTri.SetVerts(new List<DO_SmartVert> {
                verts[meshTris[curInt]],
                verts[meshTris[curInt + 1]],
                verts[meshTris[curInt + 2]] }))
                continue;
            tris.Add(curTri);
        }

        foreach (var tri in tris) retVal.LogSmartTri(tri);

        if (retVal.IsEmpty()) { Debug.Log("ERROR: H_ScapeKeeper.MeshToScape: Generated scape is empty."); return null; }
        return retVal;
    }
    public void LogScape(DO_Scape toLog)
    {
        Vector3 minPos = toLog._location;
        minPos.x += r_binPadding;
        minPos.z += r_binPadding;
        int[] minGridCoord = WorldCoordToScapeGridCoord(minPos);

        Vector3 maxPos = toLog._location;
        maxPos.x += (toLog._gridWorldWidth - r_binPadding);
        maxPos.z += (toLog._gridWorldWidth - r_binPadding);
        int[] maxGridCoord = WorldCoordToScapeGridCoord(maxPos);
        int[] thisGridCoord;

        int[] curMinBinCoord, curMaxBinCoord;
        for (int i = maxGridCoord[0] - minGridCoord[0]; i <= maxGridCoord[0]; i++)
        {
            for (int j = maxGridCoord[1] - minGridCoord[1]; j <= maxGridCoord[1]; j++)
            {
                thisGridCoord = new int[] { i, j };
                curMinBinCoord = WorldCoordToScapeBinCoord(minPos, thisGridCoord);
                curMaxBinCoord = WorldCoordToScapeBinCoord(maxPos, thisGridCoord);

                //[][] Ugly but necessary; keeps all bin coordinates inside their respective grids
                if (curMinBinCoord[0] < 0) curMinBinCoord[0] = 0;
                if (curMinBinCoord[1] < 0) curMinBinCoord[1] = 0;
                if (curMaxBinCoord[0] > r_scapeGridWidthInBins - 1) curMaxBinCoord[0] = r_scapeGridWidthInBins - 1;
                if (curMaxBinCoord[1] > r_scapeGridWidthInBins - 1) curMaxBinCoord[1] = r_scapeGridWidthInBins - 1;

                for (int k = curMaxBinCoord[0] - curMinBinCoord[0]; k <= curMaxBinCoord[0]; k++)
                {
                    for (int l = curMaxBinCoord[1] - curMinBinCoord[1]; l <= curMaxBinCoord[1]; l++)
                    {
                        GetOrCreateGrid(thisGridCoord, false)._scapeGrid[k][l].Add(toLog);
                    }
                }
            }
        }

        if (_reportScapes) ReportLoggedScape(toLog);
    }

    //[][] Private functions
    private int[] WorldCoordToScapeGridCoord(Vector3 worldCoord)
    {
        return new int[] {
            (int)(worldCoord.x * _inverseBlocksPerBin * _inverseBinsPerGrid),
            (int)(worldCoord.z * _inverseBlocksPerBin * _inverseBinsPerGrid)
        };
    }
    private int[] WorldCoordToScapeBinCoord(Vector3 worldCoord, int[] gridCoord)
    {
        //[][] Index within a given scape grid
        //[][] If worldCoord lies outside the grid, the returned bin coord WILL lay outside the grid as well
        return new int[] {
        (int)(worldCoord.x * _inverseBlocksPerBin) - (gridCoord[0] * r_scapeGridWidthInBins),
        (int)(worldCoord.z * _inverseBlocksPerBin) - (gridCoord[1] * r_scapeGridWidthInBins)
        };
    }
    private DO_ScapeGrid GetOrCreateGrid(int[] gridCoord, bool suppressCreate)
    {
        DO_ScapeGrid grid = _allScapes.FirstOrDefault(
            x => (x._scapeGridCoord[0] == gridCoord[0] && x._scapeGridCoord[1] == gridCoord[1]));
        if (grid == null && !suppressCreate)
        {
            grid = new DO_ScapeGrid(r_scapeGridWidthInBins, gridCoord);
            _allScapes.Add(grid);
        }
        return grid;
    }
    private void ReportLoggedScape(DO_Scape x)
    {
        Debug.Log(x.ReportScape());
    }

}
public class DO_ScapeGrid
{
    //[][] Contains a grid of lists of scapes; covers a large area
    //[][] The idea is to generate more of these as the player moves further from spawn to hold grids
    public List<DO_Scape>[][]   _scapeGrid;
    public readonly int[]       _scapeGridCoord;    //[][] Location in grid space

    public DO_ScapeGrid(int gridSideLength, int[] gridCoord)
    {
        _scapeGrid = new List<DO_Scape>[gridSideLength][];
        for (int i = 0; i < gridSideLength; i++)
        {
            _scapeGrid[i] = new List<DO_Scape>[gridSideLength];
            for (int j = 0; j < gridSideLength; j++)
            {
                _scapeGrid[i][j] = new List<DO_Scape>();
            }
        }
        _scapeGridCoord = gridCoord;
    }
}