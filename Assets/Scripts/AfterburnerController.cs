using System.Collections;
using UnityEngine;

public class AfterburnerController : MonoBehaviour
{
    public Vector2 targetScale = Vector2.zero;
    public float duration = 0.4f;

    private Vector3 originalLocalScale;
    private Coroutine shrinkCoroutine;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        originalLocalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        transform.localScale = originalLocalScale;
    }

    void OnDisable()
    {
        if (shrinkCoroutine != null)
        {
            StopCoroutine(shrinkCoroutine);
            shrinkCoroutine = null;
        }
    }

    public void SetColor(Color32 color)
    {
        spriteRenderer.color = color;
        shrinkCoroutine = StartCoroutine(ShrinkAndFadeOverTime(duration));
    }

    IEnumerator ShrinkAndFadeOverTime(float time)
    {
        Vector2 startScale = transform.localScale;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        float currentTime = 0f;

        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            // Interpolates linearly between start and target.
            float t = currentTime / time;
            transform.localScale = Vector2.Lerp(startScale, targetScale, t);
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        transform.localScale = targetScale; // Ensure exact final scale
        spriteRenderer.color = endColor;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}
