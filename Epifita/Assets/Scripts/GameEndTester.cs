using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// SOLO PARA PRUEBAS: simula victoria/derrota con el teclado.
// Se compila únicamente en el Editor y en Development Builds,
// así que no llega a la versión final.
public class GameEndTester : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private GameEndController gameEnd;

    private void Update()
    {
        if (KeyDown('D')) gameEnd.ShowDefeat();   // D = Derrota
        if (KeyDown('V')) gameEnd.ShowVictory();  // V = Victoria
    }

    // Compatible con Input System nuevo y antiguo
    private bool KeyDown(char key)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return false;
        return key == 'D' ? kb.dKey.wasPressedThisFrame : kb.vKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(key == 'D' ? KeyCode.D : KeyCode.V);
#endif
    }
#endif
}