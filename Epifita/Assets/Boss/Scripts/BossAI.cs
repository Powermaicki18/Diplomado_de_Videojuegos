using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossAnimationController))]
public class BossAI : MonoBehaviour, IDamageable
{
    private const string PLAYER_TAG = "Player";

    [Header("Objetivo (opcional)")]
    [SerializeField] private Transform player;

    [Header("Patrulla")]
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderSpeed = 1.2f;
    [SerializeField] private float wanderPauseMin = 1f;
    [SerializeField] private float wanderPauseMax = 3f;
    [SerializeField] private float wanderTimeout = 6f;

    [Header("Persecución")]
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float loseRange = 12f;

    [Header("Ataque")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 0.8f;
    [SerializeField] private float hitDelay = 0.4f;
    [Tooltip("Vida que le quita al prota por golpe (el prota tiene 5 por defecto)")]
    [SerializeField] private int attackDamage = 1;

    [Header("Vida")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private float hurtDuration = 0.4f;

    [Header("Fin del juego")]
    [SerializeField] private GameEndController gameEnd;
    [SerializeField] private float victoryDelay = 2f;

    [Header("Barra de vida (opcional)")]
    [SerializeField] private UIHealthBar healthBar;
    [SerializeField] private float showBarRange = 12f;

    private BossAnimationController anim;
    private PlayerController playerCtrl;
    private int currentHealth;
    private float nextAttackTime;
    private bool busy, dead, chasing;

    private Vector2 spawnPoint, wanderTarget;
    private float wanderTimer, wanderTravel;
    private bool waiting = true;
    private float nextPlayerSearch;

    private void Awake()
    {
        anim = GetComponent<BossAnimationController>();
        currentHealth = maxHealth;
        spawnPoint = transform.position;
        wanderTimer = Random.Range(wanderPauseMin, wanderPauseMax);
    }

    private void Start()
    {
        if (healthBar != null)
        {
            healthBar.SetValue(1f);
            healthBar.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (dead) return;

        FindPlayerIfNeeded();

        bool playerDead = playerCtrl != null && playerCtrl.IsDead;
        bool hasPlayer = player != null && player.gameObject.activeInHierarchy && !playerDead;
        float distance = hasPlayer
            ? Vector2.Distance(transform.position, player.position)
            : float.MaxValue;

        UpdateBar(hasPlayer, distance);

        if (busy) return;

        // Si el prota murió, el jefe se queda quieto
        if (playerDead)
        {
            chasing = false;
            anim.SetMovement(Vector2.zero);
            return;
        }

        if (!chasing && hasPlayer && distance <= detectionRange)
        {
            chasing = true;
        }
        else if (chasing && (!hasPlayer || distance > loseRange))
        {
            chasing = false;
            StartWaiting();
        }

        if (chasing) ChaseAndAttack(distance);
        else Wander();
    }

    private void UpdateBar(bool hasPlayer, float distance)
    {
        if (healthBar == null) return;
        bool show = hasPlayer && distance <= showBarRange;
        if (healthBar.gameObject.activeSelf != show) healthBar.gameObject.SetActive(show);
    }

    // ---------------- PATRULLA ----------------
    private void Wander()
    {
        if (waiting)
        {
            anim.SetMovement(Vector2.zero);
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f) PickWanderTarget();
            return;
        }

        Vector2 pos = transform.position;
        Vector2 dir = wanderTarget - pos;
        wanderTravel += Time.deltaTime;

        if (dir.magnitude < 0.1f || wanderTravel > wanderTimeout)
        {
            StartWaiting();
            return;
        }

        anim.SetMovement(dir);
        transform.position = Vector2.MoveTowards(pos, wanderTarget, wanderSpeed * Time.deltaTime);
    }

    private void StartWaiting()
    {
        waiting = true;
        wanderTimer = Random.Range(wanderPauseMin, wanderPauseMax);
        anim.SetMovement(Vector2.zero);
    }

    private void PickWanderTarget()
    {
        wanderTarget = spawnPoint + Random.insideUnitCircle * wanderRadius;
        wanderTravel = 0f;
        waiting = false;
    }

    // ---------------- PERSECUCIÓN Y ATAQUE ----------------
    private void ChaseAndAttack(float distance)
    {
        Vector2 toPlayer = player.position - transform.position;

        if (distance <= attackRange)
        {
            anim.SetMovement(Vector2.zero);
            anim.FaceDirection(toPlayer);

            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackRoutine());
            return;
        }

        anim.SetMovement(toPlayer);
        transform.position = Vector2.MoveTowards(
            transform.position, player.position, chaseSpeed * Time.deltaTime);
    }

    private IEnumerator AttackRoutine()
    {
        busy = true;
        nextAttackTime = Time.time + attackCooldown;

        anim.SetMovement(Vector2.zero);
        anim.FaceDirection(player.position - transform.position);
        anim.PlayAttack();

        yield return new WaitForSeconds(hitDelay);

        if (!dead && player != null && playerCtrl != null && !playerCtrl.IsDead &&
            Vector2.Distance(transform.position, player.position) <= attackRange * 1.3f)
        {
            playerCtrl.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));
        busy = false;
    }

    // ---------------- JUGADOR ----------------
    private void FindPlayerIfNeeded()
    {
        if (player == null && Time.time >= nextPlayerSearch)
        {
            nextPlayerSearch = Time.time + 0.5f;
            GameObject p = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (p != null) player = p.transform;
        }

        if (player != null && playerCtrl == null)
            playerCtrl = player.GetComponent<PlayerController>();
    }

    // ---------------- VIDA ----------------
    public void TakeDamage(int amount)
    {
        if (dead) return;

        currentHealth -= amount;
        if (healthBar != null) healthBar.SetValue((float)currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (player != null) chasing = true;

        StopAllCoroutines();
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        busy = true;
        anim.SetMovement(Vector2.zero);
        anim.PlayHurt();
        yield return new WaitForSeconds(hurtDuration);
        busy = false;
    }

    private void Die()
    {
        dead = true;
        StopAllCoroutines();
        anim.PlayDeath();

        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        if (healthBar != null) healthBar.gameObject.SetActive(false);
        Invoke(nameof(Win), victoryDelay);
    }

    private void Win()
    {
        if (gameEnd != null) gameEnd.ShowVictory();
    }

    [ContextMenu("Probar daño")]
    private void TestDamage() => TakeDamage(5);

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? (Vector3)spawnPoint : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, wanderRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}