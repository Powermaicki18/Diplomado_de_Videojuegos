using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(AudioSource))]
public class MediaManager : MonoBehaviour
{
    public static MediaManager Instance { get; private set; }

    [Header("Música (compartida por menú y cortinilla)")]
    [SerializeField] private AudioClip musicClip;

    [Header("Videos de fondo")]
    [SerializeField] private VideoClip menuClip;
    [SerializeField] private VideoClip continueClip;

    [Header("Llanto (solo en la cortinilla)")]
    [SerializeField] private AudioClip cryClip;
    [SerializeField, Range(0f, 1f)] private float cryVolume = 1f;
    [SerializeField] private float cryDelay = 0f;   // segundos de espera antes de sonar
    [SerializeField] private bool cryLoop = false;  // true = se repite durante la cortinilla

    private VideoPlayer video;
    private AudioSource music;
    private AudioSource cry;       // segundo AudioSource, creado por código
    private Coroutine cryRoutine;

    public double ContinueLength => continueClip != null ? continueClip.length : 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        video = GetComponent<VideoPlayer>();
        music = GetComponent<AudioSource>(); // el AudioSource que ya tienes

        video.playOnAwake = false;
        video.isLooping = true;
        video.audioOutputMode = VideoAudioOutputMode.None;
        video.clip = menuClip;

        music.clip = musicClip;
        music.loop = true;
        music.playOnAwake = false;

        // Segundo AudioSource solo para el llanto
        cry = gameObject.AddComponent<AudioSource>();
        cry.playOnAwake = false;

        video.Prepare();
    }

    // MENÚ: video con título + música (y sin llanto)
    public void PlayMenuMedia()
    {
        StopContinueSfx();
        PlayVideo(menuClip);
        PlayMusic();
    }

    // CORTINILLA: video de fondo + música + llanto
    public void PlayContinueMedia()
    {
        PlayVideo(continueClip);
        PlayMusic();
        PlayCry();
    }

    // Detiene el llanto (al salir de la cortinilla)
    public void StopContinueSfx()
    {
        if (cryRoutine != null) StopCoroutine(cryRoutine);
        cryRoutine = null;
        if (cry != null) cry.Stop();
    }

    public void PauseForGameplay()
    {
        StopContinueSfx();
        video.Pause();
        music.Stop();
    }

    private void PlayVideo(VideoClip clip)
    {
        if (clip == null) return;
        if (video.clip == clip && video.isPlaying) return;

        video.Stop();
        video.clip = clip;
        video.time = 0;
        video.Play();
    }

    private void PlayMusic()
    {
        if (!music.isPlaying) music.Play();
    }

    private void PlayCry()
    {
        if (cryClip == null) return;
        StopContinueSfx(); // evita que se solape consigo mismo
        cryRoutine = StartCoroutine(CryRoutine());
    }

    private IEnumerator CryRoutine()
    {
        if (cryDelay > 0f) yield return new WaitForSecondsRealtime(cryDelay);
        cry.clip = cryClip;
        cry.volume = cryVolume;
        cry.loop = cryLoop;
        cry.Play();
        cryRoutine = null;
    }
}