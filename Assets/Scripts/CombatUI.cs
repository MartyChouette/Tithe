using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject combatPanel;
    [SerializeField] private GameObject actionMenuPanel;
    [SerializeField] private GameObject moveListPanel;
    [SerializeField] private GameObject targetPanel;
    [SerializeField] private GameObject messagePanel;

    [Header("HP Display")]
    [SerializeField] private Text playerHPText;
    [SerializeField] private Text playerMaskText;

    [Header("Message")]
    [SerializeField] private Text messageText;

    [Header("Move Buttons")]
    [SerializeField] private Button[] moveButtons;
    [SerializeField] private Text[] moveButtonTexts;
    [SerializeField] private Button moveBackButton;

    [Header("Target Buttons")]
    [SerializeField] private Button[] targetButtons;
    [SerializeField] private Text[] targetButtonTexts;
    [SerializeField] private Button targetBackButton;

    [Header("References")]
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private PlayerStats playerStats;

    private MoveData selectedMove;

    public void Show()
    {
        combatPanel.SetActive(true);
        RefreshHP();
    }

    public void Hide()
    {
        combatPanel.SetActive(false);
    }

    public void ShowActionMenu()
    {
        actionMenuPanel.SetActive(true);
        moveListPanel.SetActive(false);
        targetPanel.SetActive(false);
        messagePanel.SetActive(false);
    }

    // Wire to Fight button OnClick
    public void OnFightPressed()
    {
        actionMenuPanel.SetActive(false);
        ShowMoveList();
    }

    // Wire to Flee button OnClick
    public void OnFleePressed()
    {
        bool fled = combatManager.OnPlayerFlee();
        if (!fled)
        {
            actionMenuPanel.SetActive(false);
            ShowMessage("Can't flee from this.");
            StartCoroutine(ReturnToActionMenuAfterDelay(1f));
        }
    }

    private IEnumerator ReturnToActionMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowActionMenu();
    }

    private void ShowMoveList()
    {
        moveListPanel.SetActive(true);
        MoveData[] moves = playerStats.GetAvailableMoves();

        for (int i = 0; i < moveButtons.Length; i++)
        {
            if (i < moves.Length)
            {
                moveButtons[i].gameObject.SetActive(true);
                moveButtonTexts[i].text = moves[i].moveName;
                int index = i;
                moveButtons[i].onClick.RemoveAllListeners();
                moveButtons[i].onClick.AddListener(() => OnMoveSelected(moves[index]));
            }
            else
            {
                moveButtons[i].gameObject.SetActive(false);
            }
        }

        if (moveBackButton != null)
        {
            moveBackButton.onClick.RemoveAllListeners();
            moveBackButton.onClick.AddListener(OnMoveBackPressed);
        }
    }

    private void OnMoveBackPressed()
    {
        moveListPanel.SetActive(false);
        ShowActionMenu();
    }

    private void OnMoveSelected(MoveData move)
    {
        selectedMove = move;

        if (move.targetType == TargetType.All)
        {
            moveListPanel.SetActive(false);
            combatManager.OnPlayerMove(move, 0);
        }
        else
        {
            moveListPanel.SetActive(false);
            ShowTargetSelection();
        }
    }

    private void ShowTargetSelection()
    {
        targetPanel.SetActive(true);
        var enemies = combatManager.Enemies;

        // Compact alive enemies into sequential button slots
        int slot = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsDead) continue;
            if (slot >= targetButtons.Length) break;

            targetButtons[slot].gameObject.SetActive(true);
            targetButtonTexts[slot].text = enemies[i].name;
            int enemyIndex = i;
            targetButtons[slot].onClick.RemoveAllListeners();
            targetButtons[slot].onClick.AddListener(() => OnTargetSelected(enemyIndex));
            slot++;
        }

        // Hide remaining slots
        for (int i = slot; i < targetButtons.Length; i++)
        {
            targetButtons[i].gameObject.SetActive(false);
        }

        if (targetBackButton != null)
        {
            targetBackButton.onClick.RemoveAllListeners();
            targetBackButton.onClick.AddListener(OnTargetBackPressed);
        }
    }

    private void OnTargetBackPressed()
    {
        targetPanel.SetActive(false);
        ShowMoveList();
    }

    private void OnTargetSelected(int index)
    {
        targetPanel.SetActive(false);
        combatManager.OnPlayerMove(selectedMove, index);
    }

    public void RefreshHP()
    {
        playerHPText.text = $"HP: {playerStats.CurrentHP} / {playerStats.MaxHP}";
        playerMaskText.text = playerStats.EquippedMask != null
            ? playerStats.EquippedMask.maskName
            : "No Mask";
    }

    public void ShowDamage(int amount, float multiplier, int targetIndex)
    {
        string suffix = "";
        if (multiplier > 1f) suffix = " WEAK!";
        else if (multiplier < 1f) suffix = " RESIST";
        ShowMessage($"{amount}{suffix}");
    }

    public void ShowEnemyAttack(string enemyName, string moveName, int damage)
    {
        ShowMessage($"{enemyName} uses {moveName}! {damage} damage!");
    }

    public void ShowMessage(string msg)
    {
        messagePanel.SetActive(true);
        messageText.text = msg;
    }
}
