using UnityEngine;

[CreateAssetMenu(fileName = "NetPresentation", menuName = "NetBreak/Net Presentation")]
public sealed class NetPresentationProfile : ScriptableObject
{
    [SerializeField] private Sprite meshTile;
    [SerializeField] private Sprite ropeTile;
    [SerializeField] private Sprite[] contactFrames = new Sprite[4];
    [SerializeField] private AudioClip placeClip;
    [Range(0f, 1f)] [SerializeField] private float placeVolume = 0.32f;

    public Sprite MeshTile => meshTile;
    public Sprite RopeTile => ropeTile;
    public Sprite[] ContactFrames => contactFrames;
    public AudioClip PlaceClip => placeClip;
    public float PlaceVolume => placeVolume;
    public bool HasContact => contactFrames != null && contactFrames.Length == 4 &&
        System.Array.TrueForAll(contactFrames, frame => frame != null);
}
