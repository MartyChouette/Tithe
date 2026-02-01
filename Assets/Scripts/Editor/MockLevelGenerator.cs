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

        // ===== Shared Moves =====
        var slash = CreateMove("Slash", 15, Element.None, TargetType.Single, "A basic slash attack.");
        var heavySlash = CreateMove("Heavy Slash", 20, Element.None, TargetType.Single, "A powerful melee strike.");

        // ===== Floor 1: Ember Crypt (Fire) =====
        var fireball = CreateMove("Fireball", 20, Element.Fire, TargetType.All, "Hurls a ball of fire at all enemies.");
        var flameWhip = CreateMove("Flame Whip", 18, Element.Fire, TargetType.Single, "Lashes out with a whip of flame.");
        var emberShower = CreateMove("Ember Shower", 16, Element.Fire, TargetType.All, "A rain of burning embers.");

        var emberWraith = CreateEnemy("Ember Wraith", Element.Fire, 30, 10, 6, 10,
            new MoveData[] { flameWhip, slash });
        var cinder_Imp = CreateEnemy("Cinder Imp", Element.Fire, 35, 12, 5, 12,
            new MoveData[] { fireball, slash });
        var lavaCrawler = CreateEnemy("Lava Crawler", Element.Fire, 45, 14, 8, 8,
            new MoveData[] { emberShower, heavySlash });

        var infernoMask = CreateBossMask("Inferno Mask", Element.Fire, 100, 16, 10, 12,
            new MoveData[] { fireball, flameWhip, heavySlash },
            bonusAttack: 4, bonusDefense: 2, bonusSpeed: 1, bonusHP: 15);

        var floor1 = CreateFloor("Ember Crypt", 1, 0.25f,
            new EnemyData[] { emberWraith, cinder_Imp, lavaCrawler },
            infernoMask);

        // ===== Floor 2: Frozen Cavern (Ice) =====
        var frostLance = CreateMove("Frost Lance", 22, Element.Ice, TargetType.Single, "A piercing lance of ice.");
        var blizzard = CreateMove("Blizzard", 18, Element.Ice, TargetType.All, "A freezing gale hits all enemies.");
        var iceShards = CreateMove("Ice Shards", 20, Element.Ice, TargetType.Single, "Sharp shards of ice impale the target.");

        var frostSprite = CreateEnemy("Frost Sprite", Element.Ice, 40, 14, 8, 14,
            new MoveData[] { frostLance, slash });
        var glacialGolem = CreateEnemy("Glacial Golem", Element.Ice, 65, 16, 12, 6,
            new MoveData[] { iceShards, heavySlash });
        var snowWraith = CreateEnemy("Snow Wraith", Element.Ice, 50, 18, 7, 10,
            new MoveData[] { blizzard, frostLance });

        var glacialMask = CreateBossMask("Glacial Mask", Element.Ice, 150, 20, 14, 10,
            new MoveData[] { blizzard, frostLance, heavySlash },
            bonusAttack: 3, bonusDefense: 5, bonusSpeed: 0, bonusHP: 25);

        var floor2 = CreateFloor("Frozen Cavern", 2, 0.30f,
            new EnemyData[] { frostSprite, glacialGolem, snowWraith },
            glacialMask);

        // ===== Floor 3: Dark Sanctum (Dark) =====
        var shadowBolt = CreateMove("Shadow Bolt", 25, Element.Dark, TargetType.Single, "A bolt of dark energy.");
        var voidPulse = CreateMove("Void Pulse", 22, Element.Dark, TargetType.All, "A wave of darkness engulfs all.");
        var darkCleave = CreateMove("Dark Cleave", 28, Element.Dark, TargetType.Single, "A devastating dark-infused strike.");

        var shadowFiend = CreateEnemy("Shadow Fiend", Element.Dark, 55, 18, 10, 14,
            new MoveData[] { shadowBolt, slash });
        var voidStalker = CreateEnemy("Void Stalker", Element.Dark, 70, 20, 8, 16,
            new MoveData[] { voidPulse, darkCleave });
        var abyssalKnight = CreateEnemy("Abyssal Knight", Element.Dark, 90, 24, 14, 10,
            new MoveData[] { darkCleave, shadowBolt });

        var abyssalMask = CreateBossMask("Abyssal Mask", Element.Dark, 220, 26, 16, 14,
            new MoveData[] { voidPulse, darkCleave, shadowBolt },
            bonusAttack: 7, bonusDefense: 5, bonusSpeed: 3, bonusHP: 30);

        var floor3 = CreateFloor("Dark Sanctum", 3, 0.35f,
            new EnemyData[] { shadowFiend, voidStalker, abyssalKnight },
            abyssalMask);

        // ===== Starter Mask =====
        var flash = CreateMove("Flash", 18, Element.Light, TargetType.All, "A burst of blinding light.");
        CreateStarterMask("Initiate's Mask", Element.Light,
            new MoveData[] { slash, flash },
            bonusAttack: 2, bonusDefense: 2, bonusSpeed: 0, bonusHP: 10);

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

    private static MaskData CreateStarterMask(string name, Element element,
        MoveData[] moves, int bonusAttack, int bonusDefense, int bonusSpeed, int bonusHP)
    {
        var mask = ScriptableObject.CreateInstance<MaskData>();
        mask.maskName = name;
        mask.element = element;
        mask.maxHP = 0;
        mask.attack = 0;
        mask.defense = 0;
        mask.speed = 0;
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
