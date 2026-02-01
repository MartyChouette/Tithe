using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int baseMaxHP = 100;
    [SerializeField] private int baseAttack = 10;
    [SerializeField] private int baseDefense = 10;
    [SerializeField] private int baseSpeed = 10;

    public int MaxHP => baseMaxHP + (EquippedMask != null ? EquippedMask.bonusHP : 0);
    public int CurrentHP { get; private set; }
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
        int oldMax = MaxHP;
        EquippedMask = mask;
        int newMax = MaxHP;
        if (oldMax > 0)
            CurrentHP = Mathf.Clamp(Mathf.RoundToInt((float)CurrentHP / oldMax * newMax), 1, newMax);
        else
            CurrentHP = newMax;
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

    public void FullHeal()
    {
        CurrentHP = MaxHP;
    }
}
