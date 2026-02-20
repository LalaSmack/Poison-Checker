using System.Collections;
using UnityEngine;

public class AmbientMusic : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip ambientClip;

    [Header("Fade")]
    [SerializeField] private float fadeInSeconds = 1.5f;
    [SerializeField] private float fadeOutSeconds = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float targetVolume = 0.8f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        // Optional: keep music when loading new scenes
        DontDestroyOnLoad(gameObject);

        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f; // 2D
    }

    private void Start()
    {
        if (ambientClip != null)
            Play(ambientClip, fadeInSeconds);
    }

    public void Play(AudioClip clip, float fadeIn)
    {
        if (clip == null) return;

        source.clip = clip;
        source.volume = 0f;
        source.loop = true;
        source.Play();

        StartFadeTo(targetVolume, fadeIn);
    }

    public void FadeOutAndStop()
    {
        StartFadeTo(0f, fadeOutSeconds, stopAfter: true);
    }

    public void FadeTo(float volume01, float seconds)
    {
        StartFadeTo(Mathf.Clamp01(volume01), seconds);
    }

    private void StartFadeTo(float newVolume, float seconds, bool stopAfter = false)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(newVolume, seconds, stopAfter));
    }

    private IEnumerator FadeRoutine(float newVolume, float seconds, bool stopAfter)
    {
        float start = source.volume;
        float t = 0f;

        if (seconds <= 0f)
        {
            source.volume = newVolume;
            if (stopAfter && Mathf.Approximately(newVolume, 0f)) source.Stop();
            yield break;
        }

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime; // keeps fading even if Time.timeScale = 0
            float lerp = t / seconds;
            source.volume = Mathf.Lerp(start, newVolume, lerp);
            yield return null;
        }

        source.volume = newVolume;

        if (stopAfter && Mathf.Approximately(newVolume, 0f))
            source.Stop();
    }
}
