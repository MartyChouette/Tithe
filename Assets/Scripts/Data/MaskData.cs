using UnityEngine;

[CreateAssetMenu(fileName = "NewMask", menuName = "Tithe/Mask")]
public class MaskData : ScriptableObject
{
    [Header("Identity")]
    public string maskName;
    [TextArea] public string description;
    public Element element;

    [Header("Visuals")]
    public GameObject modelPrefab;
    public Sprite bossSprite;

    [Header("Boss Stats")]
    public int maxHP;
    public int attack;
    public int defense;
    public int speed;

    [Header("Moves")]
    public MoveData[] moves;

    [Header("Player Bonuses When Equipped")]
    public int bonusAttack;
    public int bonusDefense;
    public int bonusSpeed;
}
