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
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (col != null) col.enabled = true;
        if (label != null) label.enabled = false;
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
