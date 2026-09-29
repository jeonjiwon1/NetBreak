using System.Collections;
using UnityEngine;

public sealed class Area1BgmController : MonoBehaviour
{
    public enum Track { None, Normal, MiniBoss, Boss }

    private const string ProfilePath = "Area1BgmProfile";
    private Area1BgmProfile profile;
    private AudioSource source;
    private Coroutine transition;
    private Track currentTrack;

    public Track CurrentTrack => currentTrack;
    public AudioSource Source => source;

    private void Awake()
    {
        profile = Resources.Load<Area1BgmProfile>(ProfilePath);
        Transform child = transform.Find("Area1BgmSource");
        if (child == null)
        {
            child = new GameObject("Area1BgmSource").transform;
            child.SetParent(transform, false);
        }

        source = child.GetComponent<AudioSource>();
        if (source == null) source = child.gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.pitch = 1f;
        source.volume = 0f;
    }

    public void PlayNormal() => Play(Track.Normal);
    public void PlayMiniBoss() => Play(Track.MiniBoss);
    public void PlayBoss() => Play(Track.Boss);
    public void StopBgm() => Play(Track.None);

    private void Play(Track requested)
    {
        if (source == null) return;
        if (requested == currentTrack) return;

        AudioClip nextClip = GetClip(requested);
        if (requested != Track.None && nextClip == null)
        {
            Debug.LogWarning($"Area 1 BGM clip missing: {requested}", this);
            return;
        }

        currentTrack = requested;
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(TransitionTo(nextClip));
    }

    private AudioClip GetClip(Track track)
    {
        if (profile == null) return null;
        switch (track)
        {
            case Track.Normal: return profile.NormalClip;
            case Track.MiniBoss: return profile.MiniBossClip;
            case Track.Boss: return profile.BossClip;
            default: return null;
        }
    }

    private IEnumerator TransitionTo(AudioClip nextClip)
    {
        float duration = profile != null ? profile.FadeDuration : 0f;
        float half = duration * 0.5f;
        float startVolume = source.volume;
        if (source.isPlaying && half > 0f)
        {
            for (float elapsed = 0f; elapsed < half; elapsed += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / half);
                yield return null;
            }
        }

        source.Stop();
        source.clip = nextClip;
        source.volume = 0f;
        if (nextClip != null)
        {
            source.pitch = 1f;
            source.Play();
            float targetVolume = profile != null ? profile.Volume : 0f;
            if (half > 0f)
            {
                for (float elapsed = 0f; elapsed < half; elapsed += Time.unscaledDeltaTime)
                {
                    source.volume = Mathf.Lerp(0f, targetVolume, elapsed / half);
                    yield return null;
                }
            }
            source.volume = targetVolume;
        }
        transition = null;
    }

    private void OnDisable()
    {
        if (transition != null) StopCoroutine(transition);
        transition = null;
        currentTrack = Track.None;
        if (source != null)
        {
            source.Stop();
            source.clip = null;
        }
    }
}
