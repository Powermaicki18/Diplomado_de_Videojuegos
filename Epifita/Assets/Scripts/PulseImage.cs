using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PulseImage : MonoBehaviour
{
    [SerializeField] private float speed = 2f;      // más alto = pulsa más rápido
    [SerializeField] private float minAlpha = 0.3f; // transparencia mínima
    [SerializeField] private float minScale = 0.95f;
    [SerializeField] private float maxScale = 1.05f;

    private CanvasGroup group;
    private Vector3 baseScale;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        // Onda seno convertida a rango 0..1
        float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
        group.alpha = Mathf.Lerp(minAlpha, 1f, t);
        transform.localScale = baseScale * Mathf.Lerp(minScale, maxScale, t);
    }
}
