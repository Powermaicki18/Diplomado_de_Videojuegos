using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndController : MonoBehaviour
{
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private float victoryDelay = 2f; // segundos antes de ir a la cortinilla (0 = inmediato)

    private bool ended; // evita mostrar derrota y victoria a la vez

    private void Start()
    {
        Time.timeScale = 1f;
        defeatPanel.SetActive(false);
        victoryPanel.SetActive(false);
    }

    // Llámalo desde tu lógica de juego cuando el jugador pierde.
    [ContextMenu("Test Defeat")]
    public void ShowDefeat()
    {
        if (ended) return;
        ended = true;
        defeatPanel.SetActive(true);
        Time.timeScale = 0f; // congela el juego detrás del popup
    }

    // Llámalo desde tu lógica de juego cuando el jugador gana.
    [ContextMenu("Test Victory")]
    public void ShowVictory()
    {
        if (ended) return;
        ended = true;
        victoryPanel.SetActive(true);
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        // Realtime: funciona aunque timeScale sea 0
        yield return new WaitForSecondsRealtime(victoryDelay);
        GoToContinue();
    }

    // Botón "Reiniciar"
    public void OnRestartClicked()
    {
        Time.timeScale = 1f; // restaurar SIEMPRE antes de cargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Segundo botón de derrota: lleva a la cortinilla
    public void OnContinueClicked() => GoToContinue();

    private void GoToContinue()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneNames.Continue);
    }
}