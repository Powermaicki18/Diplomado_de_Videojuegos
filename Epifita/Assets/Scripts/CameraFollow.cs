using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Suavizado")]
    [SerializeField] private float smoothTime = 0.15f; // 0 = sin suavizado
    [SerializeField] private Vector2 offset = Vector2.zero;

    [Header("Límites del mapa (opcional)")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -10f); // esquina inferior izquierda
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 10f);   // esquina superior derecha

    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        if (target != null)
        {
            // Empieza ya sobre el jugador, sin "viaje" inicial
            transform.position = GetTargetPosition();
        }
    }

    // LateUpdate: se ejecuta después de que el jugador se movió, evita temblores
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = GetTargetPosition();

        if (smoothTime <= 0f)
            transform.position = goal;
        else
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }

    private Vector3 GetTargetPosition()
    {
        float x = target.position.x + offset.x;
        float y = target.position.y + offset.y;

        if (useBounds && cam != null && cam.orthographic)
        {
            // Mitad del tamaño visible de la cámara
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            // Si el mapa es más pequeño que la cámara, se centra en el mapa
            float minX = minBounds.x + halfW;
            float maxX = maxBounds.x - halfW;
            float minY = minBounds.y + halfH;
            float maxY = maxBounds.y - halfH;

            x = (minX > maxX) ? (minBounds.x + maxBounds.x) * 0.5f : Mathf.Clamp(x, minX, maxX);
            y = (minY > maxY) ? (minBounds.y + maxBounds.y) * 0.5f : Mathf.Clamp(y, minY, maxY);
        }

        return new Vector3(x, y, transform.position.z); // conserva la Z de la cámara (-10)
    }

    // Dibuja el rectángulo de límites en la ventana Scene
    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.yellow;
        Vector3 center = (minBounds + maxBounds) * 0.5f;
        Vector3 size = maxBounds - minBounds;
        Gizmos.DrawWireCube(center, size);
    }
}