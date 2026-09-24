using UnityEngine;

[CreateAssetMenu(fileName = "FishVisualProfile_", menuName = "NetBreak/Fish Visual Profile")]
public sealed class FishVisualProfile : ScriptableObject
{
    [SerializeField] private Sprite[] horizontalFrames = new Sprite[4];
    [SerializeField] private Sprite[] verticalFrames = new Sprite[4];
    [SerializeField] private Sprite[] diagonalFrames = new Sprite[4];
    [SerializeField] private Sprite[] diagonalNorthWestFrames = new Sprite[4];
    [Min(0.1f)] [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private Vector2 visualScale = Vector2.one;
    [SerializeField] private Color tint = Color.white;

    public Sprite[] HorizontalFrames => horizontalFrames;
    public Sprite[] VerticalFrames => verticalFrames;
    public Sprite[] DiagonalFrames => diagonalFrames;
    public Sprite[] DiagonalNorthWestFrames => diagonalNorthWestFrames;
    public float FramesPerSecond => framesPerSecond;
    public Vector2 VisualScale => visualScale;
    public Color Tint => tint;

    public bool IsValid => HasFourFrames(horizontalFrames) &&
                           HasFourFrames(verticalFrames) &&
                           HasFourFrames(diagonalFrames) &&
                           HasFourFrames(diagonalNorthWestFrames);

    public Sprite[] GetFrames(FishVisualSet set)
    {
        switch (set)
        {
            case FishVisualSet.Vertical: return verticalFrames;
            case FishVisualSet.Diagonal: return diagonalFrames;
            case FishVisualSet.DiagonalNorthWest: return diagonalNorthWestFrames;
            default: return horizontalFrames;
        }
    }

    private static bool HasFourFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length != 4) return false;
        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] == null) return false;
        }
        return true;
    }
}
