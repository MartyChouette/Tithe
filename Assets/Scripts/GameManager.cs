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
    }

    public void StartCombat(EnemyData[] enemies)
    {
        State = GameState.Combat;
        playerController.enabled = false;
        combatManager.BeginCombat(enemies);
    }

    public void StartBossCombat(MaskData boss)
    {
        State = GameState.Combat;
        playerController.enabled = false;
        combatManager.BeginBossCombat(boss);
    }

    public void EndCombat(bool victory)
    {
        State = GameState.Exploring;
        playerController.enabled = true;
        if (victory)
            hud.Refresh();
    }

    public void BossDefeated(MaskData mask)
    {
        maskInventory.AddMask(mask);
        EndCombat(true);
    }

    public void DescendToNextFloor()
    {
        if (CurrentFloorIndex < floors.Length - 1)
        {
            CurrentFloorIndex++;
            hud.Refresh();
            // TODO: load or activate next floor layout
        }
    }

    public void PlayerDefeated()
    {
        State = GameState.Paused;
        // TODO: game over screen
    }
}
