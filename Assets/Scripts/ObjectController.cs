using DG.Tweening;
using TMPro;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshPro label;

    private Collider2D col;
    private GameObject player;
    private PlayerController playerController;
    private bool isGamePaused;

    public bool IsConsumed { get; private set; }

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        IsConsumed = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (col != null) col.enabled = true;
        if (label != null) label.enabled = false;
        if (gameObject.CompareTag("Crystal") || gameObject.CompareTag("Fuel"))
        {
            Color restingColor = gameObject.CompareTag("Crystal") ? (Color)ColorPalette.Cyan : (Color)ColorPalette.Green;
            float interval = gameObject.CompareTag("Crystal") ? 0.5f : 1f;
            spriteRenderer.color = restingColor;
            DOTween.Kill(spriteRenderer);
            var seq = DOTween.Sequence()
                .AppendInterval(interval)
                .Append(spriteRenderer.DOColor(Color.white, 0.05f))
                .Append(spriteRenderer.DOColor(restingColor, 0.05f))
                .Append(spriteRenderer.DOColor(Color.white, 0.05f))
                .Append(spriteRenderer.DOColor(restingColor, 0.05f));
            seq.SetLoops(-1, LoopType.Restart)
               .SetUpdate(true)
               .SetAutoKill(false)
               .SetTarget(spriteRenderer);
        }
    }

    private void OnDisable()
    {
        DOTween.Kill(spriteRenderer);
        if (gameObject.CompareTag("Crystal") || gameObject.CompareTag("Fuel"))
            spriteRenderer.color = gameObject.CompareTag("Crystal") ? (Color)ColorPalette.Cyan : (Color)ColorPalette.Green;
    }

    void Start()
    {
        isGamePaused = false;
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        playerController.OnGamePaused += PlayerController_HandleGamePaused;
    }

    private void PlayerController_HandleGamePaused(object sender, PlayerController.OnGamePausedArgs e)
    {
        isGamePaused = e.IsGamePaused;
    }

    void Update()
    {
        if (!isGamePaused)
        {
            transform.Translate(Vector3.down * Time.deltaTime * speed);
        }
    }

public void Consume(string text, Color32 color)
    {
        IsConsumed = true;
        spriteRenderer.enabled = false;
        col.enabled = false;
        label.text = text;
        label.color = color;
        label.enabled = true;
    }
}
