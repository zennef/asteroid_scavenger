using TMPro;
using UnityEngine;

public class ConfirmationScreenManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI upgradeDetails;
    [SerializeField] private GameObject upgradeSlot;

    public void SetTitle(string newTitle)
    {
        title.text = newTitle;
    }

    public void SetUpgradeDetails(string details)
    {
        upgradeDetails.text = details;
    }

    public void SetUpgradeButton(GameObject upgradeButton) 
    {
        if (upgradeSlot.transform.childCount > 0) Destroy(upgradeSlot.transform.GetChild(0).gameObject);
        Instantiate(upgradeButton, upgradeSlot.transform.position, Quaternion.identity, upgradeSlot.transform);
    }

}
