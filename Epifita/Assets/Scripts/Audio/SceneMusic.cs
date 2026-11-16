using System.Collections;
using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public static SceneMusic Instance { get; private set; }

    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private float fadeIn = 1f;

    private AudioSource src;
    private Coroutine fade;

    private void Awake()
    {
        Instance = this;

        src = gameObject.AddComponent<AudioSource>();
        src.clip = music;
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        src.volume = 0f;
    }

    private void Start()
    {
        if (music == null) return;
        src.Play();
        FadeTo(volume, fadeIn);
    }

    public void FadeOut(float seconds) => FadeTo(0f, seconds);

    private void FadeTo(float target, float seconds)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeRoutine(target, seconds));
    }

    private IEnumerator FadeRoutine(float target, float seconds)
    {
        float start = src.volume;

        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            src.volume = Mathf.Lerp(start, target, t / seconds);
            yield return null;
        }

        src.volume = target;
        if (target <= 0f) src.Stop();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}