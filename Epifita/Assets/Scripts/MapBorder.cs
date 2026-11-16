using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MapBorder : MonoBehaviour
{
    private void Awake()
    {
        Bounds b = GetComponent<SpriteRenderer>().bounds;

        var edge = gameObject.AddComponent<EdgeCollider2D>();
        Vector2 min = b.min;
        Vector2 max = b.max;

        edge.points = new Vector2[]
        {
            new Vector2(min.x, min.y),
            new Vector2(max.x, min.y),
            new Vector2(max.x, max.y),
            new Vector2(min.x, max.y),
            new Vector2(min.x, min.y)
        };
    }
}