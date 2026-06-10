using UnityEngine;

public class HitSparkController : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparkSystem;
    [SerializeField] private GameObject player;
    private PlayerController playerController;

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerController.OnPlayerHitByRock += (s, e) => TriggerSparks(ColorPalette.Orange, transform.position);
        playerController.OnPlayerHitByWall += (s, e) => TriggerSparks(ColorPalette.Pink, transform.position);
    }

    private void TriggerSparks(Color32 color, Vector3 position)
    {
        var main = sparkSystem.main;
        Color hdrColor = new Color(color.r / 255f * 2f, color.g / 255f * 2f, color.b / 255f * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(hdrColor);
        sparkSystem.Play();
    }
}