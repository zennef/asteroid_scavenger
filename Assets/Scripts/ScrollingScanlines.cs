using UnityEngine;
using UnityEngine.UI;

public class ScrollingScanlines : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 8.25f;
    private RawImage rawImage;
    private float offset;
    private bool isPaused;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        offset += Time.deltaTime * scrollSpeed;
        if (offset > 1f) offset -= 1f;
        rawImage.uvRect = new Rect(0, offset, rawImage.uvRect.width, rawImage.uvRect.height);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
}