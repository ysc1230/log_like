#if UNITY_EDITOR
using System.IO;
using Survivor2D.Combat;
using Survivor2D.Core;
using Survivor2D.Enemy;
using Survivor2D.Input;
using Survivor2D.Player;
using Survivor2D.Progression;
using Survivor2D.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class MainSceneAutoBuilder
{
    private const string ScenePath = "Assets/Scenes/MainScene.unity";
    private const string PrefabFolder = "Assets/Prefabs";

    [MenuItem("Tools/Survivor2D/Build MainScene")]
    public static void BuildMainScene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // 1. Camera Setup
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            var camGo = new GameObject("Main Camera");
            mainCamera = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f; 
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.12f); 
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);

        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.white;

        EnsureDirectionalLight();
        Directory.CreateDirectory(PrefabFolder);

        // 2. Core Systems
        var gameRoot = GetOrCreate("GameRoot");
        var bootstrap = GetOrAdd<GameBootstrap>(gameRoot);
        var spawner = GetOrAdd<EnemySpawner>(gameRoot);
        var registry = GetOrAdd<EnemyRegistry>(gameRoot);
        var levelSystem = GetOrAdd<LevelSystem>(gameRoot);
        var projectilePool = GetOrAdd<ProjectilePool>(gameRoot);

        // 3. Clear existing objects to rebuild
        string[] toDelete = { 
            "Background", "PlayerInfoPanel", "HUD", "JoystickArea", "LevelUpPanel", 
            "GameOverPanel", "HPBar", "EXPBar", "TimeText", "MainBackground", "Environment", 
            "GreySquare", "Square", "Plane", "Cube", "World", "Obstacles"
        };
        var allGos = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var g in allGos)
        {
            if (g != null && System.Array.Exists(toDelete, name => g.name == name))
            {
                Object.DestroyImmediate(g);
            }
        }

        // 4. Environment
        BuildEnvironment();

        // 5. Build Entities & UI
        var player = BuildPlayer();
        var weaponManager = GetOrAdd<Survivor2D.Combat.WeaponManager>(player);
        var input = BuildUI(out var joystickArea, out var canvas, out var hud, out var levelUpPanel, out var gameOverPanel);

        var enemyPrefab = BuildEnemyPrefab();
        var projectilePrefab = BuildProjectilePrefab();
        var expOrbPrefab = BuildExpOrbPrefab();

        // 6. Wiring
        SetRef(enemyPrefab.GetComponent<Survivor2D.Enemy.EnemyController>(), "expOrbPrefab", expOrbPrefab.GetComponent<Survivor2D.Progression.ExpOrb>());
        SetRef(levelSystem, "playerStats", player.GetComponent<Survivor2D.Player.PlayerStats>());
        SetRef(levelSystem, "weaponManager", weaponManager);
        SetRef(spawner, "enemyPrefab", enemyPrefab.GetComponent<Survivor2D.Enemy.EnemyController>());
        SetRef(spawner, "player", player.transform);
        SetRef(spawner, "mainCamera", mainCamera);
        SetRef(spawner, "enemyRegistry", registry);
        SetRef(spawner, "levelSystem", levelSystem);
        
        SetRef(projectilePool, "projectilePrefab", projectilePrefab.GetComponent<Projectile>());

        var stats = player.GetComponent<Survivor2D.Player.PlayerStats>();
        
        SetRef(player.GetComponent<Survivor2D.Player.PlayerMover>(), "playerStats", stats);
        SetRef(player.GetComponent<Survivor2D.Player.PlayerMover>(), "input" , input);
        SetRef(player.GetComponent<Survivor2D.Player.PlayerMover>(), "mainCamera", mainCamera);

        SetRef(weaponManager, "projectilePool", projectilePool);
        SetRef(weaponManager, "enemyRegistry", registry);
        SetRef(weaponManager, "firePoint", player.transform.Find("FirePoint"));
        
        var catClawVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Weapon_CatClaw_Visual.prefab");
        var tunaBomb = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Weapon_TunaCanBomb_Visual.prefab");
        var explosionVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Weapon_Explosion_Visual.prefab");
        SetRef(weaponManager, "catClawVfxPrefab", catClawVfx);
        SetRef(weaponManager, "tunaCanBombPrefab", tunaBomb);
        SetRef(weaponManager, "explosionVfxPrefab", explosionVfx); 

        SetRef(player.GetComponent<Survivor2D.Progression.LevelSystemLink>(), "levelSystem", levelSystem);

        SetRef(bootstrap, "playerStats", stats);
        SetRef(bootstrap, "levelSystem", levelSystem);
        SetRef(bootstrap, "levelUpPanel", levelUpPanel);
        SetRef(bootstrap, "weaponManager", weaponManager);

        SetRef(levelUpPanel, "root", levelUpPanel.gameObject);
        SetRef(levelUpPanel, "levelSystem", levelSystem);
        SetRef(levelUpPanel, "weaponManager", weaponManager);
        SetRef(gameOverPanel, "root", gameOverPanel.gameObject);

        // UI Wiring
        SetRef(hud, "hpSlider", canvas.transform.Find("HPBar").GetComponent<Slider>());
        SetRef(hud, "expSlider", canvas.transform.Find("EXPBar").GetComponent<Slider>());
        SetRef(hud, "levelText", hud.transform.Find("LevelText").GetComponent<TMP_Text>());
        SetRef(hud, "timeText", canvas.transform.Find("TimeText").GetComponent<TMP_Text>());
        SetRef(hud, "statsText", hud.transform.Find("StatsText").GetComponent<TMP_Text>());
        SetRef(hud, "weaponListText", hud.transform.Find("WeaponListText").GetComponent<TMP_Text>());

        SetRef(input, "joystickArea", joystickArea);

        BindLevelUpButtons(levelUpPanel, levelUpPanel.transform);
        BindRestartButton(gameOverPanel, gameOverPanel.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Survivor2D: Rebuilt with Weapon Level-up System.");
        }

        private static void BindLevelUpButtons(LevelUpPanel panel, Transform root)
        {
        var names = new[] { "Choice0Button", "Choice1Button", "Choice2Button" };
        var methods = new UnityAction[] { panel.SelectChoice0, panel.SelectChoice1, panel.SelectChoice2 };
        var buttons = new Button[names.Length];
        for (int i = 0; i < names.Length; i++) {
            var btnTr = root.Find(names[i]);
            if (btnTr != null) {
                var btn = btnTr.GetComponent<Button>();
                buttons[i] = btn;
                btn.onClick.RemoveAllListeners();
                UnityEventTools.AddPersistentListener(btn.onClick, methods[i]);
            }
        }
        SetRefArray(panel, "choiceButtons", buttons);
        }

        private static void SetRefArray(Object target, string fieldName, Object[] values)
        {
        if (target == null) return;
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null || !prop.isArray) return;
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildEnvironment()
    {
        var world = new GameObject("World");
        
        // Tiled Background
        var bg = new GameObject("MainBackground");
        bg.transform.SetParent(world.transform);
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/Street_Tile.png");
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(100f, 100f);
        sr.sortingOrder = 0;
        sr.color = new Color(0.7f, 0.7f, 0.7f);
        bg.transform.position = Vector3.zero;

        var obs = new GameObject("Obstacles");
        obs.transform.SetParent(world.transform);

        // Props with Colliders
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Utility_Pole.png", new Vector2(-4, 4), 0.1f, true);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Utility_Pole.png", new Vector2(4, -4), 0.1f, true);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Trash_Can.png", new Vector2(-2, -5), 0.07f, false);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Trash_Can.png", new Vector2(5, 2), 0.07f, false);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Street_Sign.png", new Vector2(1, 5), 0.08f, false);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Street_Sign.png", new Vector2(-5, -2), 0.08f, false);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Vending_Machine.png", new Vector2(3, 6), 0.12f, true);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Planter.png", new Vector2(-4, -4), 0.08f, true);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Stacked_Boxes.png", new Vector2(5, -5), 0.1f, true);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Street_Fence.png", new Vector2(0, 8), 0.12f, true);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Street_Fence.png", new Vector2(4, 8), 0.12f, true);
        PlaceProp(obs.transform, "Assets/Sprites/Environment/Street_Fence.png", new Vector2(-4, 8), 0.12f, true);
    }

    private static void PlaceProp(Transform parent, string path, Vector2 pos, float scale, bool useBox)
    {
        var go = new GameObject(Path.GetFileNameWithoutExtension(path));
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        sr.sortingOrder = 5;
        if (useBox) go.AddComponent<BoxCollider2D>();
        else go.AddComponent<CircleCollider2D>();
    }

    private static void EnsureDirectionalLight()
    {
        var light = Object.FindObjectOfType<Light>();
        if (light != null) return;
        var go = new GameObject("Directional Light");
        light = go.AddComponent<Light>();
        light.type = LightType.Directional;
    }

    private static GameObject BuildPlayer()
    {
        var player = GetOrCreate("Player");
        var sr = GetOrAdd<SpriteRenderer>(player);
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/CatHero_Concept.png");
        sr.color = Color.white;
        sr.sortingOrder = 10;
        player.transform.localScale = Vector3.one * 0.07f; 
        
        var rb = GetOrAdd<Rigidbody2D>(player);
        rb.gravityScale = 0f; rb.freezeRotation = true;
        GetOrAdd<CircleCollider2D>(player);
        GetOrAdd<PlayerStats>(player);
        GetOrAdd<PlayerMover>(player);
        GetOrAdd<LevelSystemLink>(player);

        var firePoint = player.transform.Find("FirePoint");
        if (firePoint == null)
        {
            var fp = new GameObject("FirePoint");
            fp.transform.SetParent(player.transform);
            fp.transform.localPosition = Vector3.zero;
        }

        player.transform.position = Vector3.zero;
        return player;
    }

    private static TouchJoystickInput BuildUI(out RectTransform joystickArea, out Canvas canvas, out HudController hudController, out LevelUpPanel levelUpPanel, out GameOverPanel gameOverPanel)
    {
        var canvasGo = GetOrCreate("Canvas");
        canvas = GetOrAdd<Canvas>(canvasGo);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = GetOrAdd<CanvasScaler>(canvasGo);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        GetOrAdd<GraphicRaycaster>(canvasGo);

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // HUD
        var hudGo = GetOrCreate("HUD", canvasGo.transform);
        var hudRt = hudGo.GetComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(0, 1); hudRt.anchorMax = new Vector2(0, 1);
        hudRt.pivot = new Vector2(0, 1); hudRt.anchoredPosition = new Vector2(40, -40);
        hudRt.sizeDelta = new Vector2(400, 300);
        var hudImg = hudGo.GetComponent<Image>();
        if (hudImg != null) Object.DestroyImmediate(hudImg);

        CreateTmpText("LevelText", hudGo.transform, "Lv 1", new Vector2(0, 1), new Vector2(0, 1), Vector2.zero).fontSize = 28;
        CreateTmpText("StatsText", hudGo.transform, "ATK/ASPD/MSPD", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -40)).fontSize = 22;
        CreateTmpText("WeaponListText", hudGo.transform, "", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -120)).fontSize = 20;

        // HP/EXP Bars
        var hpBar = CreateSlider("HPBar", canvasGo.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -300));
        hpBar.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 16);
        hpBar.fillRect.GetComponent<Image>().color = Color.red;
        if (hpBar.GetComponent<Image>() != null) hpBar.GetComponent<Image>().enabled = false;

        var expBar = CreateSlider("EXPBar", canvasGo.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 20));
        expBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 20); 
        expBar.fillRect.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f); 
        if (expBar.GetComponent<Image>() != null) expBar.GetComponent<Image>().enabled = false;

        // Time
        var timeText = CreateTmpText("TimeText", canvasGo.transform, "00:00", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), TextAlignmentOptions.Center);
        timeText.fontSize = 32;

        // Joystick Area
        var joystick = GetOrCreate("JoystickArea", canvasGo.transform);
        joystickArea = joystick.GetComponent<RectTransform>();
        joystickArea.anchorMin = Vector2.zero; joystickArea.anchorMax = Vector2.one;
        joystickArea.offsetMin = Vector2.zero; joystickArea.offsetMax = Vector2.zero;
        var joyImg = GetOrAdd<Image>(joystick);
        joyImg.color = new Color(0, 0, 0, 0); 
        joyImg.raycastTarget = true;

        // Panels
        var levelPanelGo = GetOrCreate("LevelUpPanel", canvasGo.transform);
        GetOrAdd<Image>(levelPanelGo).color = new Color(0, 0, 0, 0.8f);
        var levelRt = levelPanelGo.GetComponent<RectTransform>();
        levelRt.anchorMin = Vector2.zero; levelRt.anchorMax = Vector2.one; levelRt.offsetMin = Vector2.zero; levelRt.offsetMax = Vector2.zero;
        CreateTmpText("Title", levelPanelGo.transform, "LEVEL UP", new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero, TextAlignmentOptions.Center).fontSize = 60;
        CreateButton("Choice0Button", levelPanelGo.transform, "Choice 0", new Vector2(0, 100));
        CreateButton("Choice1Button", levelPanelGo.transform, "Choice 1", new Vector2(0, -50));
        CreateButton("Choice2Button", levelPanelGo.transform, "Choice 2", new Vector2(0, -200));

        var gameOverGo = GetOrCreate("GameOverPanel", canvasGo.transform);
        GetOrAdd<Image>(gameOverGo).color = new Color(0, 0, 0, 0.9f);
        var gameOverRt = gameOverGo.GetComponent<RectTransform>();
        gameOverRt.anchorMin = Vector2.zero; gameOverRt.anchorMax = Vector2.one; gameOverRt.offsetMin = Vector2.zero; gameOverRt.offsetMax = Vector2.zero;
        CreateTmpText("GameOverText", gameOverGo.transform, "GAME OVER", new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, TextAlignmentOptions.Center).fontSize = 80;
        CreateButton("RestartButton", gameOverGo.transform, "RETRY", new Vector2(0, -50));

        hudController = GetOrAdd<HudController>(hudGo);
        levelUpPanel = GetOrAdd<LevelUpPanel>(levelPanelGo);
        gameOverPanel = GetOrAdd<GameOverPanel>(gameOverGo);

        return GetOrAdd<TouchJoystickInput>(canvasGo);
        }

    private static GameObject BuildEnemyPrefab()
    {
        var path = PrefabFolder + "/Enemy.prefab";
        var go = new GameObject("Enemy");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/MouseEnemy_Concept.png");
        sr.sortingOrder = 5;
        go.transform.localScale = Vector3.one * 0.07f; 
        var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0f; rb.freezeRotation = true;
        go.AddComponent<CircleCollider2D>();
        go.AddComponent<EnemyController>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject BuildProjectilePrefab()
    {
        var path = PrefabFolder + "/Projectile.prefab";
        var go = new GameObject("Projectile");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Projectiles/FishBone_Projectile.png");
        sr.sortingOrder = 8;
        go.transform.localScale = Vector3.one * 0.04f; 
        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        go.AddComponent<Projectile>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject BuildExpOrbPrefab()
    {
        var path = PrefabFolder + "/ExpOrb.prefab";
        var go = new GameObject("ExpOrb");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0.2f, 0.8f, 1f); 
        sr.sortingOrder = 5;
        go.transform.localScale = Vector3.one * 1.5f; 
        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        go.AddComponent<ExpOrb>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void BindRestartButton(GameOverPanel panel, Transform root)
    {
        var btnTr = root.Find("RestartButton");
        if (btnTr != null) {
            var btn = btnTr.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(btn.onClick, panel.Restart);
        }
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 minAnchor, Vector2 maxAnchor, Vector2 anchoredPos)
    {
        var go = GetOrCreate(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = minAnchor; rect.anchorMax = maxAnchor;
        rect.anchoredPosition = anchoredPos;
        var slider = GetOrAdd<Slider>(go);
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        var fillArea = GetOrCreate("Fill Area", go.transform);
        var fillRect = fillArea.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;
        var fill = GetOrCreate("Fill", fillArea.transform);
        var fillImage = GetOrAdd<Image>(fill);
        fillImage.color = Color.green;
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = null;
        return slider;
    }

    private static TMP_Text CreateTmpText(string name, Transform parent, string text, Vector2 minAnchor, Vector2 maxAnchor, Vector2 anchoredPos, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var go = GetOrCreate(name, parent);
        var tmp = GetOrAdd<TextMeshProUGUI>(go);
        tmp.text = text; tmp.fontSize = 28; tmp.alignment = align; tmp.color = Color.white;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = minAnchor; rect.anchorMax = maxAnchor;
        rect.sizeDelta = new Vector2(600f, 100f);
        rect.anchoredPosition = anchoredPos;
        return tmp;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPos)
    {
        var go = GetOrCreate(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300f, 80f); rect.anchoredPosition = anchoredPos;
        var image = GetOrAdd<Image>(go);
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var btn = GetOrAdd<Button>(go);
        var txtGo = GetOrCreate("Text", go.transform);
        var tmp = GetOrAdd<TextMeshProUGUI>(txtGo);
        tmp.text = label; tmp.alignment = TextAlignmentOptions.Center; tmp.fontSize = 32; tmp.color = Color.white;
        var txtRect = txtGo.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
        return btn;
    }

    private static GameObject GetOrCreate(string name, Transform parent = null)
    {
        Transform found = parent == null ? GameObject.Find(name)?.transform : parent.Find(name);
        if (found != null) return found.gameObject;
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        if (parent != null || name == "Canvas") go.AddComponent<RectTransform>();
        return go;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        if (go == null) return null;
        var component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void SetRef(Object target, string fieldName, Object value)
    {
        if (target == null || value == null) return;
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null) return;
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif