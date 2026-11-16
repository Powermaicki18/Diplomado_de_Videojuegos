using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private UIHealthBar bar;
    [SerializeField] private GameEndController gameEnd;
    [SerializeField] private float showBarRange = 8f;
    [SerializeField] private float victoryDelay = 1.5f;
    [SerializeField] private string dieTrigger = "DIE";

    private int health;
    private bool dead;
    private Transform player;
    private Animator animator;
    private SpriteRenderer sr;

    private void Start()
    {
        health = maxHealth;
        animator = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (bar != null) { bar.SetValue(1f); bar.gameObject.SetActive(false); }
    }

    private void Update()
    {
        if (dead || bar == null || player == null) return;
        bool near = Vector2.Distance(player.position, transform.position) <= showBarRange;
        if (bar.gameObject.activeSelf != near) bar.gameObject.SetActive(near);
    }

    public void TakeDamage(int amount = 1)
    {
        if (dead) return;
        health -= amount;
        if (bar != null) bar.SetValue((float)health / maxHealth);

        if (health <= 0) Die();
        else StartCoroutine(Flash());
    }

    private System.Collections.IEnumerator Flash()
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
        if (animator != null) animator.SetTrigger(dieTrigger);
        Invoke(nameof(Win), victoryDelay);
    }

    private void Win()
    {
        if (bar != null) bar.gameObject.SetActive(false);
        if (gameEnd != null) gameEnd.ShowVictory();
    }
}