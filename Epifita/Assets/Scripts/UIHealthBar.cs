using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private float smooth = 2f;

    private float target = 1f;

    public void SetValue(float normalized) => target = Mathf.Clamp01(normalized);

    private void Update()
    {
        fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, target, smooth * Time.unscaledDeltaTime);
    }
}