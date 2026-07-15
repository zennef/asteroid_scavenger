using TMPro;
using UnityEngine;

public class VersionDisplayController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private int buildNumber = 1; // bump this by hand before each real build

    void Start()
    {
        versionText.text = $"v{Application.version} ({buildNumber})";
    }
}