using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsController : MonoBehaviour
{
    [Tooltip("Segundos que se muestra la imagen antes de cargar el juego.")]
    [SerializeField] private float duration = 8f;

    // Start como corrutina: se ejecuta al entrar a la escena y puede esperar.
    private IEnumerator Start()
    {
        Time.timeScale = 1f; // seguridad

        // Realtime: no depende de Time.timeScale
        yield return new WaitForSecondsRealtime(duration);

        SceneManager.LoadScene(SceneNames.Game);
    }
}