using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelExit : MonoBehaviour
{
    [SerializeField] private string nextScene = "Level2";
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float blackHold = 0.3f;

    private bool triggered;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(Transition(other.GetComponent<PlayerController>()));
    }

    private IEnumerator Transition(PlayerController player)
    {
        if (player != null) player.enabled = false;   // bloquea el control durante el fundido
        var rb = player != null ? player.GetComponent<Rigidbody2D>() : null;
        if (rb != null) rb.linearVelocity = Vector2.zero;   // Unity 2022 o anterior: rb.velocity

        // Pantalla negra creada por código, sin tocar tu Canvas
        var canvasGO = new GameObject("FadeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var imgGO = new GameObject("Black");
        imgGO.transform.SetParent(canvasGO.transform, false);
        var img = imgGO.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            img.color = new Color(0f, 0f, 0f, t / fadeDuration);
            yield return null;
        }
        img.color = Color.black;

        yield return new WaitForSecondsRealtime(blackHold);
        SceneManager.LoadScene(nextScene);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0f, 0f, 0.5f);
        var box = GetComponent<BoxCollider2D>();
        if (box != null) Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
    }
}