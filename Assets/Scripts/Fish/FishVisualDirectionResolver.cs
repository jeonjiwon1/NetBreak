using UnityEngine;

public enum FishVisualSet { Horizontal, Vertical, Diagonal }
public enum FishVisualHeading { East, NorthEast, North, NorthWest, West, SouthWest, South, SouthEast }

public readonly struct FishVisualDirection
{
    public readonly FishVisualHeading Heading;
    public readonly FishVisualSet Set;
    public readonly bool FlipX;
    public readonly bool FlipY;

    public FishVisualDirection(FishVisualHeading heading)
    {
        Heading = heading;
        int bucket = (int)heading;
        Set = bucket % 2 == 1 ? FishVisualSet.Diagonal :
              bucket == 2 || bucket == 6 ? FishVisualSet.Vertical : FishVisualSet.Horizontal;
        FlipX = bucket == 3 || bucket == 4 || bucket == 5;
        FlipY = bucket == 5 || bucket == 6 || bucket == 7;
    }
}

public static class FishVisualDirectionResolver
{
    private const float MinimumSpeedSquared = 0.000001f;
    private const float HysteresisDegrees = 7.5f;

    public static FishVisualDirection Resolve(Vector2 movementDirection, FishVisualHeading previous)
    {
        if (movementDirection.sqrMagnitude <= MinimumSpeedSquared)
            return new FishVisualDirection(previous);

        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        if (Mathf.Abs(Mathf.DeltaAngle((int)previous * 45f, angle)) <=
            22.5f + HysteresisDegrees)
            return new FishVisualDirection(previous);

        int bucket = (Mathf.RoundToInt(angle / 45f) + 8) % 8;
        return new FishVisualDirection((FishVisualHeading)bucket);
    }
}
