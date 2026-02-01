using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    // Cell types: 0=wall, 1=floor, 2=player start, 3=boss, 4=exit
    private const int WALL = 0;
    private const int FLOOR = 1;
    private const int START = 2;
    private const int BOSS = 3;
    private const int EXIT = 4;

    private const float GRID = 3.0f;
    private const float WALL_HEIGHT = 3.0f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    // Per-floor color themes
    private static readonly Color[][] floorThemes = new Color[][]
    {
        // Floor 1: Ember Crypt — red/orange
        new Color[] { new Color(0.5f, 0.15f, 0.1f), new Color(0.25f, 0.08f, 0.05f), new Color(0.6f, 0.3f, 0.1f) },
        // Floor 2: Frozen Cavern — blue/white
        new Color[] { new Color(0.2f, 0.35f, 0.55f), new Color(0.1f, 0.15f, 0.3f), new Color(0.7f, 0.8f, 0.9f) },
        // Floor 3: Dark Sanctum — purple/black
        new Color[] { new Color(0.3f, 0.1f, 0.35f), new Color(0.1f, 0.05f, 0.15f), new Color(0.5f, 0.2f, 0.6f) },
    };

    private static int[][,] layouts;

    private MaskData[] bossMasks;
    private bool isLastFloor;

    public void SetBossMasks(MaskData[] masks)
    {
        bossMasks = masks;
    }

    static DungeonGenerator()
    {
        BuildLayouts();
    }

    private static void BuildLayouts()
    {
        layouts = new int[3][,];

        // Floor 1: Ember Crypt (7x7) — compact L-shape corridor
        layouts[0] = new int[,]
        {
            { 0, 0, 0, 0, 0, 0, 0 },
            { 0, 2, 1, 1, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0, 1, 0 },
            { 0, 0, 0, 3, 1, 4, 0 },
            { 0, 0, 0, 0, 0, 0, 0 },
        };

        // Floor 2: Frozen Cavern (9x9) — winding corridor
        layouts[1] = new int[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 2, 1, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 3, 1, 4, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        };

        // Floor 3: Dark Sanctum (11x11) — longer dungeon, no exit
        layouts[2] = new int[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        };
    }

    public Vector3 GenerateFloor(int index)
    {
        int layoutIndex = Mathf.Clamp(index, 0, layouts.Length - 1);
        int themeIndex = Mathf.Clamp(index, 0, floorThemes.Length - 1);
        isLastFloor = (index >= layouts.Length - 1);

        var layout = layouts[layoutIndex];
        Color wallColor = floorThemes[themeIndex][0];
        Color floorColor = floorThemes[themeIndex][1];
        Color accentColor = floorThemes[themeIndex][2];

        int rows = layout.GetLength(0);
        int cols = layout.GetLength(1);

        Vector3 playerStart = Vector3.zero;
        FloorExit floorExit = null;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int cell = layout[r, c];
                if (cell == WALL) continue;

                Vector3 pos = new Vector3(c * GRID, 0f, -r * GRID);

                // Floor quad
                CreateFloorTile(pos, floorColor);

                // Walls around walkable tiles
                CreateWallsForCell(layout, r, c, rows, cols, wallColor);

                switch (cell)
                {
                    case START:
                        playerStart = pos + new Vector3(0f, 0.5f, 0f);
                        break;

                    case BOSS:
                        CreateBossTrigger(pos, index, accentColor);
                        break;

                    case EXIT:
                        floorExit = CreateExitTrigger(pos, accentColor);
                        break;
                }
            }
        }

        // Wire boss to floor exit
        var bosses = FindBossEncountersInSpawned();
        foreach (var boss in bosses)
        {
            boss.Setup(
                bossMasks != null && index < bossMasks.Length ? bossMasks[index] : null,
                floorExit
            );
        }

        // Create ceiling
        CreateCeiling(rows, cols, wallColor);

        return playerStart;
    }

    public void ClearFloor()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    private void CreateFloorTile(Vector3 pos, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "Floor";
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale = new Vector3(GRID, GRID, 1f);
        go.GetComponent<Renderer>().material.color = color;
        // Remove collider so player doesn't collide with floor
        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);
        spawnedObjects.Add(go);
    }

    private void CreateWallsForCell(int[,] layout, int r, int c, int rows, int cols, Color color)
    {
        Vector3 pos = new Vector3(c * GRID, WALL_HEIGHT / 2f, -r * GRID);

        // Check each direction: if neighbor is wall or out of bounds, place a wall
        // North (r-1)
        if (r - 1 < 0 || layout[r - 1, c] == WALL)
            CreateWall(pos + new Vector3(0, 0, GRID / 2f), new Vector3(GRID, WALL_HEIGHT, 0.2f), color);
        // South (r+1)
        if (r + 1 >= rows || layout[r + 1, c] == WALL)
            CreateWall(pos + new Vector3(0, 0, -GRID / 2f), new Vector3(GRID, WALL_HEIGHT, 0.2f), color);
        // West (c-1)
        if (c - 1 < 0 || layout[r, c - 1] == WALL)
            CreateWall(pos + new Vector3(-GRID / 2f, 0, 0), new Vector3(0.2f, WALL_HEIGHT, GRID), color);
        // East (c+1)
        if (c + 1 >= cols || layout[r, c + 1] == WALL)
            CreateWall(pos + new Vector3(GRID / 2f, 0, 0), new Vector3(0.2f, WALL_HEIGHT, GRID), color);
    }

    private void CreateWall(Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Wall";
        go.tag = "Level";
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material.color = color;
        spawnedObjects.Add(go);
    }

    private void CreateBossTrigger(Vector3 pos, int floorIndex, Color accentColor)
    {
        var go = new GameObject("BossEncounter");
        go.transform.position = pos;

        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(GRID * 0.8f, 2f, GRID * 0.8f);

        go.AddComponent<BossEncounter>();

        // Visual marker — small glowing pedestal
        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pedestal.name = "BossPedestal";
        pedestal.transform.SetParent(go.transform);
        pedestal.transform.localPosition = new Vector3(0, 0.25f, 0);
        pedestal.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        pedestal.GetComponent<Renderer>().material.color = accentColor;
        // Remove pedestal collider so it doesn't block movement
        var pedestalCol = pedestal.GetComponent<Collider>();
        if (pedestalCol != null) Destroy(pedestalCol);

        spawnedObjects.Add(go);
    }

    private FloorExit CreateExitTrigger(Vector3 pos, Color accentColor)
    {
        var go = new GameObject("FloorExit");
        go.transform.position = pos;

        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(GRID * 0.8f, 2f, GRID * 0.8f);

        var exit = go.AddComponent<FloorExit>();

        // Locked visual — red block
        var locked = GameObject.CreatePrimitive(PrimitiveType.Cube);
        locked.name = "LockedVisual";
        locked.transform.SetParent(go.transform);
        locked.transform.localPosition = new Vector3(0, 0.5f, 0);
        locked.transform.localScale = new Vector3(1f, 1f, 1f);
        locked.GetComponent<Renderer>().material.color = new Color(0.6f, 0.1f, 0.1f);
        var lockedCol = locked.GetComponent<Collider>();
        if (lockedCol != null) Destroy(lockedCol);

        // Unlocked visual — green block
        var unlocked = GameObject.CreatePrimitive(PrimitiveType.Cube);
        unlocked.name = "UnlockedVisual";
        unlocked.transform.SetParent(go.transform);
        unlocked.transform.localPosition = new Vector3(0, 0.5f, 0);
        unlocked.transform.localScale = new Vector3(1f, 1f, 1f);
        unlocked.GetComponent<Renderer>().material.color = accentColor;
        var unlockedCol = unlocked.GetComponent<Collider>();
        if (unlockedCol != null) Destroy(unlockedCol);

        exit.Setup(locked, unlocked);

        spawnedObjects.Add(go);
        return exit;
    }

    private void CreateCeiling(int rows, int cols, Color color)
    {
        float centerX = (cols - 1) * GRID / 2f;
        float centerZ = -(rows - 1) * GRID / 2f;

        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "Ceiling";
        go.transform.position = new Vector3(centerX, WALL_HEIGHT, centerZ);
        go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        go.transform.localScale = new Vector3(cols * GRID, rows * GRID, 1f);

        Color darkCeiling = color * 0.5f;
        darkCeiling.a = 1f;
        go.GetComponent<Renderer>().material.color = darkCeiling;
        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);
        spawnedObjects.Add(go);
    }

    private List<BossEncounter> FindBossEncountersInSpawned()
    {
        var result = new List<BossEncounter>();
        foreach (var obj in spawnedObjects)
        {
            if (obj == null) continue;
            var boss = obj.GetComponent<BossEncounter>();
            if (boss != null) result.Add(boss);
        }
        return result;
    }
}
