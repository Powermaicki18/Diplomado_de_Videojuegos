using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LoadingDots : MonoBehaviour
{
    [SerializeField] private string baseText = "Cargando";
    [SerializeField] private float interval = 0.4f; // segundos entre cada punto

    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        // Ciclo 0..3 puntos. unscaledTime: no depende de Time.timeScale
        int dots = (int)(Time.unscaledTime / interval) % 4;
        label.text = baseText + new string('.', dots);
    }
}
