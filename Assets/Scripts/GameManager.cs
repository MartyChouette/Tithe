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

    private DungeonGenerator dungeonGenerator;
    private VictoryScreen victoryScreen;
    private AdvancedGridMovement playerMovement;

    public GameState State { get; private set; }
    public int CurrentFloorIndex { get; private set; }
    public FloorData CurrentFloor
    {
        get
        {
            if (floors == null || CurrentFloorIndex < 0 || CurrentFloorIndex >= floors.Length)
            {
                Debug.LogError("[GameManager] CurrentFloor: index out of bounds or floors array is null.");
                return null;
            }
            return floors[CurrentFloorIndex];
        }
    }

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
        if (floors == null || floors.Length == 0)
        {
            Debug.LogError("[GameManager] floors array is null or empty. Cannot start game.");
            return;
        }
        State = GameState.Exploring;
        hud.SetVisible(true);
    }

    public void Initialize(FloorData[] floorArray, PlayerStats stats, MaskInventory inventory,
        CombatManager combat, TithePlayerController controller, HUD hudRef,
        DungeonGenerator dungeon, VictoryScreen victory, AdvancedGridMovement movement)
    {
        floors = floorArray;
        playerStats = stats;
        maskInventory = inventory;
        combatManager = combat;
        playerController = controller;
        hud = hudRef;
        dungeonGenerator = dungeon;
        victoryScreen = victory;
        playerMovement = movement;
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

        // Last floor — game complete
        if (CurrentFloorIndex >= floors.Length - 1)
        {
            EndCombat(true);
            GameComplete();
            return;
        }

        EndCombat(true);
    }

    public void DescendToNextFloor()
    {
        if (floors == null) return;
        if (CurrentFloorIndex < floors.Length - 1)
        {
            CurrentFloorIndex++;
            playerStats.FullHeal();

            if (dungeonGenerator != null)
            {
                dungeonGenerator.ClearFloor();
                Vector3 startPos = dungeonGenerator.GenerateFloor(CurrentFloorIndex);
                if (playerMovement != null)
                    GridMovementHelper.TeleportTo(playerMovement, startPos);
            }

            hud.Refresh();
        }
    }

    public void GameComplete()
    {
        State = GameState.Paused;
        hud.SetVisible(false);
        playerController.enabled = false;
        if (victoryScreen != null)
            victoryScreen.Show(maskInventory);
    }

    public void PlayerDefeated()
    {
        State = GameState.Paused;
        hud.SetVisible(false);
        playerController.enabled = false;
    }
}
