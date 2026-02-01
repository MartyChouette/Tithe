using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text hpText;
    [SerializeField] private Text maskText;
    [SerializeField] private Text floorText;
    [SerializeField] private PlayerStats playerStats;

    public void Initialize(PlayerStats stats, GameObject panel, Text hp, Text mask, Text floor)
    {
        playerStats = stats;
        hudPanel = panel;
        hpText = hp;
        maskText = mask;
        floorText = floor;
    }

    public void SetVisible(bool visible)
    {
        hudPanel.SetActive(visible);
    }

    public void Refresh()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentFloor == null)
            return;

        hudPanel.SetActive(true);
        hpText.text = $"HP: {playerStats.CurrentHP}/{playerStats.MaxHP}";
        maskText.text = playerStats.EquippedMask != null
            ? playerStats.EquippedMask.maskName
            : "No Mask";
        floorText.text = $"Floor {GameManager.Instance.CurrentFloor.floorNumber}";
    }
}
