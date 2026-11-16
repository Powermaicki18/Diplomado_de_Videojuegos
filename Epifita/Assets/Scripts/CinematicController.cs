using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CinematicController : MonoBehaviour
{
    [Tooltip("Si la cinemática es un video, arrástralo aquí. Termina cuando acabe el video.")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Tooltip("Si no hay video (animación, Timeline, imágenes), dura estos segundos.")]
    [SerializeField] private float duration = 20f;

    [SerializeField] private bool allowSkip = true;
    [SerializeField] private string nextScene = "Instructions";

    private bool loading;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = false;
            videoPlayer.loopPointReached += _ => GoNext();
            if (!videoPlayer.isPlaying) videoPlayer.Play();
        }
        else
        {
            yield return new WaitForSecondsRealtime(duration);
            GoNext();
        }
    }

    private void Update()
    {
        if (!allowSkip || loading) return;

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            GoNext();
        }
    }

    private void GoNext()
    {
        if (loading) return;   // evita cargar dos veces
        loading = true;
        SceneManager.LoadScene(nextScene);
    }
}