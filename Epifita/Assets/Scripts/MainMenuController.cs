using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MainMenuController : MonoBehaviour
{
    private bool started; // evita cargar la escena dos veces

    private void Start()
    {
        Time.timeScale = 1f; // seguridad si vienes de un juego pausado
		PlayerController.CarriesWeapon = false;
        if (MediaManager.Instance != null)
            MediaManager.Instance.PlayMenuMedia();
		PlayerController.CarriesWeapon = false;
    }

    private void Update()
    {
        if (started || !EnterPressed()) return;
        started = true;

        if (MediaManager.Instance != null)
            MediaManager.Instance.PauseForGameplay();

        SceneManager.LoadScene(SceneNames.Cinematica); // acción de "continuar"
    }

    // Funciona con el Input System nuevo y con el antiguo
    private bool EnterPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && (kb.enterKey.wasPressedThisFrame ||
                              kb.numpadEnterKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
    }
}