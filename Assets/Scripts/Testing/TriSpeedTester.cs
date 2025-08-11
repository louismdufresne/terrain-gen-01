using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriSpeedTester : MonoBehaviour
{
    [SerializeField] private bool _doSetSeed;
    [SerializeField] private int _seed;

    private void Awake()
    {
        if (_doSetSeed) Random.InitState(_seed);
        RunPointInTriTest2();
        DoYCalcCheck();
    }

    #region IsPointInTriTests
    private void RunPointInTriTest2()
    {
        Debug.Log("Running point in triangle tests!");
        //[][] A headache to start, but generates the same list for all tests
        int testIterations = 100000;
        int numOfTests = 4;
        DO_SmartVert[][] triVertsArray = new DO_SmartVert[testIterations][];
        Vector3[] pArray = new Vector3[testIterations];
        bool[][] agreements = new bool[testIterations][];
        int[] problemChildren = new int[numOfTests];

        for (int i = 0; i < testIterations; i++)
        {
            triVertsArray[i] = new DO_SmartVert[] {
                new DO_SmartVert(new Vector3(Random.value, Random.value, Random.value)),
                new DO_SmartVert(new Vector3(Random.value, Random.value, Random.value)),
                new DO_SmartVert(new Vector3(Random.value, Random.value, Random.value)) };
            pArray[i] = new Vector3(Random.value, Random.value, Random.value);
            agreements[i] = new bool[numOfTests];
        }

        //[][] Heralded as the default
        float lagueTime = Time.realtimeSinceStartup;
        for (int i = 0; i < testIterations; i++)
        {
            agreements[i][0] = DO_Scape.IsInTriLague(pArray[i], triVertsArray[i]);
        }
        lagueTime = Time.realtimeSinceStartup - lagueTime;

        float alphaTime = Time.realtimeSinceStartup;
        for (int i = 0; i < testIterations; i++)
        {
            agreements[i][1] = DO_Scape.IsInTriAlpha(pArray[i], triVertsArray[i]);
        }
        alphaTime = Time.realtimeSinceStartup - alphaTime;

        float areaTime = Time.realtimeSinceStartup;
        for (int i = 0; i < testIterations; i++)
        {
            agreements[i][2] = DO_Scape.IsInTriArea(pArray[i], triVertsArray[i]);
        }
        areaTime = Time.realtimeSinceStartup - areaTime;

        float betaTime = Time.realtimeSinceStartup;
        for (int i = 0; i < testIterations; i++)
        {
            agreements[i][3] = DO_Scape.IsInTriBeta(pArray[i], triVertsArray[i]);
        }
        betaTime = Time.realtimeSinceStartup - betaTime;

        int numAgree = 0;
        for (int i = 0; i < testIterations; i++)
        {
            if (DoAllAgree(agreements[i]) == -1) numAgree++;
            else
            {
                problemChildren[DoAllAgree(agreements[i])]++;
            }
        }


        Debug.Log($"alphaTime = {alphaTime}; lagueTime = {lagueTime}; areaTime = {areaTime}; " +
            $"betaTime = {betaTime}; {numAgree} agreements out of {testIterations}");
        if (numAgree != testIterations) Debug.Log(
            $"Problems: {problemChildren[1]} Alpha; {problemChildren[2]} Area; {problemChildren[3]} Beta");
    }
    private void Randomize(Vector3 p, DO_SmartVert[] triVerts)
    {
        triVerts[0]._vertex = new Vector3(Random.value, Random.value, Random.value);
        triVerts[1]._vertex = new Vector3(Random.value, Random.value, Random.value);
        triVerts[2]._vertex = new Vector3(Random.value, Random.value, Random.value);
        p = new Vector3(Random.value, Random.value, Random.value);
    }
    private int DoAllAgree(bool[] bools)
    {
        //[][] Returns -1 if all agree; else, returns index of first disagreer
        for (int i = 0; i < bools.Length - 1; i++)
        {
            if (bools[i] != bools[i + 1]) return i + 1;
        }
        return -1;
    }
    #endregion

    private void DoYCalcCheck()
    {
        Debug.Log("Running Y-point in plane tests!");
        int numOfTests = 100000;
        float result;

        DO_SmartVert[] verts = new DO_SmartVert[] {
            new DO_SmartVert(new Vector3(12, 16, -3)),
            new DO_SmartVert(new Vector3(2, -11, 5)),
            new DO_SmartVert(new Vector3(9, 2, 4))
        };
        DO_SmartTri tri = new DO_SmartTri();
        tri._sVertices = verts;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i]._sTriangles.Add(tri);
        }
        Vector3 point = new Vector3(11f, 0f, 1f);

        //[][] Do this correctly, this time
        float elapsedTime;

        //[][] Plane
        elapsedTime = Time.realtimeSinceStartup;
        for (int i = 0; i < numOfTests; i++)
        {
            DO_Scape.YFromTriPointPlane(point, tri);
        }
        elapsedTime = Time.realtimeSinceStartup - elapsedTime;
        Debug.Log($"Plane time: {elapsedTime} seconds for {numOfTests} iterations.");

        //[][] Alpha
        elapsedTime = Time.realtimeSinceStartup;
        for (int i = 0; i < numOfTests; i++)
        {
            DO_Scape.YFromTriPointAlpha(point, tri);
        }
        elapsedTime = Time.realtimeSinceStartup - elapsedTime;
        Debug.Log($"Alpha time: {elapsedTime} seconds for {numOfTests} iterations.");

        //[][] Distance
        elapsedTime = Time.realtimeSinceStartup;
        for (int i = 0; i < numOfTests; i++)
        {
            DO_Scape.YFromTriPointDist(point, tri);
        }
        elapsedTime = Time.realtimeSinceStartup - elapsedTime;
        Debug.Log($"Distance time: {elapsedTime} seconds for {numOfTests} iterations.");

        result = DO_Scape.YFromTriPointAlpha(point, tri);
        Debug.Log($"Alpha Result: {result}");
        result = DO_Scape.YFromTriPointPlane(point, tri);
        Debug.Log($"Plane Result: {result}");
        result = DO_Scape.YFromTriPointDist(point, tri);
        Debug.Log($"Distance Result: {result}");
    }

}
