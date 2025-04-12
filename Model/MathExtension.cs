public static class MathExtension
{
    public static int Map(this int value, int fromLow, int fromHigh, int toLow, int toHigh)
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }

    public static decimal Map(this decimal value, decimal fromLow, decimal fromHigh, decimal toLow, decimal toHigh)
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}