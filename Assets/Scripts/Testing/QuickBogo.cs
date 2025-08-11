using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] Quogo Sort (Quick Bogo Sort). Patent NOT Pending
public class QuickBogo
{
    private int[]  _sortArray;
    private int[]  _indexArray;
    private int downCount, upCount;

    //[][] Constructor
    public QuickBogo(int[] sortArray)
    {
        _sortArray = sortArray;
        _indexArray = new int[_sortArray.Length];
    }
    public void BeginSort()
    {
        float elapsedTime = Time.realtimeSinceStartup;
        DoSort(0, _sortArray.Length - 1, _sortArray.Length / 2);
        elapsedTime = Time.realtimeSinceStartup - elapsedTime;
        Debug.Log($"Sort complete! Sorted {_sortArray.Length} elements in {elapsedTime} seconds.");
        ValidateSort();
    }
    private void DoSort(int lowerIndex, int upperIndex, int pivotIndex)
    {
        //[][] NOTE: lowerIndex and upperIndex must both be valid indices of the array, as the method is Inclusive

        //[][] NEW Method (the old one had outrageously bad time complexity)

        if (lowerIndex >= upperIndex) return;
        do
        {
            downCount = upCount = 0;
            //[][] Count Downs and Ups (Downs = larger value than pivot at smaller index; Ups = vice versa)
            for (int i = lowerIndex; i < upperIndex + 1; i++)
            {
                if (_sortArray[i] > _sortArray[pivotIndex] && i < pivotIndex)
                {
                    _indexArray[downCount + upCount] = i;
                    downCount++;
                }
                if (_sortArray[i] < _sortArray[pivotIndex] && i > pivotIndex)
                {
                    _indexArray[downCount + upCount] = i;
                    upCount++;
                }
            }
            //[][] If Down and Up counts are both positive, reshuffle Downs and Ups
            if (downCount > 0 && upCount > 0)
            {
                Shuffle(_indexArray, 0, downCount + upCount - 1);
                int temp = _sortArray[_indexArray[0]];
                for (int i = 0; i < downCount + upCount; i++)
                {
                    _sortArray[_indexArray[i]] = (i == downCount + upCount - 1) ? temp : _sortArray[_indexArray[i + 1]];
                }
            }
            //[][] If Down count is positive while Up count is zero or vice versa, apply specific pivot slide
            if (downCount == 0 && upCount > 0)
            {
                SSwapAt(pivotIndex, pivotIndex + upCount);
            }
            if (downCount > 0 && upCount == 0)
            {
                SSwapAt(pivotIndex, pivotIndex - downCount);
            }
            //[][] if Down and Up counts are both zero, sort each half; end loop
            if (downCount == 0 && upCount == 0)
            {
                DoSort(lowerIndex, pivotIndex - 1, (lowerIndex + pivotIndex - 1) / 2);
                DoSort(pivotIndex + 1, upperIndex, (pivotIndex + 1 + upperIndex) / 2);
                break;
            }

        } while (true);
    }
    private void SSwapAt(int index1, int index2)
    {
        int temp            = _sortArray[index2];
        _sortArray[index2]  = _sortArray[index1];
        _sortArray[index1]  = temp;
    }
    private void ValidateSort()
    {
        for (int i = 0; i < _sortArray.Length - 1; i++)
        {
            if (_sortArray[i] > _sortArray[i + 1])
            {
                Debug.Log($"Sort array failed validation. Value {_sortArray[i]} before value {_sortArray[i + 1]} at {i}.");
                return;
            }
        }
        Debug.Log("Sort array passed validation!");
    }
    private void Shuffle(int[] array, int lowerIndex, int upperIndex)
    {
        //[][] Fisher-Yates Shuffle

        for (int i = upperIndex; i > lowerIndex; i--)
        {
            int rand = Random.Range(lowerIndex, i + 1);
            int temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }
}
