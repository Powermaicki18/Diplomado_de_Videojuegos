using UnityEngine;

public class BossAnimationTest : MonoBehaviour
{
    [SerializeField] private BossAnimationController boss;
    [SerializeField] private float speed = 2f;

    private void Start()
    {
        if (boss == null) boss = GetComponent<BossAnimationController>();
        if (boss == null) Debug.LogError("BossAnimationTest: no hay BossAnimationController asignado.");
    }

    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        boss.SetMovement(input);
        boss.transform.Translate(input.normalized * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space)) { Debug.Log("Attack"); boss.PlayAttack(); }
        if (Input.GetKeyDown(KeyCode.H))     { Debug.Log("Hurt");   boss.PlayHurt(); }
        if (Input.GetKeyDown(KeyCode.K))     { Debug.Log("Death");  boss.PlayDeath(); }
    }
}