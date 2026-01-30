using UnityEngine;

[CreateAssetMenu(fileName = "NewFloor", menuName = "Tithe/Floor")]
public class FloorData : ScriptableObject
{
    public int floorNumber;
    public string floorName;

    [Header("Random Encounters")]
    public EnemyData[] enemyTable;
    [Range(0f, 1f)] public float encounterRate;

    [Header("Boss")]
    public MaskData maskBoss;
}
