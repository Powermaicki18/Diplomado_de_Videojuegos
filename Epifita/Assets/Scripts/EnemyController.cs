using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour, IDamageable
{
    private const string P_ATTACK = "ATTACK";
    private const string P_DIE    = "DIE";

    [Header("Zona de patrulla")]
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float verticalTolerance = 1.5f;

    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed = 1f;
    [SerializeField] private float chaseSpeed = 2f;
    [SerializeField] private float waitAtEdge = 1f;
    [Tooltip("Marca esto si el sprite original mira a la DERECHA")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Detección y ataque")]
    [SerializeField] private float detectRange = 4f;
    [SerializeField] private float attackRange = 0.4f;   // espacio entre bordes
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float hitDelay = 0.3f;
    [SerializeField] private float attackLockTime = 0.5f;

    [Header("Vida")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float destroyDelay = 1.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Collider2D myCol, playerCol;
    private Transform player;
    private PlayerController playerCtrl;
    private WorldHealthBar healthBar;

    private int health;
    private float spawnX, minX, maxX;
    private int patrolDir = 1;
    private float waitUntil, busyUntil, nextAttack;
    private bool dead;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        myCol = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;

        health = maxHealth;

        spawnX = transform.position.x;
        minX = spawnX - patrolRadius;
        maxX = spawnX + patrolRadius;

        healthBar = GetComponent<WorldHealthBar>();
        if (healthBar == null) healthBar = gameObject.AddComponent<WorldHealthBar>();
        healthBar.SetValue(1f);

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerCtrl = p.GetComponent<PlayerController>();
            playerCol = p.GetComponent<Collider2D>();
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

    private float HorizontalGap()
    {
        if (myCol == null || playerCol == null)
            return Mathf.Abs(player.position.x - transform.position.x);

        float dx = Mathf.Abs(playerCol.bounds.center.x - myCol.bounds.center.x);
        return dx - myCol.bounds.extents.x - playerCol.bounds.extents.x;
    }

    private float VerticalGap()
    {
        if (myCol == null || playerCol == null)
            return Mathf.Abs(player.position.y - transform.position.y);
        return Mathf.Abs(playerCol.bounds.center.y - myCol.bounds.center.y);
    }

    private bool PlayerInZone()
    {
        if (player == null || (playerCtrl != null && playerCtrl.IsDead)) return false;

        float px = player.position.x;
        bool insideX = px >= minX && px <= maxX;
        bool insideY = VerticalGap() <= verticalTolerance;
        bool near = Mathf.Abs(px - transform.position.x) <= detectRange;
        return insideX && insideY && near;
    }

    private void Patrol()
    {
        float x = transform.position.x;

        if (x >= maxX && patrolDir > 0) { patrolDir = -1; waitUntil = Time.time + waitAtEdge; Stop(); return; }
        if (x <= minX && patrolDir < 0) { patrolDir =  1; waitUntil = Time.time + waitAtEdge; Stop(); return; }

        Face(patrolDir);
        MoveHorizontal(patrolDir, patrolSpeed);
    }

    private void MoveHorizontal(float dir, float speed)
    {
        float x = transform.position.x;
        if ((dir > 0 && x >= maxX) || (dir < 0 && x <= minX)) { Stop(); return; }
        rb.linearVelocity = new Vector2(dir * speed, 0f);   // Unity 2022 o anterior: rb.velocity
    }

    private void Stop() => rb.linearVelocity = Vector2.zero; // Unity 2022 o anterior: rb.velocity

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
        healthBar.SetValue((float)health / maxHealth);

        if (health <= 0) Die();
        else StartCoroutine(Flash());
    }

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
        healthBar.Hide();
        rb.simulated = false;
        animator.SetTrigger(P_DIE);
        Destroy(gameObject, destroyDelay);
    }

    private void OnDrawGizmosSelected()
    {
        float cx = Application.isPlaying ? spawnX : transform.position.x;
        Vector3 center = new Vector3(cx, transform.position.y, 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, new Vector3(patrolRadius * 2f, verticalTolerance * 2f, 0f));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}