using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- Nombres EXACTOS de los parámetros del Animator ---
    private const string P_FACE_X         = "FACEX";
    private const string P_FACE_Y         = "FACEY";
    private const string P_IS_MOVING      = "IS_MOVING";
    private const string P_HAS_WEAPON     = "HAS_WEAPON";
    private const string P_HURT           = "HURT";
    private const string P_DIE            = "DIE";
    private const string P_SPECIAL_ATTACK = "SPECIAL_ATTACK";

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Estado")]
    [SerializeField] private bool hasWeapon = false;

    [Header("Duraciones (segundos) - ajusta a tus clips")]
    [SerializeField] private float specialAttackDuration = 0.6f;
    [SerializeField] private float hurtDuration = 0.4f;
    [SerializeField] private GameEndController gameEnd;
    
    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    private int health;

    [Header("Ataque con pala")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackHitDelay = 0.25f;  // cuándo pega dentro de la animación
    [SerializeField] private float attackReach = 0.8f;      // qué tan lejos delante del jugador
    [SerializeField] private Vector2 attackSize = new Vector2(0.9f, 0.9f);
    
    [Header("Flip")]
    [Tooltip("Marca esto si el sprite original mira a la DERECHA")]
    [SerializeField] private bool spriteFacesRight = true;
    private SpriteRenderer sr;

    public bool IsDead => isDead;
    public bool HasWeapon => hasWeapon;
    

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 input;
    private Vector2 facing = Vector2.down;   // empieza mirando al frente
    private bool isDead = false;
    private bool isLocked = false;           // bloquea movimiento (ataque/daño)
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        animator.SetBool(P_HAS_WEAPON, hasWeapon);
        UpdateFacingAnimator();
        health = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        // --- Movimiento ---
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (isLocked) input = Vector2.zero;

        // Ajustar a 4 direcciones (prioriza el eje con más input)
        if (input != Vector2.zero)
        {
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
                facing = new Vector2(Mathf.Sign(input.x), 0);
            else
                facing = new Vector2(0, Mathf.Sign(input.y));

            UpdateFacingAnimator();
        }

        animator.SetBool(P_IS_MOVING, input != Vector2.zero);

        // --- Ataque especial (tecla J, solo con arma) ---
        if (Input.GetMouseButtonDown(0) && hasWeapon && !isLocked)
        {
            animator.SetTrigger(P_SPECIAL_ATTACK);
            LockMovement(specialAttackDuration);
            Invoke(nameof(DoAttackHit), attackHitDelay);
        }

        // --- Pruebas: daño (H) y muerte (K) ---
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage();
        if (Input.GetKeyDown(KeyCode.K)) Die();
    }

    void FixedUpdate()
    {
        if (isDead || isLocked)
        {
            rb.linearVelocity = Vector2.zero;   // Unity 2022 o anterior: rb.velocity
            return;
        }

        rb.linearVelocity = input.normalized * moveSpeed;   // Unity 2022 o anterior: rb.velocity
    }

    void UpdateFacingAnimator()
    {
        animator.SetFloat(P_FACE_X, facing.x);   // -1 izquierda, 1 derecha
        animator.SetFloat(P_FACE_Y, facing.y);   // -1 abajo, 1 arriba
    }

    // Lo llama la pala al tomarla o soltarla
    public void SetWeapon(bool value)
    {
        hasWeapon = value;
        animator.SetBool(P_HAS_WEAPON, hasWeapon);
    }

    void DoAttackHit()
    {
        if (isDead) return;
        Vector2 center = (Vector2)transform.position + facing * attackReach;

        foreach (var hit in Physics2D.OverlapBoxAll(center, attackSize, 0f))
        {
            var enemy = hit.GetComponentInParent<EnemyController>();
            if (enemy != null) enemy.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage()
    {
        if (isDead) return;
        health--;
        if (health <= 0) { Die(); return; }
        animator.SetTrigger(P_HURT);
        LockMovement(hurtDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + facing * attackReach, attackSize);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        input = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool(P_IS_MOVING, false);
        animator.SetTrigger(P_DIE);
        Invoke(nameof(NotifyDefeat), 1.5f); // deja ver la animación de muerte
    }
    
    void NotifyDefeat()
    {
        if (gameEnd != null) gameEnd.ShowDefeat();
    }

    void LockMovement(float seconds)
    {
        isLocked = true;
        CancelInvoke(nameof(Unlock));
        Invoke(nameof(Unlock), seconds);
    }

    void Unlock() => isLocked = false;
}