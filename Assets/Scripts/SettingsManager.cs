using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private TextMeshProUGUI masterValueText;
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;

    private void Start()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        AudioManager.Instance.SetMasterVolume(masterSlider.value);
        AudioManager.Instance.SetMusicVolume(musicSlider.value);
        AudioManager.Instance.SetSFXVolume(sfxSlider.value);

        UpdateAllText();

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    private void OnMasterChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
        SetValueText(masterValueText, value);
    }

    private void OnMusicChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
        SetValueText(musicValueText, value);
    }

    private void OnSFXChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        SetValueText(sfxValueText, value);
    }

    private void UpdateAllText()
    {
        SetValueText(masterValueText, masterSlider.value);
        SetValueText(musicValueText, musicSlider.value);
        SetValueText(sfxValueText, sfxSlider.value);
    }

    private void SetValueText(TextMeshProUGUI text, float value)
    {
        text.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
    }
}