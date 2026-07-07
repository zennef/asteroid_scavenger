using UnityEngine;
using UnityEngine.UI;

public class ShieldBarController : MonoBehaviour
{
    public Slider slider;
    [SerializeField] private GameObject shieldBarFill;
    private Image shieldBarFillImage;
    private float rechargeDuration;
    public bool isOnline = true;
    public bool isGamePaused = false;
    public bool isFrozen = false;

    void Start()
    {
        SetMaxShield(100);
        SetShield(100);
        
        isOnline = true;
        isGamePaused = false;

        shieldBarFillImage = shieldBarFill.GetComponent<Image>();
    }

    void Update()
    {

        if (!isGamePaused && !isFrozen && slider.value < slider.maxValue)
        {
            isOnline = false;
            float ratePerSecond = slider.maxValue / rechargeDuration;

            slider.value = Mathf.Min(
                slider.value + ratePerSecond * Time.deltaTime,
                slider.maxValue
            );

        }
        else
        {
            isOnline = true;
        }

        shieldBarFillImage.color =
                slider.value == slider.maxValue
                ? ColorPalette.Blue
                : ColorPalette.White;
    }


    public void togglePause()
    {
        isGamePaused = !isGamePaused;

    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
    }

    public void SetMaxShield(int maxValue)
    {
        slider.maxValue = maxValue;
        slider.value = maxValue;
    }

    public void SetShield(int value)
    {
        slider.value = value;
    }

    public void SetRechargeDuration(float duration)
    {
        rechargeDuration = duration;
    }

}
