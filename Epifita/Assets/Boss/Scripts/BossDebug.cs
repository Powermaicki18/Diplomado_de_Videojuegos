using UnityEngine;

public class BossDebug : MonoBehaviour
{
    private Animator animator;
    private float t;

    private void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log("BossDebug iniciado. Animator: " + (animator != null));
    }

    private void Update()
    {
        t += Time.deltaTime;

        // Cada 2 segundos cambia de dirección: Derecha, Arriba, Izquierda, Abajo
        int step = (int)(t / 2f) % 4;
        Vector2 dir = step == 0 ? Vector2.right :
                      step == 1 ? Vector2.up :
                      step == 2 ? Vector2.left : Vector2.down;

        transform.position += (Vector3)(dir * 1.5f * Time.deltaTime);

        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetFloat("WalkSpeed", 1f);
    }
}