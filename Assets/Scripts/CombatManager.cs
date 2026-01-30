using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatState
{
    Start,
    PlayerTurn,
    PlayerAttack,
    EnemyTurn,
    Victory,
    Defeat,
    Fled
}

[System.Serializable]
public class CombatEnemy
{
    public string name;
    public Sprite sprite;
    public Element element;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public int speed;
    public MoveData[] moves;
    public bool isBoss;
    public MaskData maskDrop;

    public bool IsDead => currentHP <= 0;

    public static CombatEnemy FromEnemyData(EnemyData data)
    {
        return new CombatEnemy
        {
            name = data.enemyName,
            sprite = data.sprite,
            element = data.element,
            maxHP = data.maxHP,
            currentHP = data.maxHP,
            attack = data.attack,
            defense = data.defense,
            speed = data.speed,
            moves = data.moves,
            isBoss = false,
            maskDrop = null
        };
    }

    public static CombatEnemy FromMaskData(MaskData data)
    {
        return new CombatEnemy
        {
            name = data.maskName,
            sprite = data.bossSprite,
            element = data.element,
            maxHP = data.maxHP,
            currentHP = data.maxHP,
            attack = data.attack,
            defense = data.defense,
            speed = data.speed,
            moves = data.moves,
            isBoss = true,
            maskDrop = data
        };
    }
}

public class CombatManager : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private CombatUI combatUI;
    [SerializeField] private EnemyDisplay enemyDisplay;
    [SerializeField] private MaskAttackAnimator maskAttackAnimator;

    public CombatState State { get; private set; }
    public List<CombatEnemy> Enemies { get; private set; } = new List<CombatEnemy>();

    public void BeginCombat(EnemyData[] enemyDatas)
    {
        Enemies.Clear();
        foreach (var data in enemyDatas)
            Enemies.Add(CombatEnemy.FromEnemyData(data));
        StartCombatSequence();
    }

    public void BeginBossCombat(MaskData boss)
    {
        Enemies.Clear();
        Enemies.Add(CombatEnemy.FromMaskData(boss));
        StartCombatSequence();
    }

    private void StartCombatSequence()
    {
        State = CombatState.Start;
        enemyDisplay.ShowEnemies(Enemies);
        maskAttackAnimator.SpawnMask();
        combatUI.Show();
        StartCoroutine(CombatLoop());
    }

    private IEnumerator CombatLoop()
    {
        yield return new WaitForSeconds(0.5f);

        while (State != CombatState.Victory &&
               State != CombatState.Defeat &&
               State != CombatState.Fled)
        {
            // Player turn
            State = CombatState.PlayerTurn;
            combatUI.ShowActionMenu();
            yield return new WaitUntil(() => State != CombatState.PlayerTurn);

            if (State == CombatState.Fled) break;

            if (AllEnemiesDead())
            {
                State = CombatState.Victory;
                break;
            }

            // Enemy turn
            State = CombatState.EnemyTurn;
            yield return ExecuteEnemyTurns();

            if (playerStats.IsDead)
            {
                State = CombatState.Defeat;
                break;
            }
        }

        yield return new WaitForSeconds(0.5f);
        EndCombat();
    }

    public void OnPlayerMove(MoveData move, int targetIndex)
    {
        StartCoroutine(ExecutePlayerAttack(move, targetIndex));
    }

    public void OnPlayerFlee()
    {
        if (Enemies.Exists(e => e.isBoss))
        {
            combatUI.ShowMessage("Can't flee from this.");
            return;
        }
        State = CombatState.Fled;
    }

    private IEnumerator ExecutePlayerAttack(MoveData move, int targetIndex)
    {
        State = CombatState.PlayerAttack;

        if (move.targetType == TargetType.All)
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (!Enemies[i].IsDead)
                {
                    yield return maskAttackAnimator.PlayAttack(i);
                    ApplyDamageToEnemy(move, i);
                }
            }
        }
        else
        {
            yield return maskAttackAnimator.PlayAttack(targetIndex);
            ApplyDamageToEnemy(move, targetIndex);
        }

        enemyDisplay.RefreshAll(Enemies);
        combatUI.RefreshHP();
    }

    private void ApplyDamageToEnemy(MoveData move, int targetIndex)
    {
        var enemy = Enemies[targetIndex];
        float multiplier = ElementChart.GetMultiplier(move.element, enemy.element);
        int damage = Mathf.Max(1, Mathf.RoundToInt((playerStats.Attack + move.power - enemy.defense) * multiplier));
        enemy.currentHP = Mathf.Max(0, enemy.currentHP - damage);

        combatUI.ShowDamage(damage, multiplier > 1f, targetIndex);

        if (enemy.IsDead)
            enemyDisplay.PlayDeath(targetIndex);
    }

    private IEnumerator ExecuteEnemyTurns()
    {
        foreach (var enemy in Enemies)
        {
            if (enemy.IsDead) continue;

            var move = enemy.moves[Random.Range(0, enemy.moves.Length)];
            Element playerElement = playerStats.EquippedMask != null
                ? playerStats.EquippedMask.element
                : Element.None;
            float multiplier = ElementChart.GetMultiplier(move.element, playerElement);
            int damage = Mathf.Max(1, Mathf.RoundToInt((enemy.attack + move.power - playerStats.Defense) * multiplier));

            playerStats.TakeDamage(damage);
            combatUI.ShowEnemyAttack(enemy.name, move.moveName, damage);
            combatUI.RefreshHP();

            yield return new WaitForSeconds(0.6f);

            if (playerStats.IsDead) break;
        }
    }

    private bool AllEnemiesDead()
    {
        return Enemies.TrueForAll(e => e.IsDead);
    }

    private void EndCombat()
    {
        combatUI.Hide();
        enemyDisplay.Clear();
        maskAttackAnimator.ClearMask();

        MaskData droppedMask = null;
        foreach (var enemy in Enemies)
        {
            if (enemy.isBoss && enemy.IsDead && enemy.maskDrop != null)
            {
                droppedMask = enemy.maskDrop;
                break;
            }
        }

        if (State == CombatState.Victory)
        {
            if (droppedMask != null)
                GameManager.Instance.BossDefeated(droppedMask);
            else
                GameManager.Instance.EndCombat(true);
        }
        else if (State == CombatState.Defeat)
        {
            GameManager.Instance.PlayerDefeated();
        }
        else
        {
            GameManager.Instance.EndCombat(false);
        }
    }
}
