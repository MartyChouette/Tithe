using System.Collections.Generic;
using UnityEngine;

public class MaskInventory : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    private List<MaskData> collectedMasks = new List<MaskData>();

    public IReadOnlyList<MaskData> CollectedMasks => collectedMasks;

    public void AddMask(MaskData mask)
    {
        if (!collectedMasks.Contains(mask))
        {
            collectedMasks.Add(mask);

            if (collectedMasks.Count == 1)
                playerStats.EquipMask(mask);
        }
    }

    public void EquipMask(int index)
    {
        if (index >= 0 && index < collectedMasks.Count)
            playerStats.EquipMask(collectedMasks[index]);
    }

    public void EquipMask(MaskData mask)
    {
        if (collectedMasks.Contains(mask))
            playerStats.EquipMask(mask);
    }

    public bool HasMask(MaskData mask)
    {
        return collectedMasks.Contains(mask);
    }
}
