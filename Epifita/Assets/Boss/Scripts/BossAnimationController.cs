using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BossAnimationController : MonoBehaviour
{
    // --- Nombres de estados (cámbialos aquí si usas otras mayúsculas) ---
    public const string WALK   = "Walk_Boss";
    public const string ATTACK = "Attack_Boss";
    public const string HURT   = "Hurt_Boss";
    public const string DEATH  = "Death_Boss";

    // --- Parámetros del Animator (cacheados como hash por rendimiento) ---
    private static readonly int MoveX     = Animator.StringToHash("MoveX");
    private static readonly int MoveY     = Animator.StringToHash("MoveY");
    private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
    private static readonly int AttackTrg = Animator.StringToHash("Attack");
    private static readonly int HurtTrg   = Animator.StringToHash("Hurt");
    private static readonly int DeathTrg  = Animator.StringToHash("Death");

    private Animator animator;
    private Vector2 lastDirection = Vector2.down; // dirección inicial
    private bool isDead;

    public bool IsDead => isDead;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        SetFacing(lastDirection);
        animator.SetFloat(WalkSpeed, 0f);
    }

    /// <summary>
    /// Llamar cada frame con la dirección de movimiento.
    /// Si el vector es (0,0), el jefe se queda quieto mirando a la última dirección.
    /// </summary>
    public void SetMovement(Vector2 direction)
    {
        if (isDead) return;

        if (direction.sqrMagnitude > 0.01f)
        {
            lastDirection = ToCardinal(direction);
            SetFacing(lastDirection);
            animator.SetFloat(WalkSpeed, 1f);
        }
        else
        {
            animator.SetFloat(WalkSpeed, 0f); // congela el frame actual
        }
    }

    /// <summary>Gira al jefe hacia un objetivo sin moverlo (útil antes de atacar).</summary>
    public void FaceDirection(Vector2 direction)
    {
        if (isDead || direction.sqrMagnitude < 0.01f) return;
        lastDirection = ToCardinal(direction);
        SetFacing(lastDirection);
    }

    public void PlayAttack()
    {
        if (isDead) return;
        animator.SetTrigger(AttackTrg);
    }

    public void PlayHurt()
    {
        if (isDead) return;
        animator.SetTrigger(HurtTrg);
    }

    public void PlayDeath()
    {
        if (isDead) return;
        isDead = true;
        animator.ResetTrigger(AttackTrg);
        animator.ResetTrigger(HurtTrg);
        animator.SetTrigger(DeathTrg);
    }

    // --- Utilidades ---
    private void SetFacing(Vector2 dir)
    {
        animator.SetFloat(MoveX, dir.x);
        animator.SetFloat(MoveY, dir.y);
    }

    // Convierte cualquier vector a una de las 4 direcciones (evita diagonales)
    private Vector2 ToCardinal(Vector2 v)
    {
        return Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
    }

    // --- Eventos de animación opcionales ---
    // Añádelos como Animation Events en el clip Attack_Boss y Death_Boss.
    public void OnAttackHit()  { /* el programador conecta aquí el daño */ }
    public void OnDeathEnd()   { /* ej. desactivar colisión, soltar loot */ }
}