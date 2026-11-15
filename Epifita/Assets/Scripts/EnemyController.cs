using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int health = 3;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Transform player;
    private float nextAttack;
    private bool dead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (dead || player == null) { rb.linearVelocity = Vector2.zero; return; }

        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 dir = (player.position - transform.position).normalized;

        if (dist <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IS_MOVING", false);
            if (Time.time >= nextAttack)
            {
                nextAttack = Time.time + attackCooldown;
                animator.SetTrigger("ATTACK");
                player.GetComponent<PlayerController>()?.TakeDamage();
            }
        }
        else if (dist <= detectRange)
        {
            rb.linearVelocity = dir * moveSpeed;
            animator.SetBool("IS_MOVING", true);
            sr.flipX = dir.x < 0;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IS_MOVING", false);
        }
    }

    public void TakeDamage(int amount = 1)
    {
        if (dead) return;
        health -= amount;
        if (health <= 0) Die();
    }

    void Die()
    {
        dead = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        animator.SetTrigger("DIE");
        Destroy(gameObject, 1f);
    }
}