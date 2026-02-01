using UnityEngine;
using UnityEditor;
using System.IO;

public class MockLevelGenerator
{
    private const string OutputPath = "Assets/Data/MockLevel";

    [MenuItem("Tithe/Generate Mock Level")]
    public static void Generate()
    {
        EnsureDirectory(OutputPath);

        // --- Moves ---
        var slash = CreateMove("Slash", 15, Element.None, TargetType.Single, "A basic slash attack.");
        var fireball = CreateMove("Fireball", 20, Element.Fire, TargetType.All, "Hurls a ball of fire at all enemies.");
        var frostLance = CreateMove("Frost Lance", 22, Element.Ice, TargetType.Single, "A piercing lance of ice.");
        var spark = CreateMove("Spark", 18, Element.Shock, TargetType.Single, "A quick jolt of electricity.");
        var shadowBolt = CreateMove("Shadow Bolt", 25, Element.Dark, TargetType.Single, "A bolt of dark energy.");
        var holyLight = CreateMove("Holy Light", 20, Element.Light, TargetType.All, "A wave of purifying light.");

        // --- Enemies ---
        var fireImp = CreateEnemy("Fire Imp", Element.Fire, 45, 12, 6, 10,
            new MoveData[] { fireball, slash });
        var frostSprite = CreateEnemy("Frost Sprite", Element.Ice, 40, 10, 8, 14,
            new MoveData[] { frostLance, slash });
        var shockBeetle = CreateEnemy("Shock Beetle", Element.Shock, 50, 14, 10, 8,
            new MoveData[] { spark, slash });

        // --- Boss Mask ---
        var shadowKing = CreateBossMask("Shadow King", Element.Dark, 120, 18, 12, 11,
            new MoveData[] { shadowBolt, holyLight, slash },
            bonusAttack: 5, bonusDefense: 3, bonusSpeed: 2, bonusHP: 20);

        // --- Floor ---
        CreateFloor("Sunken Crypt", 1, 0.3f,
            new EnemyData[] { fireImp, frostSprite, shockBeetle },
            shadowKing);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MockLevelGenerator] Mock level assets created in " + OutputPath);
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                string grandparent = Path.GetDirectoryName(parent).Replace("\\", "/");
                string parentFolder = Path.GetFileName(parent);
                AssetDatabase.CreateFolder(grandparent, parentFolder);
            }
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static MoveData CreateMove(string name, int power, Element element, TargetType target, string desc)
    {
        var move = ScriptableObject.CreateInstance<MoveData>();
        move.moveName = name;
        move.power = power;
        move.element = element;
        move.targetType = target;
        move.description = desc;
        AssetDatabase.CreateAsset(move, $"{OutputPath}/{name.Replace(" ", "")}.asset");
        return move;
    }

    private static EnemyData CreateEnemy(string name, Element element, int hp, int atk, int def, int spd, MoveData[] moves)
    {
        var enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = name;
        enemy.element = element;
        enemy.maxHP = hp;
        enemy.attack = atk;
        enemy.defense = def;
        enemy.speed = spd;
        enemy.moves = moves;
        AssetDatabase.CreateAsset(enemy, $"{OutputPath}/{name.Replace(" ", "")}.asset");
        return enemy;
    }

    private static MaskData CreateBossMask(string name, Element element, int hp, int atk, int def, int spd,
        MoveData[] moves, int bonusAttack, int bonusDefense, int bonusSpeed, int bonusHP)
    {
        var mask = ScriptableObject.CreateInstance<MaskData>();
        mask.maskName = name;
        mask.element = element;
        mask.maxHP = hp;
        mask.attack = atk;
        mask.defense = def;
        mask.speed = spd;
        mask.moves = moves;
        mask.bonusAttack = bonusAttack;
        mask.bonusDefense = bonusDefense;
        mask.bonusSpeed = bonusSpeed;
        mask.bonusHP = bonusHP;
        AssetDatabase.CreateAsset(mask, $"{OutputPath}/{name.Replace(" ", "")}.asset");
        return mask;
    }

    private static FloorData CreateFloor(string name, int floorNumber, float encounterRate,
        EnemyData[] enemies, MaskData boss)
    {
        var floor = ScriptableObject.CreateInstance<FloorData>();
        floor.floorName = name;
        floor.floorNumber = floorNumber;
        floor.encounterRate = encounterRate;
        floor.enemyTable = enemies;
        floor.maskBoss = boss;
        AssetDatabase.CreateAsset(floor, $"{OutputPath}/{name.Replace(" ", "")}.asset");
        return floor;
    }
}
