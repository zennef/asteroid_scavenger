using System.Collections;
using UnityEngine;

public class AfterburnerController : MonoBehaviour
{
    public Vector2 targetScale = Vector2.zero;
    public float duration = 0.4f;

    private Vector3 originalLocalScale;
    private Coroutine shrinkCoroutine;

    void Awake()
    {
        originalLocalScale = transform.localScale;
    }

    void OnEnable()
    {
        transform.localScale = originalLocalScale;
        shrinkCoroutine = StartCoroutine(ShrinkOverTime(duration));
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
        GetComponent<SpriteRenderer>().color = color;
    }

    IEnumerator ShrinkOverTime(float time)
    {
        Vector2 startScale = transform.localScale;
        float currentTime = 0f;

        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            // Interpolates linearly between start and target.
            transform.localScale = Vector2.Lerp(startScale, targetScale, currentTime / time);
            yield return null;
        }

        transform.localScale = targetScale; // Ensure exact final scale
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}
