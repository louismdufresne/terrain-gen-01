using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_SortTester : MonoBehaviour
{
    private static readonly int r_arrayLength = 10000;
    private QuickBogo _quogo;
    int[] _toSort;
    private void Awake()
    {
        //[][] Array setup
        _toSort = new int[r_arrayLength];
        for (int i = 0; i < r_arrayLength; i++)
        {
            _toSort[i] = i;
        }
        Shuffle(_toSort);

        //[][] Sorts
        _quogo = new QuickBogo(_toSort);
        _quogo.BeginSort();
    }
    private void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }
}
