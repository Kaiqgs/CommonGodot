using System;

public static class MathUtil
{
    public static float Constrain(float n, float low, float high)
    {
        return Math.Max(Math.Min(n, high), low);
    }

    public static float Map(
        float n,
        float start1,
        float stop1,
        float start2,
        float stop2,
        bool withinBounds = true
    )
    {
        float newval = (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;
        if (!withinBounds)
        {
            return newval;
        }
        if (start2 < stop2)
        {
            return MathUtil.Constrain(newval, start2, stop2);
        }
        else
        {
            return MathUtil.Constrain(newval, stop2, start2);
        }
    }
}
