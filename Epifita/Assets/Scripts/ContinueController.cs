using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueController : MonoBehaviour
{
    [Tooltip("Segundos que dura la cortinilla. 0 = duración del video.")]
    [SerializeField] private float duration = 6f;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;
        float wait = duration;

        var media = MediaManager.Instance;
        if (media != null)
        {
            media.PlayContinueMedia();   // video + música + llanto
            if (wait <= 0f) wait = (float)media.ContinueLength;
        }

        if (wait <= 0f) wait = 5f;

        yield return new WaitForSecondsRealtime(wait);

        if (media != null) media.StopContinueSfx(); // corta el llanto
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}