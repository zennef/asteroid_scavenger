using UnityEngine;
using DG.Tweening;

public class CameraShakeManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private PlayerController playerController;

    [Header("Rock Hit")]
    [SerializeField] private float rockShakeDuration = 0.25f;
    [SerializeField] private float rockShakeStrength = 0.15f;
    [SerializeField] private int rockShakeVibrato = 20;

    [Header("Wall Hit")]
    [SerializeField] private float wallShakeDuration = 0.4f;
    [SerializeField] private float wallShakeStrength = 0.3f;
    [SerializeField] private int wallShakeVibrato = 25;

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerController.OnPlayerHitByRock += (s, e) => Shake(rockShakeDuration, rockShakeStrength, rockShakeVibrato);
        playerController.OnPlayerHitByWall += (s, e) => Shake(wallShakeDuration, wallShakeStrength, wallShakeVibrato);
    }

    private void Shake(float duration, float strength, int vibrato)
    {
        transform.DOShakePosition(duration, strength, vibrato)
                 .SetUpdate(true); // works even if timeScale = 0
    }
}