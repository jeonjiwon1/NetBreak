using UnityEngine;

[CreateAssetMenu(fileName = "Area1BgmProfile", menuName = "NetBreak/Area 1 BGM Profile")]
public sealed class Area1BgmProfile : ScriptableObject
{
    [SerializeField] private AudioClip normalClip;
    [SerializeField] private AudioClip miniBossClip;
    [SerializeField] private AudioClip bossClip;
    [Range(0f, 1f)] [SerializeField] private float volume = 0.22f;
    [Min(0f)] [SerializeField] private float fadeDuration = 0.5f;

    public AudioClip NormalClip => normalClip;
    public AudioClip MiniBossClip => miniBossClip;
    public AudioClip BossClip => bossClip;
    public float Volume => Mathf.Clamp01(volume);
    public float FadeDuration => Mathf.Max(0f, fadeDuration);
}
