using System.Collections.Generic;
using UnityEngine;

public class MaskInventory : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    private List<MaskData> collectedMasks = new List<MaskData>();

    public IReadOnlyList<MaskData> CollectedMasks => collectedMasks;

    public void Initialize(PlayerStats stats)
    {
        playerStats = stats;
    }

    public void AddMask(MaskData mask)
    {
        if (!collectedMasks.Contains(mask))
        {
            collectedMasks.Add(mask);

            if (playerStats == null)
            {
                Debug.LogError("[MaskInventory] playerStats is not assigned.", this);
                return;
            }

            if (collectedMasks.Count == 1)
                playerStats.EquipMask(mask);
        }
    }

    public void EquipMask(int index)
    {
        if (playerStats == null)
        {
            Debug.LogError("[MaskInventory] playerStats is not assigned.", this);
            return;
        }
        if (index >= 0 && index < collectedMasks.Count)
            playerStats.EquipMask(collectedMasks[index]);
    }

    public void EquipMask(MaskData mask)
    {
        if (playerStats == null)
        {
            Debug.LogError("[MaskInventory] playerStats is not assigned.", this);
            return;
        }
        if (collectedMasks.Contains(mask))
            playerStats.EquipMask(mask);
    }

    public bool HasMask(MaskData mask)
    {
        return collectedMasks.Contains(mask);
    }
}
