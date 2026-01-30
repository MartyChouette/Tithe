using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int baseMaxHP = 100;
    [SerializeField] private int baseAttack = 10;
    [SerializeField] private int baseDefense = 10;
    [SerializeField] private int baseSpeed = 10;

    public int MaxHP => baseMaxHP;
    public int CurrentHP { get; set; }
    public int Attack => baseAttack + (EquippedMask != null ? EquippedMask.bonusAttack : 0);
    public int Defense => baseDefense + (EquippedMask != null ? EquippedMask.bonusDefense : 0);
    public int Speed => baseSpeed + (EquippedMask != null ? EquippedMask.bonusSpeed : 0);

    public MaskData EquippedMask { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    void Start()
    {
        CurrentHP = MaxHP;
    }

    public void EquipMask(MaskData mask)
    {
        EquippedMask = mask;
    }

    public MoveData[] GetAvailableMoves()
    {
        if (EquippedMask != null)
            return EquippedMask.moves;
        return new MoveData[0];
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }
}
