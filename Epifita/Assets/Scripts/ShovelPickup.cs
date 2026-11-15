using TMPro;
using UnityEngine;

public class ShovelPickup : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private TMP_Text prompt;           // el cartel emergente
    [SerializeField] private float pickupRadius = 1.2f;
    [SerializeField] private Vector2 promptOffset = new Vector2(0f, 1f);
    [SerializeField] private Vector2 dropOffset = new Vector2(0f, -0.3f);
    [SerializeField] private float dropHintSeconds = 2.5f;

    private SpriteRenderer shovelSprite;
    private bool held;
    private float hintUntil;

    private void Awake()
    {
        shovelSprite = GetComponent<SpriteRenderer>();
        if (prompt != null) prompt.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (player == null || player.IsDead)
        {
            ShowPrompt(false);
            return;
        }

        bool inRange = !held &&
            Vector2.Distance(player.transform.position, transform.position) <= pickupRadius;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (held) Drop();
            else if (inRange) Pickup();
        }

        // Recalcula tras el posible cambio de estado
        inRange = !held &&
            Vector2.Distance(player.transform.position, transform.position) <= pickupRadius;

        if (held)
        {
            ShowPrompt(Time.time < hintUntil, "Presiona E para dejar",
                       (Vector2)player.transform.position + promptOffset);
        }
        else
        {
            ShowPrompt(inRange, "Presiona E para tomar",
                       (Vector2)transform.position + promptOffset);
        }
    }

    private void Pickup()
    {
        held = true;
        shovelSprite.enabled = false;      // desaparece del mapa (el objeto sigue activo)
        player.SetWeapon(true);            // cambia a las animaciones con pala
        hintUntil = Time.time + dropHintSeconds;
    }

    private void Drop()
    {
        held = false;
        transform.position = (Vector2)player.transform.position + dropOffset;
        shovelSprite.enabled = true;       // reaparece donde la soltó
        player.SetWeapon(false);
    }

    private void ShowPrompt(bool show, string text = "", Vector2 pos = default)
    {
        if (prompt == null) return;
        if (prompt.gameObject.activeSelf != show) prompt.gameObject.SetActive(show);
        if (!show) return;
        prompt.text = text;
        prompt.transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}