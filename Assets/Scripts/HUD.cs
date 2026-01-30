using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text hpText;
    [SerializeField] private Text maskText;
    [SerializeField] private Text floorText;
    [SerializeField] private PlayerStats playerStats;

    void Update()
    {
        bool exploring = GameManager.Instance != null &&
                         GameManager.Instance.State == GameState.Exploring;
        hudPanel.SetActive(exploring);
    }

    public void Refresh()
    {
        hpText.text = $"HP: {playerStats.CurrentHP}/{playerStats.MaxHP}";
        maskText.text = playerStats.EquippedMask != null
            ? playerStats.EquippedMask.maskName
            : "No Mask";
        floorText.text = $"Floor {GameManager.Instance.CurrentFloor.floorNumber}";
    }
}
