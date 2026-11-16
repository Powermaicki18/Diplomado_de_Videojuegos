using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndController : MonoBehaviour
{
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private float victoryDelay = 2f;

    [Header("Sonidos")]
    [SerializeField] private AudioClip defeatClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private bool ended;

    private void Start()
    {
        Time.timeScale = 1f;
        defeatPanel.SetActive(false);
        victoryPanel.SetActive(false);
    }

    [ContextMenu("Test Defeat")]
    public void ShowDefeat()
    {
        if (ended) return;
        ended = true;

        if (SceneMusic.Instance != null) SceneMusic.Instance.FadeOut(0.5f);
        Sfx.Play(defeatClip, sfxVolume);

        defeatPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    [ContextMenu("Test Victory")]
    public void ShowVictory()
    {
        if (ended) return;
        ended = true;

        if (SceneMusic.Instance != null) SceneMusic.Instance.FadeOut(0.5f);
        Sfx.Play(victoryClip, sfxVolume);

        victoryPanel.SetActive(true);
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        yield return new WaitForSecondsRealtime(victoryDelay);
        GoToContinue();
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnContinueClicked() => GoToContinue();

    private void GoToContinue()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneNames.Continue);
    }
}