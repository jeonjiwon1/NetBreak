using System;

public static class ItemLevelScaling
{
    public static float CalculateAdditiveDamage(
        float baseDamage,
        int itemLevel,
        float bonusPerLevel)
    {
        return CalculateScaledValue(
            Math.Max(0f, baseDamage),
            Math.Max(1, itemLevel),
            Math.Max(0f, bonusPerLevel),
            true);
    }

    public static float CalculateAdditiveDuration(
        float baseDuration,
        int itemLevel,
        float additionalSecondsPerLevel)
    {
        return CalculateScaledValue(
            Math.Max(0f, baseDuration),
            Math.Max(1, itemLevel),
            Math.Max(0f, additionalSecondsPerLevel),
            false);
    }

    private static float CalculateScaledValue(
        float baseline,
        int itemLevel,
        float perLevel,
        bool isMultiplier)
    {
        double extraLevels = itemLevel - 1d;
        double value = isMultiplier
            ? baseline * (1d + perLevel * extraLevels)
            : baseline + perLevel * extraLevels;

        if (double.IsNaN(value) || value <= 0d)
        {
            return 0f;
        }

        return value >= float.MaxValue ? float.MaxValue : (float)value;
    }
}
