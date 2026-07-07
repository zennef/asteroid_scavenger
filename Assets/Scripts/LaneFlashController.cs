using DG.Tweening;
using UnityEngine;

public class LaneFlashController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] lanes;
    [SerializeField] private GameObject gameManager;
    [SerializeField] private Color flashColor = new Color32(255, 255, 255, 255);
    [SerializeField] private float flashDuration = 0.15f;

    private GameManager gameManagerScript;
    private Color[] restingColors;
    private bool isLowTimeActive = false;
    private int lastFlashSecond = -1;

    void Awake()
    {
        restingColors = new Color[lanes.Length];
        for (int i = 0; i < lanes.Length; i++)
        {
            restingColors[i] = lanes[i].color;
        }
    }

    void Start()
    {
        gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.OnLevelStarted += Handle_LevelStarted;
        gameManagerScript.OnLevelEnded += Handle_LevelEnded;
        gameManagerScript.OnGameOver += Handle_GameOver;
    }

    private void OnDestroy()
    {
        if (gameManagerScript != null)
        {
            gameManagerScript.OnLevelStarted -= Handle_LevelStarted;
            gameManagerScript.OnLevelEnded -= Handle_LevelEnded;
            gameManagerScript.OnGameOver -= Handle_GameOver;
        }
    }

    private void Handle_LevelStarted(object sender, System.EventArgs e)
    {
        isLowTimeActive = true;
        lastFlashSecond = -1;
    }

    private void Handle_LevelEnded(object sender, System.EventArgs e)
    {
        isLowTimeActive = false;
        ResetLanes();
    }

    private void Handle_GameOver(object sender, System.EventArgs e)
    {
        isLowTimeActive = false;
        ResetLanes();
    }

    void Update()
    {
        if (!isLowTimeActive) return;
        float t = gameManagerScript.timeRemaining;
        if (t <= 0f || t > 10f) return;

        int wholeSecond = Mathf.FloorToInt(t);
        if (wholeSecond != lastFlashSecond)
        {
            lastFlashSecond = wholeSecond;
            FlashLanes();
        }
    }

    private void FlashLanes()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            SpriteRenderer lane = lanes[i];
            DOTween.Kill(lane);
            lane.color = flashColor;
            lane.DOColor(restingColors[i], flashDuration).SetUpdate(true).SetTarget(lane);
        }
    }

    private void ResetLanes()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            SpriteRenderer lane = lanes[i];
            DOTween.Kill(lane);
            lane.color = restingColors[i];
        }
    }
}
