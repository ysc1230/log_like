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
using UnityEngine.UI;

public static class MainSceneAutoBuilder
{
    private const string ScenePath = "Assets/Scenes/MainScene.unity";
    private const string PrefabFolder = "Assets/Prefabs";

    [MenuItem("Tools/Survivor2D/Build MainScene")]
    public static void BuildMainScene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            var camGo = new GameObject("Main Camera");
            mainCamera = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }

        mainCamera.orthographic = true;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);

        EnsureDirectionalLight();
        Directory.CreateDirectory(PrefabFolder);

        var gameRoot = GetOrCreate("GameRoot");
        var bootstrap = GetOrAdd<GameBootstrap>(gameRoot);
        var spawner = GetOrAdd<EnemySpawner>(gameRoot);
        var registry = GetOrAdd<EnemyRegistry>(gameRoot);
        var levelSystem = GetOrAdd<LevelSystem>(gameRoot);
        var projectilePool = GetOrAdd<ProjectilePool>(gameRoot);

        var player = BuildPlayer(mainCamera);
        var input = BuildInputUI(out var joystickArea, out var canvas, out var hud, out var levelUpPanel, out var gameOverPanel);

        var enemyPrefab = BuildEnemyPrefab();
        var projectilePrefab = BuildProjectilePrefab();
        var expOrbPrefab = BuildExpOrbPrefab();

        enemyPrefab.GetComponent<EnemyController>().GetType();
        SetRef(enemyPrefab.GetComponent<EnemyController>(), "expOrbPrefab", expOrbPrefab);

        SetRef(levelSystem, "playerStats", player.GetComponent<PlayerStats>());
        SetRef(spawner, "enemyPrefab", enemyPrefab.GetComponent<EnemyController>());
        SetRef(spawner, "player", player.transform);
        SetRef(spawner, "mainCamera", mainCamera);
        SetRef(spawner, "enemyRegistry", registry);
        SetRef(spawner, "levelSystem", levelSystem);

        SetRef(projectilePool, "projectilePrefab", projectilePrefab.GetComponent<Projectile>());

        var stats = player.GetComponent<PlayerStats>();
        SetRef(player.GetComponent<PlayerMover>(), "playerStats", stats);
        SetRef(player.GetComponent<PlayerMover>(), "input", input);
        SetRef(player.GetComponent<PlayerMover>(), "mainCamera", mainCamera);

        SetRef(player.GetComponent<AutoAttacker>(), "playerStats", stats);
        SetRef(player.GetComponent<AutoAttacker>(), "projectilePool", projectilePool);
        SetRef(player.GetComponent<AutoAttacker>(), "firePoint", player.transform.Find("FirePoint"));
        SetRef(player.GetComponent<AutoAttacker>(), "enemyRegistry", registry);

        SetRef(player.GetComponent<LevelSystemLink>(), "levelSystem", levelSystem);

        SetRef(bootstrap, "playerStats", stats);
        SetRef(bootstrap, "levelSystem", levelSystem);
        SetRef(bootstrap, "levelUpPanel", levelUpPanel);

        SetRef(levelUpPanel, "root", levelUpPanel.gameObject);
        SetRef(levelUpPanel, "levelSystem", levelSystem);
        SetRef(gameOverPanel, "root", gameOverPanel.gameObject);

        SetRef(hud, "hpSlider", canvas.transform.Find("HUD/HPBar").GetComponent<Slider>());
        SetRef(hud, "expSlider", canvas.transform.Find("HUD/EXPBar").GetComponent<Slider>());
        SetRef(hud, "levelText", canvas.transform.Find("HUD/LevelText").GetComponent<TMP_Text>());
        SetRef(hud, "timeText", canvas.transform.Find("HUD/TimeText").GetComponent<TMP_Text>());

        SetRef(input, "joystickArea", joystickArea);

        BindLevelUpButtons(levelUpPanel, canvas.transform.Find("LevelUpPanel"));
        BindRestartButton(gameOverPanel, canvas.transform.Find("GameOverPanel"));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("MainScene 자동 구성이 완료되었습니다.");
    }

    private static void EnsureDirectionalLight()
    {
        var light = Object.FindObjectOfType<Light>();
        if (light != null) return;
        var go = new GameObject("Directional Light");
        light = go.AddComponent<Light>();
        light.type = LightType.Directional;
    }

    private static GameObject BuildPlayer(Camera mainCamera)
    {
        var player = GetOrCreate("Player");
        if (player.GetComponent<SpriteRenderer>() == null)
            player.AddComponent<SpriteRenderer>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var rb = GetOrAdd<Rigidbody2D>(player);
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        GetOrAdd<CircleCollider2D>(player);
        GetOrAdd<PlayerStats>(player);
        GetOrAdd<PlayerMover>(player);
        GetOrAdd<AutoAttacker>(player);
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

    private static TouchJoystickInput BuildInputUI(out RectTransform joystickArea, out Canvas canvas, out HudController hudController, out LevelUpPanel levelUpPanel, out GameOverPanel gameOverPanel)
    {
        var canvasGo = GetOrCreate("Canvas");
        canvas = GetOrAdd<Canvas>(canvasGo);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        GetOrAdd<CanvasScaler>(canvasGo).uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        GetOrAdd<GraphicRaycaster>(canvasGo);

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var hud = GetOrCreate("HUD", canvasGo.transform);
        var hpBar = CreateSlider("HPBar", hud.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f));
        var expBar = CreateSlider("EXPBar", hud.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f));
        var levelText = CreateTmpText("LevelText", hud.transform, "Lv 1", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -10f));
        var timeText = CreateTmpText("TimeText", hud.transform, "Time 0.0s", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -10f), TextAlignmentOptions.TopRight);

        var joystick = GetOrCreate("JoystickArea", canvasGo.transform);
        joystickArea = joystick.GetComponent<RectTransform>();
        joystickArea.anchorMin = new Vector2(0f, 0f);
        joystickArea.anchorMax = new Vector2(0f, 0f);
        joystickArea.sizeDelta = new Vector2(220f, 220f);
        joystickArea.anchoredPosition = new Vector2(130f, 130f);
        var joyImage = GetOrAdd<Image>(joystick);
        joyImage.color = new Color(1f, 1f, 1f, 0.15f);

        var levelPanelGo = GetOrCreate("LevelUpPanel", canvasGo.transform);
        levelPanelGo.SetActive(false);
        var levelPanelImage = GetOrAdd<Image>(levelPanelGo);
        levelPanelImage.color = new Color(0f, 0f, 0f, 0.75f);
        var levelRect = levelPanelGo.GetComponent<RectTransform>();
        levelRect.anchorMin = Vector2.zero; levelRect.anchorMax = Vector2.one; levelRect.offsetMin = Vector2.zero; levelRect.offsetMax = Vector2.zero;
        CreateButton("AttackDamageButton", levelPanelGo.transform, "ATK +", new Vector2(0f, 60f));
        CreateButton("AttackSpeedButton", levelPanelGo.transform, "ASPD +", new Vector2(0f, 0f));
        CreateButton("MoveSpeedButton", levelPanelGo.transform, "MSPD +", new Vector2(0f, -60f));

        var gameOverGo = GetOrCreate("GameOverPanel", canvasGo.transform);
        gameOverGo.SetActive(false);
        var gameOverImage = GetOrAdd<Image>(gameOverGo);
        gameOverImage.color = new Color(0f, 0f, 0f, 0.75f);
        var gameOverRect = gameOverGo.GetComponent<RectTransform>();
        gameOverRect.anchorMin = Vector2.zero; gameOverRect.anchorMax = Vector2.one; gameOverRect.offsetMin = Vector2.zero; gameOverRect.offsetMax = Vector2.zero;
        CreateTmpText("GameOverText", gameOverGo.transform, "Game Over", new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, TextAlignmentOptions.Center);
        CreateButton("RestartButton", gameOverGo.transform, "Restart", new Vector2(0f, -20f));

        hudController = GetOrAdd<HudController>(hud);
        levelUpPanel = GetOrAdd<LevelUpPanel>(levelPanelGo);
        gameOverPanel = GetOrAdd<GameOverPanel>(gameOverGo);

        var input = GetOrAdd<TouchJoystickInput>(canvasGo);
        return input;
    }

    private static GameObject BuildEnemyPrefab()
    {
        var path = PrefabFolder + "/Enemy.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("Enemy");
        go.AddComponent<SpriteRenderer>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
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
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("Projectile");
        go.transform.localScale = Vector3.one * 0.25f;
        go.AddComponent<SpriteRenderer>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
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
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("ExpOrb");
        go.transform.localScale = Vector3.one * 0.3f;
        go.AddComponent<SpriteRenderer>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        go.AddComponent<ExpOrb>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void BindLevelUpButtons(LevelUpPanel panel, Transform root)
    {
        var atkBtn = root.Find("AttackDamageButton").GetComponent<Button>();
        var aspdBtn = root.Find("AttackSpeedButton").GetComponent<Button>();
        var mspdBtn = root.Find("MoveSpeedButton").GetComponent<Button>();
        atkBtn.onClick.RemoveAllListeners();
        aspdBtn.onClick.RemoveAllListeners();
        mspdBtn.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(atkBtn.onClick, panel.SelectAttackDamage);
        UnityEventTools.AddPersistentListener(aspdBtn.onClick, panel.SelectAttackSpeed);
        UnityEventTools.AddPersistentListener(mspdBtn.onClick, panel.SelectMoveSpeed);
    }

    private static void BindRestartButton(GameOverPanel panel, Transform root)
    {
        var btn = root.Find("RestartButton").GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(btn.onClick, panel.Restart);
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 minAnchor, Vector2 maxAnchor, Vector2 anchoredPos)
    {
        var go = GetOrCreate(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = minAnchor; rect.anchorMax = maxAnchor; rect.sizeDelta = new Vector2(300f, 20f); rect.anchoredPosition = anchoredPos;
        var image = GetOrAdd<Image>(go);
        image.color = Color.gray;
        var slider = GetOrAdd<Slider>(go);
        slider.targetGraphic = image;

        var fillArea = GetOrCreate("Fill Area", go.transform);
        var fillRect = fillArea.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = new Vector2(5f, 5f); fillRect.offsetMax = new Vector2(-5f, -5f);
        var fill = GetOrCreate("Fill", fillArea.transform);
        var fillImage = GetOrAdd<Image>(fill);
        fillImage.color = Color.green;
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = null;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        return slider;
    }

    private static TMP_Text CreateTmpText(string name, Transform parent, string text, Vector2 minAnchor, Vector2 maxAnchor, Vector2 anchoredPos, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var go = GetOrCreate(name, parent);
        var tmp = GetOrAdd<TextMeshProUGUI>(go);
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = align;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = minAnchor; rect.anchorMax = maxAnchor; rect.sizeDelta = new Vector2(300f, 50f); rect.anchoredPosition = anchoredPos;
        return tmp;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPos)
    {
        var go = GetOrCreate(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(260f, 45f); rect.anchoredPosition = anchoredPos;
        var image = GetOrAdd<Image>(go);
        image.color = new Color(1f, 1f, 1f, 0.9f);
        var btn = GetOrAdd<Button>(go);

        var txt = GetOrCreate("Text", go.transform);
        var tmp = GetOrAdd<TextMeshProUGUI>(txt);
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        var txtRect = txt.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
        return btn;
    }

    private static GameObject GetOrCreate(string name, Transform parent = null)
    {
        Transform found = parent == null ? GameObject.Find(name)?.transform : parent.Find(name);
        if (found != null) return found.gameObject;
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        if (parent != null) go.AddComponent<RectTransform>();
        return go;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void SetRef(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null) return;
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
