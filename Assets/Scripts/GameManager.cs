using UnityEngine;

public enum GameState
{
    Exploring,
    Combat,
    Paused
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private FloorData[] floors;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private MaskInventory maskInventory;
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private TithePlayerController playerController;
    [SerializeField] private HUD hud;

    public GameState State { get; private set; }
    public int CurrentFloorIndex { get; private set; }
    public FloorData CurrentFloor => floors[CurrentFloorIndex];

    private BossEncounter activeBossEncounter;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        State = GameState.Exploring;
        hud.SetVisible(true);
    }

    public void SetActiveBossEncounter(BossEncounter encounter)
    {
        activeBossEncounter = encounter;
    }

    public void StartCombat(EnemyData[] enemies)
    {
        State = GameState.Combat;
        hud.SetVisible(false);
        playerController.enabled = false;
        combatManager.BeginCombat(enemies);
    }

    public void StartBossCombat(MaskData boss)
    {
        State = GameState.Combat;
        hud.SetVisible(false);
        playerController.enabled = false;
        combatManager.BeginBossCombat(boss);
    }

    public void EndCombat(bool victory)
    {
        State = GameState.Exploring;
        hud.SetVisible(true);
        playerController.enabled = true;
        if (victory)
            hud.Refresh();
    }

    public void BossDefeated(MaskData mask)
    {
        maskInventory.AddMask(mask);
        if (activeBossEncounter != null)
        {
            activeBossEncounter.OnBossDefeated();
            activeBossEncounter = null;
        }
        EndCombat(true);
    }

    public void DescendToNextFloor()
    {
        if (CurrentFloorIndex < floors.Length - 1)
        {
            CurrentFloorIndex++;
            playerStats.FullHeal();
            hud.Refresh();
            // TODO: load or activate next floor layout
        }
    }

    public void PlayerDefeated()
    {
        State = GameState.Paused;
        hud.SetVisible(false);
        // TODO: game over screen
    }
}
