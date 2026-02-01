using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        // ===== Create ScriptableObject data in memory =====
        var data = CreateGameData();

        // ===== Lighting =====
        CreateLighting();

        // ===== Player =====
        var playerGO = CreatePlayer();
        var playerStats = playerGO.GetComponent<PlayerStats>();
        var maskInventory = playerGO.GetComponent<MaskInventory>();
        var movement = playerGO.GetComponent<AdvancedGridMovement>();
        var controller = playerGO.GetComponent<TithePlayerController>();
        var encounters = playerGO.GetComponent<RandomEncounters>();

        // Initialize player systems
        playerStats.Initialize();
        maskInventory.Initialize(playerStats);

        // Equip starter mask
        maskInventory.AddMask(data.starterMask);

        // Wire stepEvent via reflection
        WireStepEvent(movement, encounters);

        // Set AnimationCurve fields via reflection
        SetAnimationCurves(movement);

        // ===== Systems =====
        var systemsGO = new GameObject("Systems");
        var gameManager = systemsGO.AddComponent<GameManager>();
        var combatManager = systemsGO.AddComponent<CombatManager>();
        var dungeonGenerator = systemsGO.AddComponent<DungeonGenerator>();

        // ===== UI Canvas =====
        var canvas = RuntimeUIBuilder.CreateCanvas("GameCanvas");
        var canvasTransform = canvas.transform;

        // --- HUD ---
        var hud = systemsGO.AddComponent<HUD>();
        var hudPanel = RuntimeUIBuilder.CreatePanel(canvasTransform, "HUDPanel",
            new Color(0, 0, 0, 0.5f),
            new Vector2(0, 0.9f), Vector2.one);
        var hudLayout = hudPanel.AddComponent<HorizontalLayoutGroup>();
        hudLayout.padding = new RectOffset(15, 15, 5, 5);
        hudLayout.spacing = 30;
        hudLayout.childAlignment = TextAnchor.MiddleLeft;

        var hpText = RuntimeUIBuilder.CreateText(hudPanel.transform, "HPText", "HP:", 18);
        var maskText = RuntimeUIBuilder.CreateText(hudPanel.transform, "MaskText", "Mask:", 18);
        var floorText = RuntimeUIBuilder.CreateText(hudPanel.transform, "FloorText", "Floor:", 18);

        hud.Initialize(playerStats, hudPanel, hpText, maskText, floorText);

        // --- Combat UI ---
        var combatUI = systemsGO.AddComponent<CombatUI>();
        var enemyDisplay = systemsGO.AddComponent<EnemyDisplay>();
        var maskAnimator = systemsGO.AddComponent<MaskAttackAnimator>();

        BuildCombatUI(canvasTransform, combatUI, combatManager, playerStats, enemyDisplay);

        // --- Mask Attack Animator ---
        var maskMount = new GameObject("MaskMount").transform;
        maskMount.SetParent(playerGO.transform);
        maskMount.localPosition = new Vector3(0, 0.5f, 0.5f);

        var enemyTargets = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            var t = new GameObject($"EnemyTarget{i}").transform;
            t.SetParent(systemsGO.transform);
            t.position = new Vector3(-2f + i * 2f, 1f, 5f);
            enemyTargets[i] = t;
        }
        maskAnimator.Initialize(playerStats, maskMount, enemyTargets);

        // --- Enemy Display Slots ---
        BuildEnemyDisplay(canvasTransform, enemyDisplay);

        // --- Victory Screen ---
        var victoryScreen = systemsGO.AddComponent<VictoryScreen>();
        victoryScreen.Initialize(canvasTransform);

        // ===== Wire all systems =====
        combatManager.Initialize(playerStats, combatUI, enemyDisplay, maskAnimator);

        dungeonGenerator.SetBossMasks(data.bossMasks);

        gameManager.Initialize(
            data.floors, playerStats, maskInventory,
            combatManager, controller, hud,
            dungeonGenerator, victoryScreen, movement
        );

        // ===== Generate first floor =====
        Vector3 startPos = dungeonGenerator.GenerateFloor(0);
        playerGO.transform.position = startPos;
        GridMovementHelper.TeleportTo(movement, startPos);

        // ===== Start =====
        hud.Refresh();
    }

    private struct GameData
    {
        public FloorData[] floors;
        public MaskData[] bossMasks;
        public MaskData starterMask;
    }

    private GameData CreateGameData()
    {
        // ===== Shared Moves =====
        var slash = CreateMove("Slash", 15, Element.None, TargetType.Single);
        var heavySlash = CreateMove("Heavy Slash", 20, Element.None, TargetType.Single);

        // ===== Floor 1: Ember Crypt (Fire) =====
        var fireball = CreateMove("Fireball", 20, Element.Fire, TargetType.All);
        var flameWhip = CreateMove("Flame Whip", 18, Element.Fire, TargetType.Single);
        var emberShower = CreateMove("Ember Shower", 16, Element.Fire, TargetType.All);

        var emberWraith = CreateEnemy("Ember Wraith", Element.Fire, 30, 10, 6, 10,
            new[] { flameWhip, slash });
        var cinderImp = CreateEnemy("Cinder Imp", Element.Fire, 35, 12, 5, 12,
            new[] { fireball, slash });
        var lavaCrawler = CreateEnemy("Lava Crawler", Element.Fire, 45, 14, 8, 8,
            new[] { emberShower, heavySlash });

        var infernoMask = CreateBossMask("Inferno Mask", Element.Fire, 100, 16, 10, 12,
            new[] { fireball, flameWhip, heavySlash }, 4, 2, 1, 15);

        var floor1 = CreateFloor("Ember Crypt", 1, 0.25f,
            new[] { emberWraith, cinderImp, lavaCrawler }, infernoMask);

        // ===== Floor 2: Frozen Cavern (Ice) =====
        var frostLance = CreateMove("Frost Lance", 22, Element.Ice, TargetType.Single);
        var blizzard = CreateMove("Blizzard", 18, Element.Ice, TargetType.All);
        var iceShards = CreateMove("Ice Shards", 20, Element.Ice, TargetType.Single);

        var frostSprite = CreateEnemy("Frost Sprite", Element.Ice, 40, 14, 8, 14,
            new[] { frostLance, slash });
        var glacialGolem = CreateEnemy("Glacial Golem", Element.Ice, 65, 16, 12, 6,
            new[] { iceShards, heavySlash });
        var snowWraith = CreateEnemy("Snow Wraith", Element.Ice, 50, 18, 7, 10,
            new[] { blizzard, frostLance });

        var glacialMask = CreateBossMask("Glacial Mask", Element.Ice, 150, 20, 14, 10,
            new[] { blizzard, frostLance, heavySlash }, 3, 5, 0, 25);

        var floor2 = CreateFloor("Frozen Cavern", 2, 0.30f,
            new[] { frostSprite, glacialGolem, snowWraith }, glacialMask);

        // ===== Floor 3: Dark Sanctum (Dark) =====
        var shadowBolt = CreateMove("Shadow Bolt", 25, Element.Dark, TargetType.Single);
        var voidPulse = CreateMove("Void Pulse", 22, Element.Dark, TargetType.All);
        var darkCleave = CreateMove("Dark Cleave", 28, Element.Dark, TargetType.Single);

        var shadowFiend = CreateEnemy("Shadow Fiend", Element.Dark, 55, 18, 10, 14,
            new[] { shadowBolt, slash });
        var voidStalker = CreateEnemy("Void Stalker", Element.Dark, 70, 20, 8, 16,
            new[] { voidPulse, darkCleave });
        var abyssalKnight = CreateEnemy("Abyssal Knight", Element.Dark, 90, 24, 14, 10,
            new[] { darkCleave, shadowBolt });

        var abyssalMask = CreateBossMask("Abyssal Mask", Element.Dark, 220, 26, 16, 14,
            new[] { voidPulse, darkCleave, shadowBolt }, 7, 5, 3, 30);

        var floor3 = CreateFloor("Dark Sanctum", 3, 0.35f,
            new[] { shadowFiend, voidStalker, abyssalKnight }, abyssalMask);

        // ===== Starter Mask =====
        var flash = CreateMove("Flash", 18, Element.Light, TargetType.All);
        var starterMask = CreateStarterMask("Initiate's Mask", Element.Light,
            new[] { slash, flash }, 2, 2, 0, 10);

        return new GameData
        {
            floors = new[] { floor1, floor2, floor3 },
            bossMasks = new[] { infernoMask, glacialMask, abyssalMask },
            starterMask = starterMask
        };
    }

    // ===== Player creation =====
    private GameObject CreatePlayer()
    {
        var go = new GameObject("Player");
        go.tag = "Player";

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        var col = go.AddComponent<BoxCollider>();
        col.size = new Vector3(1f, 2f, 1f);
        col.center = new Vector3(0, 1f, 0);

        go.AddComponent<AdvancedGridMovement>();
        go.AddComponent<TithePlayerController>();
        go.AddComponent<PlayerStats>();
        go.AddComponent<MaskInventory>();
        go.AddComponent<RandomEncounters>();

        // Camera as child
        var camGO = new GameObject("PlayerCamera");
        camGO.transform.SetParent(go.transform);
        camGO.transform.localPosition = new Vector3(0, 1.6f, 0);
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();

        // Remove default camera if exists
        var defaultCam = Camera.main;
        if (defaultCam != null && defaultCam.gameObject != camGO)
            Destroy(defaultCam.gameObject);

        return go;
    }

    // ===== Lighting =====
    private void CreateLighting()
    {
        var lightGO = new GameObject("DirectionalLight");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.intensity = 0.8f;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    // ===== Combat UI construction =====
    private void BuildCombatUI(Transform canvasTransform, CombatUI combatUI,
        CombatManager combatManager, PlayerStats playerStats, EnemyDisplay enemyDisplay)
    {
        combatUI.Initialize(combatManager, playerStats);

        // Combat root panel (full screen)
        var combatPanel = RuntimeUIBuilder.CreatePanel(canvasTransform, "CombatPanel",
            new Color(0, 0, 0, 0.7f),
            Vector2.zero, Vector2.one);

        // Player HP area (top-left)
        var hpPanel = RuntimeUIBuilder.CreatePanel(combatPanel.transform, "HPPanel",
            new Color(0, 0, 0, 0.6f),
            new Vector2(0, 0.85f), new Vector2(0.35f, 1f),
            new Vector2(5, 5), new Vector2(-5, -5));
        var playerHPText = RuntimeUIBuilder.CreateText(hpPanel.transform, "PlayerHP", "HP:", 18);
        var playerMaskText = RuntimeUIBuilder.CreateText(hpPanel.transform, "PlayerMask", "Mask:", 14);
        RuntimeUIBuilder.SetAnchors(playerHPText.rectTransform,
            new Vector2(0.05f, 0.5f), new Vector2(0.95f, 1f));
        RuntimeUIBuilder.SetAnchors(playerMaskText.rectTransform,
            new Vector2(0.05f, 0f), new Vector2(0.95f, 0.5f));

        // Message panel (bottom center)
        var messagePanel = RuntimeUIBuilder.CreatePanel(combatPanel.transform, "MessagePanel",
            new Color(0.1f, 0.1f, 0.15f, 0.9f),
            new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.12f));
        var messageText = RuntimeUIBuilder.CreateText(messagePanel.transform, "MessageText", "",
            18, TextAnchor.MiddleCenter);

        // Action menu panel (bottom-left)
        var actionPanel = RuntimeUIBuilder.CreatePanel(combatPanel.transform, "ActionMenu",
            new Color(0.15f, 0.15f, 0.2f, 0.9f),
            new Vector2(0.02f, 0.15f), new Vector2(0.25f, 0.45f));
        var actionLayout = actionPanel.AddComponent<VerticalLayoutGroup>();
        actionLayout.padding = new RectOffset(10, 10, 10, 10);
        actionLayout.spacing = 8;
        actionLayout.childAlignment = TextAnchor.MiddleCenter;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = false;

        var fightBtn = RuntimeUIBuilder.CreateButton(actionPanel.transform, "FightBtn", "Fight",
            new Color(0.2f, 0.5f, 0.2f), 20);
        var fleeBtn = RuntimeUIBuilder.CreateButton(actionPanel.transform, "FleeBtn", "Flee",
            new Color(0.5f, 0.3f, 0.1f), 20);

        fightBtn.onClick.AddListener(() => combatUI.OnFightPressed());
        fleeBtn.onClick.AddListener(() => combatUI.OnFleePressed());

        // Move list panel (bottom-center)
        var moveListPanel = RuntimeUIBuilder.CreatePanel(combatPanel.transform, "MoveListPanel",
            new Color(0.15f, 0.15f, 0.2f, 0.9f),
            new Vector2(0.02f, 0.15f), new Vector2(0.4f, 0.55f));
        var moveLayout = moveListPanel.AddComponent<VerticalLayoutGroup>();
        moveLayout.padding = new RectOffset(10, 10, 10, 10);
        moveLayout.spacing = 5;
        moveLayout.childAlignment = TextAnchor.MiddleCenter;
        moveLayout.childForceExpandWidth = true;
        moveLayout.childForceExpandHeight = false;

        var moveButtons = new Button[4];
        var moveButtonTexts = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            var btn = RuntimeUIBuilder.CreateButton(moveListPanel.transform, $"MoveBtn{i}", $"Move {i}",
                new Color(0.25f, 0.25f, 0.35f), 16);
            moveButtons[i] = btn;
            moveButtonTexts[i] = btn.GetComponentInChildren<Text>();
        }
        var moveBackBtn = RuntimeUIBuilder.CreateButton(moveListPanel.transform, "MoveBackBtn", "Back",
            new Color(0.4f, 0.2f, 0.2f), 14);

        // Target panel (bottom-right)
        var targetPanel = RuntimeUIBuilder.CreatePanel(combatPanel.transform, "TargetPanel",
            new Color(0.15f, 0.15f, 0.2f, 0.9f),
            new Vector2(0.02f, 0.15f), new Vector2(0.4f, 0.55f));
        var targetLayout = targetPanel.AddComponent<VerticalLayoutGroup>();
        targetLayout.padding = new RectOffset(10, 10, 10, 10);
        targetLayout.spacing = 5;
        targetLayout.childAlignment = TextAnchor.MiddleCenter;
        targetLayout.childForceExpandWidth = true;
        targetLayout.childForceExpandHeight = false;

        var targetButtons = new Button[3];
        var targetButtonTexts = new Text[3];
        for (int i = 0; i < 3; i++)
        {
            var btn = RuntimeUIBuilder.CreateButton(targetPanel.transform, $"TargetBtn{i}", $"Enemy {i}",
                new Color(0.35f, 0.2f, 0.2f), 16);
            targetButtons[i] = btn;
            targetButtonTexts[i] = btn.GetComponentInChildren<Text>();
        }
        var targetBackBtn = RuntimeUIBuilder.CreateButton(targetPanel.transform, "TargetBackBtn", "Back",
            new Color(0.4f, 0.2f, 0.2f), 14);

        // Wire panels and buttons to CombatUI
        combatUI.SetPanels(combatPanel, actionPanel, moveListPanel, targetPanel, messagePanel,
            playerHPText, playerMaskText, messageText);
        combatUI.SetButtons(moveButtons, moveButtonTexts, moveBackBtn,
            targetButtons, targetButtonTexts, targetBackBtn);

        combatPanel.SetActive(false);
    }

    // ===== Enemy display construction =====
    private void BuildEnemyDisplay(Transform canvasTransform, EnemyDisplay enemyDisplay)
    {
        var displayPanel = RuntimeUIBuilder.CreatePanel(canvasTransform, "EnemyDisplayPanel",
            new Color(0, 0, 0, 0),
            new Vector2(0.15f, 0.4f), new Vector2(0.85f, 0.85f));
        var layout = displayPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        var slots = new Image[3];
        var hpTexts = new Text[3];

        for (int i = 0; i < 3; i++)
        {
            // Each slot is a container with an Image used for the enemy sprite.
            // EnemyDisplay toggles enemySlots[i].gameObject, so the slot's own
            // gameObject must be what gets shown/hidden.
            var slotGO = new GameObject($"EnemySlot{i}");
            slotGO.transform.SetParent(displayPanel.transform, false);

            var rt = slotGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = slotGO.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;

            var hp = RuntimeUIBuilder.CreateText(slotGO.transform, "EnemyHP", "",
                14, TextAnchor.MiddleCenter);
            RuntimeUIBuilder.SetAnchors(hp.rectTransform,
                new Vector2(0.05f, 0f), new Vector2(0.95f, 0.2f));

            slots[i] = img;
            hpTexts[i] = hp;
            slotGO.SetActive(false);
        }

        enemyDisplay.Initialize(slots, hpTexts);
    }

    // ===== Reflection helpers =====
    private void WireStepEvent(AdvancedGridMovement movement, RandomEncounters encounters)
    {
        var type = typeof(AdvancedGridMovement);
        var field = type.GetField("stepEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var evt = field.GetValue(movement) as UnityEvent;
            if (evt == null)
            {
                evt = new UnityEvent();
                field.SetValue(movement, evt);
            }
            evt.AddListener(encounters.OnStep);
        }
        else
        {
            Debug.LogWarning("[GameBootstrap] Could not find stepEvent field on AdvancedGridMovement");
        }
    }

    private void SetAnimationCurves(AdvancedGridMovement movement)
    {
        var type = typeof(AdvancedGridMovement);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        // Simple linear curve for walk
        var walkCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        // Slight ease for head bob
        var bobCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 0.05f),
            new Keyframe(1f, 0f),
            new Keyframe(1.5f, 0.05f),
            new Keyframe(2f, 0f),
            new Keyframe(2.5f, 0.05f),
            new Keyframe(3f, 0f)
        );

        SetField(type, movement, "walkSpeedCurve", walkCurve, flags);
        SetField(type, movement, "walkHeadBobCurve", bobCurve, flags);
        SetField(type, movement, "runningSpeedCurve", walkCurve, flags);
        SetField(type, movement, "runningHeadBobCurve", bobCurve, flags);
    }

    private void SetField(System.Type type, object target, string fieldName, object value, BindingFlags flags)
    {
        var field = type.GetField(fieldName, flags);
        if (field != null)
            field.SetValue(target, value);
    }

    // ===== ScriptableObject factory methods =====
    private MoveData CreateMove(string name, int power, Element element, TargetType target)
    {
        var move = ScriptableObject.CreateInstance<MoveData>();
        move.moveName = name;
        move.power = power;
        move.element = element;
        move.targetType = target;
        move.name = name;
        return move;
    }

    private EnemyData CreateEnemy(string name, Element element, int hp, int atk, int def, int spd, MoveData[] moves)
    {
        var enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = name;
        enemy.element = element;
        enemy.maxHP = hp;
        enemy.attack = atk;
        enemy.defense = def;
        enemy.speed = spd;
        enemy.moves = moves;
        enemy.name = name;
        return enemy;
    }

    private MaskData CreateBossMask(string name, Element element, int hp, int atk, int def, int spd,
        MoveData[] moves, int bonusAtk, int bonusDef, int bonusSpd, int bonusHP)
    {
        var mask = ScriptableObject.CreateInstance<MaskData>();
        mask.maskName = name;
        mask.element = element;
        mask.maxHP = hp;
        mask.attack = atk;
        mask.defense = def;
        mask.speed = spd;
        mask.moves = moves;
        mask.bonusAttack = bonusAtk;
        mask.bonusDefense = bonusDef;
        mask.bonusSpeed = bonusSpd;
        mask.bonusHP = bonusHP;
        mask.name = name;
        return mask;
    }

    private MaskData CreateStarterMask(string name, Element element,
        MoveData[] moves, int bonusAtk, int bonusDef, int bonusSpd, int bonusHP)
    {
        var mask = ScriptableObject.CreateInstance<MaskData>();
        mask.maskName = name;
        mask.element = element;
        mask.maxHP = 0;
        mask.attack = 0;
        mask.defense = 0;
        mask.speed = 0;
        mask.moves = moves;
        mask.bonusAttack = bonusAtk;
        mask.bonusDefense = bonusDef;
        mask.bonusSpeed = bonusSpd;
        mask.bonusHP = bonusHP;
        mask.name = name;
        return mask;
    }

    private FloorData CreateFloor(string name, int floorNumber, float encounterRate,
        EnemyData[] enemies, MaskData boss)
    {
        var floor = ScriptableObject.CreateInstance<FloorData>();
        floor.floorName = name;
        floor.floorNumber = floorNumber;
        floor.encounterRate = encounterRate;
        floor.enemyTable = enemies;
        floor.maskBoss = boss;
        floor.name = name;
        return floor;
    }
}
