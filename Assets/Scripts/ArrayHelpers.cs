using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ArrayHelpers
{
    public static ICollection<T> Shuffle<T>(ICollection<T> collectionToShuffle)
    {
        T[] shuffledArray = collectionToShuffle.ToArray();
        int n = shuffledArray.Length;

        while (n > 1)
        {
            int k = UnityEngine.Random.Range(0, n);
            n--;

            T temp = shuffledArray[n];
            shuffledArray[n] = shuffledArray[k];
            shuffledArray[k] = temp;
        }

        return shuffledArray;
    }
}

