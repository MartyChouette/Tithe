using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Tithe/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite sprite;
    public Element element;

    [Header("Stats")]
    public int maxHP;
    public int attack;
    public int defense;
    public int speed;

    [Header("Moves")]
    public MoveData[] moves;
}
