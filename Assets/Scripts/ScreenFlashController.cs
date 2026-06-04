using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenFlashController : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private GameObject player;
    private PlayerController playerController;

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerController.OnPlayerHitByRock += (s, e) => Flash(ColorPalette.Pink);
        playerController.OnPlayerHitByWall += (s, e) => Flash(ColorPalette.Pink);
        playerController.OnFuelCellCollected += (s, e) => Flash(ColorPalette.Green);
        playerController.OnCrystalCollectedSfx += (s, e) => Flash(ColorPalette.Cyan);
    }

    private void Flash(Color32 color)
    {
        flashImage.color = new Color32(color.r, color.g, color.b, 0);
        flashImage.DOFade(0.15f, 0f)
                  .OnComplete(() => flashImage.DOFade(0f, 0.15f));
    }
}