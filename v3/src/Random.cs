using System.Collections.Generic;
using Godot;

public static class RandomExtensions
{
    public static T GetRandom<T>(this IList<T> list, RandomNumberGenerator rng)
    {
        return list[rng.RandiRange(0, list.Count - 1)];
    }

    public static T GetRandomWeighted<T>(
        this IList<T> list,
        float[] chance,
        RandomNumberGenerator rng
    )
    { 
        var total = 0f;
        foreach (var c in chance)
        {
            total += c;
        }
        var random = rng.RandfRange(0, total);
        var current = 0f;
        for (var i = 0; i < chance.Length; i++)
        {
            current += chance[i];
            if (random <= current)
            {
                return list[i];
            }
        }
        return list[0];

    }

    public static T GetRandomOnce<T>(this IList<T> list, RandomNumberGenerator rng)
    {
        if (list.Count == 0)
        {
            return default(T);
        }
        var index = rng.RandiRange(0, list.Count - 1);
        var item = list[index];
        list.RemoveAt(index);
        return item;
    }
}
