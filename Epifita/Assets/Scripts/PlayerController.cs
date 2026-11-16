using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    [Header("Duraciones (segundos)")]
    [SerializeField] private float specialAttackDuration = 0.6f;
    [SerializeField] private float hurtDuration = 0.4f;

    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    [Header("Regeneración")]
    [SerializeField] private float regenDelay = 5f;      // segundos sin daño antes de empezar
    [SerializeField] private float regenInterval = 2f;   // segundos entre cada punto recuperado
    [SerializeField] private int regenAmount = 1;        // puntos que recupera cada vez

	[Header("Continuidad entre niveles")]
	[SerializeField] private bool startWithWeaponInThisLevel = false;
	public static bool CarriesWeapon;   // se mantiene al cambiar de escena

    private float lastDamageTime = -999f;
    private float nextRegenTime;

    [Header("Ataque con pala")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackHitDelay = 0.25f;
    [SerializeField] private float attackReach = 0.8f;
    [SerializeField] private Vector2 attackSize = new Vector2(0.9f, 0.9f);
    [Tooltip("Marca esto si el sprite de ataque original mira a la DERECHA")]
    [SerializeField] private bool attackSpriteFacesRight = true;

    [Header("Fin del juego")]
    [SerializeField] private GameEndController gameEnd;

    public bool IsDead => isDead;
    public bool HasWeapon => hasWeapon;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private WorldHealthBar healthBar;

    private Vector2 input;
    private Vector2 facing = Vector2.down;
    private bool isDead = false;
    private bool isLocked = false;
    private int health;

    private bool attackFlip;      // true mientras dura el golpe
    private float attackSide = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        health = maxHealth;

        healthBar = GetComponent<WorldHealthBar>();
        if (healthBar != null) healthBar.SetValue(1f);


		if (startWithWeaponInThisLevel || CarriesWeapon) hasWeapon = true;
        animator.SetBool(P_HAS_WEAPON, hasWeapon);
        UpdateFacingAnimator();
    }

    void Update()
    {
        if (isDead) return;
        
        // --- Regeneración de vida ---
        if (health < maxHealth && Time.time >= lastDamageTime + regenDelay && Time.time >= nextRegenTime)
        {
            health = Mathf.Min(maxHealth, health + regenAmount);
            nextRegenTime = Time.time + regenInterval;
            if (healthBar != null) healthBar.SetValue((float)health / maxHealth);
        }

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (isLocked) input = Vector2.zero;

        if (input != Vector2.zero)
        {
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
                facing = new Vector2(Mathf.Sign(input.x), 0f);
            else
                facing = new Vector2(0f, Mathf.Sign(input.y));

            UpdateFacingAnimator();
        }

        animator.SetBool(P_IS_MOVING, input != Vector2.zero);

        // --- Ataque con clic izquierdo (solo con pala) ---
        if (Input.GetMouseButtonDown(0) && hasWeapon && !isLocked)
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            attackSide = mouse.x >= transform.position.x ? 1f : -1f;

            facing = new Vector2(attackSide, 0f);
            UpdateFacingAnimator();

            attackFlip = true;   // el flip real se aplica en LateUpdate

            animator.SetTrigger(P_SPECIAL_ATTACK);
            LockMovement(specialAttackDuration);
            Invoke(nameof(DoAttackHit), attackHitDelay);
        }

        // --- Pruebas: daño (H) y muerte (K) ---
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage();
        if (Input.GetKeyDown(KeyCode.K)) Die();
    }

    // Corre DESPUÉS del Animator, así que no puede sobrescribir el flip
    void LateUpdate()
    {
        if (sr == null || !attackFlip) return;
        sr.flipX = attackSpriteFacesRight ? attackSide < 0f : attackSide > 0f;
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
        animator.SetFloat(P_FACE_X, facing.x);
        animator.SetFloat(P_FACE_Y, facing.y);
    }

    public void SetWeapon(bool value)
    {
        hasWeapon = value;
        animator.SetBool(P_HAS_WEAPON, hasWeapon);
		CarriesWeapon = value;
    }

    void DoAttackHit()
    {
        if (isDead) return;
        Vector2 center = (Vector2)transform.position + facing * attackReach;

        foreach (var hit in Physics2D.OverlapBoxAll(center, attackSize, 0f))
        {
            var target = hit.GetComponentInParent<IDamageable>();
            if (target != null) target.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage()
    {
        if (isDead) return;
        health--;
        lastDamageTime = Time.time;
        if (healthBar != null) healthBar.SetValue((float)health / maxHealth);
        if (health <= 0) { Die(); return; }
        animator.SetTrigger(P_HURT);
        LockMovement(hurtDuration);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        attackFlip = false;
        sr.flipX = false;
        input = Vector2.zero;
        rb.linearVelocity = Vector2.zero;   // Unity 2022 o anterior: rb.velocity
        animator.SetBool(P_IS_MOVING, false);
        animator.SetTrigger(P_DIE);
        if (healthBar != null) healthBar.SetValue(0f);
        Invoke(nameof(NotifyDefeat), 1.5f);
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

    void Unlock()
    {
        isLocked = false;
        attackFlip = false;
        if (sr != null) sr.flipX = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + facing * attackReach, attackSize);
    }
}