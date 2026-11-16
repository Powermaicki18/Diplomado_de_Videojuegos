using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    private const string P_ATTACK = "ATTACK";
    private const string P_DIE    = "DIE";

    [Header("Zona de patrulla (centrada donde pongas al enemigo)")]
    [SerializeField] private float patrolRadius = 3f;       // alcance a cada lado
    [SerializeField] private float verticalTolerance = 1f;  // cuánta diferencia en Y acepta

    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed = 1f;
    [SerializeField] private float chaseSpeed = 2f;
    [SerializeField] private float waitAtEdge = 1f;
    [Tooltip("Marca esto si el sprite original mira a la DERECHA")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Detección y ataque")]
    [SerializeField] private float detectRange = 4f;
    [SerializeField] private float attackRange = 0.9f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float hitDelay = 0.3f;         // cuándo "pega" dentro de la animación
    [SerializeField] private float attackLockTime = 0.5f;   // duración aprox. del clip de ataque

    [Header("Vida")]
    [SerializeField] private int health = 3;
    [SerializeField] private float destroyDelay = 1.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Transform player;
    private PlayerController playerCtrl;

    private float spawnX, minX, maxX;
    private int patrolDir = 1;
    private float waitUntil, busyUntil, nextAttack;
    private bool dead;
    private Collider2D myCol;
    private Collider2D playerCol;
    

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        spawnX = transform.position.x;
        minX = spawnX - patrolRadius;
        maxX = spawnX + patrolRadius;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerCtrl = p.GetComponent<PlayerController>();
        }
    }

    private void FixedUpdate()
    {
        if (dead || Time.time < busyUntil || Time.time < waitUntil)
        {
            Stop();
            return;
        }

        if (PlayerInZone())
        {
            float dx = player.position.x - transform.position.x;
            Face(dx);

            if (HorizontalGap() <= attackRange)
            {
                Stop();
                if (Time.time >= nextAttack) StartCoroutine(Attack());
            }
            else
            {
                MoveHorizontal(Mathf.Sign(dx), chaseSpeed);
            }
            return;
        }

        Patrol();
    }

    // El jugador cuenta solo si está vivo, dentro de la zona y cerca
    private bool PlayerInZone()
    {
        if (player == null || (playerCtrl != null && playerCtrl.IsDead)) return false;

        Vector2 p = player.position;
        bool insideX = p.x >= minX && p.x <= maxX;
        bool insideY = VerticalGap() <= verticalTolerance;
        bool near = Mathf.Abs(p.x - transform.position.x) <= detectRange;
        return insideX && insideY && near;
    }

    private void Patrol()
    {
        float x = transform.position.x;

        if (x >= maxX && patrolDir > 0)      { patrolDir = -1; waitUntil = Time.time + waitAtEdge; Stop(); return; }
        if (x <= minX && patrolDir < 0)      { patrolDir =  1; waitUntil = Time.time + waitAtEdge; Stop(); return; }

        Face(patrolDir);
        MoveHorizontal(patrolDir, patrolSpeed);
    }

    // Nunca sale de la zona, ni persiguiendo
    private void MoveHorizontal(float dir, float speed)
    {
        float x = transform.position.x;
        if ((dir > 0 && x >= maxX) || (dir < 0 && x <= minX))
        {
            Stop();
            return;
        }
        rb.linearVelocity = new Vector2(dir * speed, 0f);   // Unity 2022 o anterior: rb.velocity
    }
    
    // Distancia horizontal entre los bordes de los colliders (no entre centros)
    private float HorizontalGap()
    {
        if (myCol == null || playerCol == null)
            return Mathf.Abs(player.position.x - transform.position.x);

        float dx = Mathf.Abs(playerCol.bounds.center.x - myCol.bounds.center.x);
        return dx - myCol.bounds.extents.x - playerCol.bounds.extents.x;
    }
    
    // Diferencia vertical entre los centros de los colliders
    private float VerticalGap()
    {
        if (myCol == null || playerCol == null)
            return Mathf.Abs(player.position.y - transform.position.y);
        return Mathf.Abs(playerCol.bounds.center.y - myCol.bounds.center.y);
    }

    private void Stop() => rb.linearVelocity = Vector2.zero; // Unity 2022 o anterior: rb.velocity

    // FLIP: se voltea el SpriteRenderer, no hace falta otra animación
    private void Face(float dx)
    {
        if (Mathf.Approximately(dx, 0f)) return;
        sr.flipX = spriteFacesRight ? dx < 0f : dx > 0f;
    }

    private IEnumerator Attack()
    {
        nextAttack = Time.time + attackCooldown;
        busyUntil = Time.time + attackLockTime;
        animator.SetTrigger(P_ATTACK);

        yield return new WaitForSeconds(hitDelay);

        if (dead || player == null || playerCtrl == null || playerCtrl.IsDead) yield break;

        if (HorizontalGap() <= attackRange + 0.3f && VerticalGap() <= verticalTolerance)
            playerCtrl.TakeDamage();
    }

    public void TakeDamage(int amount = 1)
    {
        if (dead) return;
        health -= amount;
        if (health <= 0) Die();
        else StartCoroutine(Flash());
    }

    // No hay animación de daño, así que parpadea en rojo
    private IEnumerator Flash()
    {
        sr.color = new Color(1f, 0.4f, 0.4f);
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    private void Die()
    {
        dead = true;
        StopAllCoroutines();
        sr.color = Color.white;
        Stop();
        rb.simulated = false;           // desactiva su collider y física
        animator.SetTrigger(P_DIE);
        Destroy(gameObject, destroyDelay);
    }

    private void OnDrawGizmosSelected()
    {
        float cx = Application.isPlaying ? spawnX : transform.position.x;
        Vector3 center = new Vector3(cx, transform.position.y, 0f);

        Gizmos.color = Color.cyan; // zona de patrulla
        Gizmos.DrawWireCube(center, new Vector3(patrolRadius * 2f, verticalTolerance * 2f, 0f));
        Gizmos.color = Color.red;  // rango de ataque
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}