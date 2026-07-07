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
        masterSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("MasterVolume", 0.5f) * 10f);
        musicSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("MusicVolume", 0.5f) * 10f);
        sfxSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("SFXVolume", 1f) * 10f);

        AudioManager.Instance.SetMasterVolume(masterSlider.value / 10f);
        AudioManager.Instance.SetMusicVolume(musicSlider.value / 10f);
        AudioManager.Instance.SetSFXVolume(sfxSlider.value / 10f);

        UpdateAllText();

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    private void OnMasterChanged(float value)
    {
        float volume = value / 10f;
        AudioManager.Instance.SetMasterVolume(volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
        SetValueText(masterValueText, value);
    }

    private void OnMusicChanged(float value)
    {
        float volume = value / 10f;
        AudioManager.Instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        SetValueText(musicValueText, value);
    }

    private void OnSFXChanged(float value)
    {
        float volume = value / 10f;
        AudioManager.Instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
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
        text.text = Mathf.RoundToInt(value * 10f) + "%";
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
    }
}