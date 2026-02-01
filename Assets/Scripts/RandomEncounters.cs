using UnityEngine;

/// <summary>
/// Attach to the player. Wire OnStep() to AdvancedGridMovement's stepEvent
/// in the Inspector so random encounters roll on each tile moved.
/// </summary>
public class RandomEncounters : MonoBehaviour
{
    [SerializeField] private int minEnemies = 1;
    [SerializeField] private int maxEnemies = 3;

    public void OnStep()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Exploring) return;

        FloorData floor = GameManager.Instance.CurrentFloor;
        if (floor == null || floor.enemyTable == null || floor.enemyTable.Length == 0) return;
        if (Random.value > floor.encounterRate) return;

        int count = Random.Range(minEnemies, maxEnemies + 1);
        EnemyData[] enemies = new EnemyData[count];
        for (int i = 0; i < count; i++)
            enemies[i] = floor.enemyTable[Random.Range(0, floor.enemyTable.Length)];

        GameManager.Instance.StartCombat(enemies);
    }
}
