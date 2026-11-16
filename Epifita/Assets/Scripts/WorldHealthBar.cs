using UnityEngine;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(1f, 0.12f);
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);
    [SerializeField] private Color backColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Color fillColor = new Color(0.9f, 0.15f, 0.15f);
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private int sortingOrder = 500;

    private Transform root, fill;
    private static Sprite square;

    private void Awake()
    {
        if (square == null)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            square = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }

        root = new GameObject("HealthBar").transform;
        root.SetParent(transform, false);
        root.localPosition = new Vector3(offset.x - size.x * 0.5f, offset.y, 0f);

        MakePart("Back", backColor, sortingOrder).localScale = new Vector3(size.x, size.y, 1f);
        fill = MakePart("Fill", fillColor, sortingOrder + 1);
        fill.localScale = new Vector3(size.x, size.y, 1f);

        if (hideWhenFull) root.gameObject.SetActive(false);
    }

    private Transform MakePart(string name, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = square;
        r.color = color;
        r.sortingOrder = order;
        return go.transform;
    }

    public void SetValue(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        fill.localScale = new Vector3(size.x * normalized, size.y, 1f);
        root.gameObject.SetActive(!(hideWhenFull && normalized >= 1f) && normalized > 0f);
    }

    public void Hide() => root.gameObject.SetActive(false);
}