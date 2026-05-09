// File: UnityPrototype/Assets/Editor/MainSceneAutoBuilder.cs
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

        SetupCamera(mainCamera);
        EnsureDirectionalLight();
        BuildBackground();
        Directory.CreateDirectory(PrefabFolder);

        var gameRoot = GetOrCreate("GameRoot");
        var bootstrap = GetOrAdd<GameBootstrap>(gameRoot);
        var spawner = GetOrAdd<EnemySpawner>(gameRoot);
        var registry = GetOrAdd<EnemyRegistry>(gameRoot);
        var levelSystem = GetOrAdd<LevelSystem>(gameRoot);
        var projectilePool = GetOrAdd<ProjectilePool>(gameRoot);

        var player = BuildPlayer();
        var input = BuildInputUI(out var joystickArea, out var canvas, out var hud, out var levelUpPanel, out var gameOverPanel);

        var enemyPrefab = BuildEnemyPrefab();
        var projectilePrefab = BuildProjectilePrefab();
        var expOrbPrefab = BuildExpOrbPrefab();

        SetRef(enemyPrefab.GetComponent<EnemyController>(), "expOrbPrefab", expOrbPrefab);

        var stats = player.GetComponent<PlayerStats>();
        SetRef(levelSystem, "playerStats", stats);
        SetRef(spawner, "enemyPrefab", enemyPrefab.GetComponent<EnemyController>());
        SetRef(spawner, "player", player.transform);
        SetRef(spawner, "mainCamera", mainCamera);
        SetRef(spawner, "enemyRegistry", registry);
        SetRef(spawner, "levelSystem", levelSystem);

        SetRef(projectilePool, "projectilePrefab", projectilePrefab.GetComponent<Projectile>());

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

        SetRef(hud, "hpSlider", canvas.transform.Find("HUD/InfoPanel/HPBar").GetComponent<Slider>());
        SetRef(hud, "expSlider", canvas.transform.Find("BottomExpBar").GetComponent<Slider>());
        SetRef(hud, "levelText", canvas.transform.Find("HUD/InfoPanel/LevelText").GetComponent<TMP_Text>());
        SetRef(hud, "timeText", canvas.transform.Find("HUD/TimeText").GetComponent<TMP_Text>());

        SetRef(input, "joystickArea", joystickArea);

        BindLevelUpButtons(levelUpPanel, canvas.transform.Find("LevelUpPanel"));
        BindRestartButton(gameOverPanel, canvas.transform.Find("GameOverPanel"));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("MainScene 자동 구성이 완료되었습니다.");
    }

    private static void SetupCamera(Camera mainCamera)
    {
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 10f;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.08f, 0.1f, 0.14f, 1f);
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void EnsureDirectionalLight()
    {
        var light = Object.FindObjectOfType<Light>();
        if (light != null)
        {
            light.enabled = false;
            return;
        }

        var go = new GameObject("Directional Light");
        light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.enabled = false;
    }

    private static void BuildBackground()
    {
        var background = GetOrCreate("Background");
        var renderer = GetOrAdd<SpriteRenderer>(background);
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        renderer.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        renderer.sortingOrder = -100;
        background.transform.position = Vector3.zero;
        background.transform.localScale = new Vector3(40f, 24f, 1f);
    }

    private static GameObject BuildPlayer()
    {
        var player = GetOrCreate("Player");
        var sr = GetOrAdd<SpriteRenderer>(player);
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0.25f, 0.6f, 1f, 1f);
        sr.sortingOrder = 10;
        player.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

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

    private static TouchJoystickInput BuildInputUI(
        out RectTransform joystickArea,
        out Canvas canvas,
        out HudController hudController,
        out LevelUpPanel levelUpPanel,
        out GameOverPanel gameOverPanel)
    {
        var canvasGo = GetOrCreate("Canvas");
        canvas = GetOrAdd<Canvas>(canvasGo);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = GetOrAdd<CanvasScaler>(canvasGo);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        GetOrAdd<GraphicRaycaster>(canvasGo);

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var hud = RecreateChild("HUD", canvasGo.transform);

        RemoveLegacyUi(canvasGo.transform, "HPBar");
        RemoveLegacyUi(canvasGo.transform, "EXPBar");

        var timeText = CreateTmpText(
            "TimeText",
            hud.transform,
            "00:00",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -36f),
            TextAlignmentOptions.Center);
        timeText.fontSize = 34;

        var infoPanel = GetOrCreate("InfoPanel", hud.transform);
        var infoPanelRect = infoPanel.GetComponent<RectTransform>();
        infoPanelRect.anchorMin = new Vector2(0f, 1f);
        infoPanelRect.anchorMax = new Vector2(0f, 1f);
        infoPanelRect.pivot = new Vector2(0f, 1f);
        infoPanelRect.sizeDelta = new Vector2(360f, 280f);
        infoPanelRect.anchoredPosition = new Vector2(20f, -20f);
        var infoBg = GetOrAdd<Image>(infoPanel);
        infoBg.color = new Color(0f, 0f, 0f, 0.35f);

        CreateLabel("HPLabel", infoPanel.transform, "HP", new Vector2(12f, -16f));
        var hpBar = CreateSlider("HPBar", infoPanel.transform, new Vector2(12f, -54f), new Vector2(330f, 18f), new Color(0.82f, 0.24f, 0.24f, 1f));
        hpBar.interactable = false;
        CreateStatText("HPValueText", infoPanel.transform, "100 / 100", new Vector2(14f, -74f));

        var levelText = CreateTmpText("LevelText", infoPanel.transform, "Lv 1", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -84f));
        levelText.fontSize = 28;

        CreateLabel("ExpLabel", infoPanel.transform, "EXP", new Vector2(12f, -130f));
        var miniExp = CreateSlider("MiniEXPBar", infoPanel.transform, new Vector2(12f, -168f), new Vector2(330f, 18f), new Color(0.2f, 0.75f, 1f, 1f));
        miniExp.interactable = false;
        CreateStatText("EXPValueText", infoPanel.transform, "0 / 10", new Vector2(14f, -188f));

        CreateStatText("AttackDamageText", infoPanel.transform, "ATK 10", new Vector2(14f, -216f));
        CreateStatText("AttackSpeedText", infoPanel.transform, "ASPD 1.00", new Vector2(14f, -242f));
        CreateStatText("MoveSpeedText", infoPanel.transform, "MSPD 5.00", new Vector2(14f, -268f));

        var bottomExp = CreateSlider("BottomExpBar", canvasGo.transform, new Vector2(26f, 20f), new Vector2(1028f, 18f), new Color(0.18f, 0.65f, 1f, 1f));
        var bottomRect = bottomExp.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0f, 0f);
        bottomRect.anchorMax = new Vector2(0f, 0f);
        bottomExp.interactable = false;

        var joystick = GetOrCreate("JoystickArea", canvasGo.transform);
        joystickArea = joystick.GetComponent<RectTransform>();
        joystickArea.anchorMin = new Vector2(0f, 0f);
        joystickArea.anchorMax = new Vector2(0f, 0f);
        joystickArea.pivot = new Vector2(0.5f, 0.5f);
        joystickArea.sizeDelta = new Vector2(220f, 220f);
        joystickArea.anchoredPosition = new Vector2(150f, 170f);
        var joyImage = GetOrAdd<Image>(joystick);
        joyImage.color = new Color(1f, 1f, 1f, 0.08f);

        var levelPanelGo = GetOrCreate("LevelUpPanel", canvasGo.transform);
        levelPanelGo.SetActive(false);
        StretchFullScreen(levelPanelGo.GetComponent<RectTransform>());
        var levelPanelImage = GetOrAdd<Image>(levelPanelGo);
        levelPanelImage.color = new Color(0f, 0f, 0f, 0.7f);
        CreateButton("AttackDamageButton", levelPanelGo.transform, "ATK +", new Vector2(0f, 80f));
        CreateButton("AttackSpeedButton", levelPanelGo.transform, "ASPD +", new Vector2(0f, 20f));
        CreateButton("MoveSpeedButton", levelPanelGo.transform, "MSPD +", new Vector2(0f, -40f));

        var gameOverGo = GetOrCreate("GameOverPanel", canvasGo.transform);
        gameOverGo.SetActive(false);
        StretchFullScreen(gameOverGo.GetComponent<RectTransform>());
        var gameOverImage = GetOrAdd<Image>(gameOverGo);
        gameOverImage.color = new Color(0f, 0f, 0f, 0.65f);
        CreateTmpText("GameOverText", gameOverGo.transform, "GAME OVER", new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, TextAlignmentOptions.Center);
        CreateButton("RestartButton", gameOverGo.transform, "Restart", new Vector2(0f, -40f));

        hudController = GetOrAdd<HudController>(hud);
        levelUpPanel = GetOrAdd<LevelUpPanel>(levelPanelGo);
        gameOverPanel = GetOrAdd<GameOverPanel>(gameOverGo);

        return GetOrAdd<TouchJoystickInput>(canvasGo);
    }

    private static GameObject BuildEnemyPrefab()
    {
        var path = PrefabFolder + "/Enemy.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            UpdateEnemyVisual(existing);
            return existing;
        }

        var go = new GameObject("Enemy");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 0.25f, 0.25f, 1f);
        sr.sortingOrder = 9;
        go.transform.localScale = new Vector3(0.38f, 0.38f, 1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        go.AddComponent<CircleCollider2D>();
        go.AddComponent<EnemyController>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void UpdateEnemyVisual(GameObject prefab)
    {
        var sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(1f, 0.25f, 0.25f, 1f);
            sr.sortingOrder = 9;
        }

        prefab.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
    }

    private static GameObject BuildProjectilePrefab()
    {
        var path = PrefabFolder + "/Projectile.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            UpdateProjectileVisual(existing);
            return existing;
        }

        var go = new GameObject("Projectile");
        go.transform.localScale = new Vector3(0.16f, 0.16f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 0.9f, 0.2f, 1f);
        sr.sortingOrder = 12;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        go.AddComponent<Projectile>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void UpdateProjectileVisual(GameObject prefab)
    {
        var sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(1f, 0.9f, 0.2f, 1f);
            sr.sortingOrder = 12;
        }

        prefab.transform.localScale = new Vector3(0.16f, 0.16f, 1f);
    }

    private static GameObject BuildExpOrbPrefab()
    {
        var path = PrefabFolder + "/ExpOrb.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            UpdateExpOrbVisual(existing);
            return existing;
        }

        var go = new GameObject("ExpOrb");
        go.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0.35f, 0.85f, 1f, 1f);
        sr.sortingOrder = 8;

        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        go.AddComponent<ExpOrb>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void UpdateExpOrbVisual(GameObject prefab)
    {
        var sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(0.35f, 0.85f, 1f, 1f);
            sr.sortingOrder = 8;
        }

        prefab.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
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

    private static Slider CreateSlider(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color fillColor)
    {
        var go = GetOrCreate(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        var bg = GetOrAdd<Image>(go);
        bg.color = new Color(0f, 0f, 0f, 0.45f);

        var slider = GetOrAdd<Slider>(go);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.direction = Slider.Direction.LeftToRight;
        slider.targetGraphic = bg;

        var fillArea = GetOrCreate("Fill Area", go.transform);
        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2f, 2f);
        fillAreaRect.offsetMax = new Vector2(-2f, -2f);

        var fill = GetOrCreate("Fill", fillArea.transform);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        var fillImage = GetOrAdd<Image>(fill);
        fillImage.color = fillColor;

        slider.fillRect = fillRect;
        slider.handleRect = null;
        return slider;
    }

    private static TMP_Text CreateTmpText(
        string name,
        Transform parent,
        string text,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPos,
        TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var go = GetOrCreate(name, parent);
        var tmp = GetOrAdd<TextMeshProUGUI>(go);
        tmp.text = text;
        tmp.fontSize = 26;
        tmp.color = Color.white;
        tmp.alignment = align;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMin.y);
        rect.sizeDelta = new Vector2(400f, 42f);
        rect.anchoredPosition = anchoredPos;
        return tmp;
    }

    private static TMP_Text CreateLabel(string name, Transform parent, string text, Vector2 anchoredPos)
    {
        var label = CreateTmpText(name, parent, text, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPos);
        label.fontSize = 20;
        label.color = new Color(0.85f, 0.9f, 1f, 0.95f);
        return label;
    }

    private static TMP_Text CreateStatText(string name, Transform parent, string text, Vector2 anchoredPos)
    {
        var stat = CreateTmpText(name, parent, text, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPos);
        stat.fontSize = 18;
        stat.color = new Color(0.95f, 0.97f, 1f, 0.95f);
        return stat;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPos)
    {
        var go = GetOrCreate(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(280f, 52f);
        rect.anchoredPosition = anchoredPos;

        var image = GetOrAdd<Image>(go);
        image.color = new Color(1f, 1f, 1f, 0.92f);

        var btn = GetOrAdd<Button>(go);

        var txt = GetOrCreate("Text", go.transform);
        var tmp = GetOrAdd<TextMeshProUGUI>(txt);
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = new Color(0.1f, 0.12f, 0.16f, 1f);

        var txtRect = txt.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return btn;
    }

    private static void StretchFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject GetOrCreate(string name, Transform parent = null)
    {
        Transform found = parent == null ? GameObject.Find(name)?.transform : parent.Find(name);
        if (found != null) return found.gameObject;

        var go = new GameObject(name);
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
        }

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

    private static GameObject RecreateChild(string name, Transform parent)
    {
        var existing = parent.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void RemoveLegacyUi(Transform root, string childName)
    {
        var legacy = root.Find(childName);
        if (legacy != null)
        {
            Object.DestroyImmediate(legacy.gameObject);
        }
    }
}
#endif
