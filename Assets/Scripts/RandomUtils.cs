

using System.Linq;
using UnityEngine;

internal static class RandomUtils
{
    public static int RandomEvent(params float[] weights)
    {
        var max = weights.Aggregate((a, b) => a + b);
        var val = Random.value * max;
        var cur = 0f;
        for(int i = 0; i < weights.Length; i++)
        {
            cur += weights[i];
            if(val <= cur)
            {
                return i;
            }
        }
        return Random.Range(0, weights.Length);
    }
}

