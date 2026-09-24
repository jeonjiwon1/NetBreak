using UnityEngine;

public enum FishVisualSet { Horizontal, Vertical, Diagonal, DiagonalNorthWest }
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
        // 원본 E/N/NE/NW와 반대 방향은 같은 두 축을 함께 뒤집는다.
        Set = bucket == 1 || bucket == 5 ? FishVisualSet.Diagonal :
              bucket == 3 || bucket == 7 ? FishVisualSet.DiagonalNorthWest :
              bucket == 2 || bucket == 6 ? FishVisualSet.Vertical : FishVisualSet.Horizontal;
        FlipX = bucket >= 4;
        FlipY = bucket >= 4;
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
