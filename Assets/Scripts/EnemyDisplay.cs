using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDisplay : MonoBehaviour
{
    [SerializeField] private Image[] enemySlots;
    [SerializeField] private Text[] enemyHPTexts;

    public void ShowEnemies(List<CombatEnemy> enemies)
    {
        for (int i = 0; i < enemySlots.Length; i++)
        {
            if (i < enemies.Count)
            {
                enemySlots[i].gameObject.SetActive(true);
                enemySlots[i].sprite = enemies[i].sprite;
                enemySlots[i].SetNativeSize();
                enemySlots[i].color = Color.white;
                enemyHPTexts[i].text = $"{enemies[i].currentHP}/{enemies[i].maxHP}";
            }
            else
            {
                enemySlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void RefreshAll(List<CombatEnemy> enemies)
    {
        for (int i = 0; i < enemies.Count && i < enemySlots.Length; i++)
        {
            if (enemies[i].IsDead)
            {
                enemySlots[i].color = new Color(1f, 1f, 1f, 0.3f);
                enemyHPTexts[i].text = "DEAD";
            }
            else
            {
                enemyHPTexts[i].text = $"{enemies[i].currentHP}/{enemies[i].maxHP}";
            }
        }
    }

    public void PlayDeath(int index)
    {
        if (index >= 0 && index < enemySlots.Length)
            enemySlots[index].color = new Color(1f, 1f, 1f, 0.3f);
    }

    public void Clear()
    {
        if (enemySlots == null) return;
        foreach (var slot in enemySlots)
        {
            if (slot != null)
                slot.gameObject.SetActive(false);
        }
    }
}
