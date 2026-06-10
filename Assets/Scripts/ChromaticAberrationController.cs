using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class ChromaticAberrationController : MonoBehaviour
{
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private GameObject player;
    [SerializeField] private float hitIntensity = 0.8f;
    [SerializeField] private float recoverDuration = 0.4f;

    private ChromaticAberration chromaticAberration;
    private PlayerController playerController;

    void Start()
    {
        postProcessVolume.profile.TryGet(out chromaticAberration);

        playerController = player.GetComponent<PlayerController>();
        playerController.OnPlayerHitByRock += (s, e) => Trigger(hitIntensity);
        playerController.OnPlayerHitByWall += (s, e) => Trigger(hitIntensity * 1.5f);
    }

    private void Trigger(float intensity)
    {
        DOTween.Kill(chromaticAberration);
        chromaticAberration.intensity.value = Mathf.Clamp01(intensity);
        DOTween.To(
            () => chromaticAberration.intensity.value,
            x => chromaticAberration.intensity.value = x,
            0f,
            recoverDuration
        );
    }
}