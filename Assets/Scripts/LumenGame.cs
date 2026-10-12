using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Owns the 2D level, phase rules, objective, respawns, and simple screen UI.</summary>
public class LumenGame : MonoBehaviour
{
    public enum RunState { Title, Playing, Paused, Won, Lost }

    [Header("Reusable prefabs")]
    public GameObject platformPrefab;
    public GameObject fireflyPrefab;

    [Header("Run settings")]
    [SerializeField] private int requiredFireflies = 6;
    [SerializeField] private int startingHearts = 3;

    private readonly List<PhasePlatform2D> platforms = new List<PhasePlatform2D>();
    private PlayerController2D player;
    private Rigidbody2D playerBody;
    private Vector2 spawnPoint;
    private int foundFireflies;
    private int hearts;
    private WorldPhase currentPhase = WorldPhase.Moon;
    private RunState state = RunState.Title;
    private GoalGate2D gate;
    private GUIStyle titleStyle, subtitleStyle, bodyStyle, smallStyle, buttonStyle, panelStyle;
    private Texture2D panelTexture, buttonTexture;
    private int lastStyleHeight;

    public bool IsPlaying => state == RunState.Playing;
    public bool HasAllFireflies => foundFireflies >= requiredFireflies;
    public WorldPhase CurrentPhase => currentPhase;

    private void Start()
    {
        BuildWorld();
        ApplyPhase();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (state == RunState.Playing) Pause();
            else if (state == RunState.Paused) Resume();
        }
        if (state == RunState.Playing)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame) ShiftPhase();
            if (player != null && player.transform.position.y < -10f) LoseHeart("The mist caught you.");
        }
        if (state == RunState.Won || state == RunState.Lost)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) RestartRun();
        }
    }

    private void BuildWorld()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }
        camera.orthographic = true;
        camera.orthographicSize = 6.3f;
        camera.backgroundColor = new Color(.035f, .045f, .115f);
        camera.transform.position = new Vector3(-5, 1.4f, -10);
        camera.transform.rotation = Quaternion.identity;
        if (camera.GetComponent<FollowCamera2D>() == null) camera.gameObject.AddComponent<FollowCamera2D>();

        BuildBackdrop();
        CreatePlatform(-8.5f, -3.45f, 9f, .9f, null, true);
        CreatePlatform(-1.7f, -2.55f, 2.7f, .55f, WorldPhase.Moon);
        CreatePlatform(2.1f, -1.1f, 2.7f, .55f, WorldPhase.Dawn);
        CreatePlatform(5.9f, .15f, 2.7f, .55f, WorldPhase.Moon);
        CreatePlatform(9.8f, -.75f, 2.7f, .55f, WorldPhase.Dawn);
        CreatePlatform(14.4f, -3.45f, 7.2f, .9f, null, true);
        CreatePlatform(20.7f, -2.35f, 2.8f, .55f, WorldPhase.Moon);
        CreatePlatform(24.6f, -.9f, 2.8f, .55f, WorldPhase.Dawn);
        CreatePlatform(28.5f, .25f, 2.8f, .55f, WorldPhase.Moon);
        CreatePlatform(32.6f, -.8f, 2.8f, .55f, WorldPhase.Dawn);
        CreatePlatform(37f, -3.45f, 7.2f, .9f, null, true);
        CreatePlatform(43f, -2.25f, 2.8f, .55f, WorldPhase.Moon);
        CreatePlatform(47f, -.8f, 2.8f, .55f, WorldPhase.Dawn);
        CreatePlatform(52.6f, -3.45f, 9f, .9f, null, true);

        CreateFirefly(-9f, -1.6f, .1f);
        CreateFirefly(-1.7f, -1.45f, .2f);
        CreateFirefly(5.8f, 1.15f, .6f);
        CreateFirefly(14.2f, -1.35f, .3f);
        CreateFirefly(28.5f, 1.3f, .5f);
        CreateFirefly(37f, -1.35f, .4f);

        CreateWisp(7.5f, 2.1f, 1.4f);
        CreateWisp(30f, 2.05f, 1.6f);

        var gateObject = new GameObject("Moon Gate");
        gateObject.transform.position = new Vector3(54.3f, -2.2f, 0);
        gate = gateObject.AddComponent<GoalGate2D>();
        gate.Configure(this);
        var gateGlow = MakeSprite("Gate Glow", RuntimeSprites.SoftCircle, new Vector2(54.3f, -2.0f), new Vector2(3.4f, 4.3f), new Color(.58f, .38f, 1, .22f), -1);
        gateGlow.transform.SetParent(gateObject.transform, true);
        var gateCore = MakeSprite("Gate Frame", RuntimeSprites.Circle, new Vector2(54.3f, -2.0f), new Vector2(1.65f, 2.5f), new Color(.72f, .58f, 1), 3);
        gateCore.transform.SetParent(gateObject.transform, true);
        var gateHole = MakeSprite("Gate Interior", RuntimeSprites.Circle, new Vector2(54.3f, -2.0f), new Vector2(1.18f, 2.0f), new Color(.06f, .055f, .19f), 4);
        gateHole.transform.SetParent(gateObject.transform, true);
        var gateCollider = gateObject.AddComponent<BoxCollider2D>();
        gateCollider.isTrigger = true;
        gateCollider.size = new Vector2(1.8f, 2.2f);

        spawnPoint = new Vector2(-9f, -2.35f);
        hearts = startingHearts;
        CreatePlayer(spawnPoint);
    }

    private void BuildBackdrop()
    {
        MakeSprite("Far ridge", RuntimeSprites.Circle, new Vector2(12, -1.1f), new Vector2(54, 12), new Color(.075f, .085f, .22f), -20);
        MakeSprite("Near ridge", RuntimeSprites.Circle, new Vector2(30, -3.5f), new Vector2(55, 10), new Color(.09f, .1f, .25f), -18);
        MakeSprite("Distant moon", RuntimeSprites.SoftCircle, new Vector2(42, 5.5f), new Vector2(9, 9), new Color(.75f, .72f, 1, .18f), -16);
        MakeSprite("Moon", RuntimeSprites.Circle, new Vector2(42, 5.5f), new Vector2(3.2f, 3.2f), new Color(.86f, .82f, 1), -15);
        MakeSprite("Moon shadow", RuntimeSprites.Circle, new Vector2(42.8f, 5.8f), new Vector2(3f, 3f), new Color(.22f, .22f, .43f), -14);

        var random = new System.Random(27);
        for (int i = 0; i < 110; i++)
        {
            float x = -18 + (float)random.NextDouble() * 86;
            float y = -1 + (float)random.NextDouble() * 12;
            float size = .035f + (float)random.NextDouble() * .07f;
            Color tint = i % 7 == 0 ? new Color(.98f, .72f, .45f, .8f) : new Color(.62f, .7f, 1f, .7f);
            MakeSprite("Distant star", RuntimeSprites.Circle, new Vector2(x, y), Vector2.one * size, tint, -12);
        }

        for (int x = -10; x < 62; x += 5)
        {
            float y = -2.8f + (x % 3) * .25f;
            var stalk = MakeSprite("Silhouette reeds", RuntimeSprites.Square, new Vector2(x, y + .7f), new Vector2(.08f, 1.6f), new Color(.1f, .11f, .25f), -4);
            stalk.transform.rotation = Quaternion.Euler(0, 0, (x % 2 == 0 ? -1 : 1) * 12f);
            MakeSprite("Reed glow", RuntimeSprites.Circle, new Vector2(x + .15f, y + 1.3f), Vector2.one * .12f, new Color(.45f, .82f, 1f, .8f), -3);
        }
    }

    private void CreatePlatform(float x, float y, float width, float height, WorldPhase? phase, bool safe = false)
    {
        GameObject platformObject;
        if (platformPrefab != null) platformObject = Instantiate(platformPrefab, new Vector3(x, y, 0), Quaternion.identity);
        else platformObject = new GameObject("Phase Platform");
        platformObject.name = phase.HasValue ? (phase == WorldPhase.Moon ? "Moon Platform" : "Dawn Platform") : "Mossy Rest Platform";
        platformObject.transform.position = new Vector3(x, y, 0);
        platformObject.transform.localScale = new Vector3(width, height, 1);
        var renderer = platformObject.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = platformObject.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSprites.Square;
        renderer.sortingOrder = 1;
        var collider = platformObject.GetComponent<BoxCollider2D>();
        if (collider == null) collider = platformObject.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        var platform = platformObject.GetComponent<PhasePlatform2D>();
        if (platform == null) platform = platformObject.AddComponent<PhasePlatform2D>();
        Color color = safe ? new Color(.19f, .35f, .29f) : (phase == WorldPhase.Moon ? new Color(.27f, .65f, .95f) : new Color(1f, .61f, .3f));
        platform.Configure(phase, color, safe);
        platforms.Add(platform);

        var trim = MakeSprite("Luminous platform rim", RuntimeSprites.Square, new Vector2(x, y + height * .5f - .035f), new Vector2(width, .075f), safe ? new Color(.53f, .81f, .56f) : color * 1.3f, 2);
        trim.transform.SetParent(platformObject.transform, true);
        if (safe)
        {
            for (int i = 0; i < Mathf.FloorToInt(width / 1.5f); i++)
                MakeSprite("Tiny moss light", RuntimeSprites.Circle, new Vector2(x - width * .35f + i * 1.5f, y + height * .5f + .08f), Vector2.one * .09f, new Color(.64f, .94f, .55f), 3);
        }
    }

    private void CreateFirefly(float x, float y, float phaseOffset)
    {
        GameObject fly;
        if (fireflyPrefab != null) fly = Instantiate(fireflyPrefab, new Vector3(x, y, 0), Quaternion.identity);
        else fly = new GameObject("Firefly");
        fly.name = "Firefly - collectible";
        fly.transform.position = new Vector3(x, y, 0);
        fly.transform.localScale = Vector3.one * .36f;
        var renderer = fly.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = fly.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSprites.Circle;
        renderer.color = new Color(1, .82f, .45f);
        renderer.sortingOrder = 6;
        var collider = fly.GetComponent<CircleCollider2D>();
        if (collider == null) collider = fly.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = .65f;
        var pickup = fly.GetComponent<FireflyPickup>();
        if (pickup == null) pickup = fly.AddComponent<FireflyPickup>();
        pickup.Configure(this, phaseOffset);
        var halo = MakeSprite("Firefly halo", RuntimeSprites.SoftCircle, fly.transform.position, Vector2.one * 1.3f, new Color(1, .7f, .3f, .32f), 5);
        halo.transform.SetParent(fly.transform, false);
        halo.transform.localPosition = Vector3.zero;
        halo.transform.localScale = Vector3.one * 3.6f;
    }

    private void CreateWisp(float x, float y, float patrol)
    {
        var wisp = new GameObject("Drifting Wisp - hazard");
        wisp.transform.position = new Vector3(x, y, 0);
        wisp.transform.localScale = Vector3.one * .72f;
        var renderer = wisp.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSprites.SoftCircle;
        renderer.color = new Color(.99f, .3f, .65f, .9f);
        renderer.sortingOrder = 5;
        var collider = wisp.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = .7f;
        wisp.AddComponent<WispHazard>().Configure(new Vector2(x, y), patrol);
        var core = MakeSprite("Wisp heart", RuntimeSprites.Circle, new Vector2(x, y), Vector2.one * .23f, new Color(1f, .83f, .9f), 6);
        core.transform.SetParent(wisp.transform, true);
    }

    private void CreatePlayer(Vector2 position)
    {
        var playerObject = new GameObject("Player - the little lantern");
        playerObject.transform.position = position;
        playerBody = playerObject.AddComponent<Rigidbody2D>();
        playerBody.gravityScale = 3.2f;
        playerBody.freezeRotation = true;
        playerBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerBody.simulated = false;
        var capsule = playerObject.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(.62f, .9f);
        var visualRoot = new GameObject("Lantern Art").transform;
        visualRoot.SetParent(playerObject.transform, false);
        var main = MakeSprite("Warm lantern body", RuntimeSprites.Circle, position, new Vector2(.62f, .8f), new Color(1f, .68f, .34f), 8).GetComponent<SpriteRenderer>();
        main.transform.SetParent(visualRoot, false);
        main.transform.localPosition = Vector3.zero;
        for (int side = -1; side <= 1; side += 2)
        {
            var wing = MakeSprite("Small moth wing", RuntimeSprites.SoftCircle, position, new Vector2(.52f, .34f), new Color(.8f, .72f, 1f, .88f), 7);
            wing.transform.SetParent(visualRoot, false);
            wing.transform.localPosition = new Vector3(side * .28f, .18f, 0);
            wing.transform.localRotation = Quaternion.Euler(0, 0, side * 22f);
        }
        MakeEye(visualRoot, new Vector2(-.13f, .05f));
        MakeEye(visualRoot, new Vector2(.13f, .05f));
        player = playerObject.AddComponent<PlayerController2D>();
        player.Configure(this, main, visualRoot);
        player.SetControl(false);
        var glow = MakeSprite("Player glow", RuntimeSprites.SoftCircle, position, new Vector2(1.8f, 1.8f), new Color(1f, .62f, .33f, .18f), 6);
        glow.transform.SetParent(visualRoot, false);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = new Vector3(3, 3, 1);
        spawnPoint = position;
        var cam = Camera.main.GetComponent<FollowCamera2D>();
        cam.SetTarget(playerObject.transform);
    }

    private void MakeEye(Transform parent, Vector2 localPosition)
    {
        var eye = MakeSprite("Lantern eye", RuntimeSprites.Circle, Vector2.zero, Vector2.one * .09f, new Color(.19f, .12f, .2f), 9);
        eye.transform.SetParent(parent, false);
        eye.transform.localPosition = localPosition;
    }

    private GameObject MakeSprite(string name, Sprite sprite, Vector2 position, Vector2 scale, Color color, int order)
    {
        var item = new GameObject(name);
        item.transform.position = new Vector3(position.x, position.y, 0);
        item.transform.localScale = new Vector3(scale.x, scale.y, 1);
        var renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        return item;
    }

    public void BeginRun()
    {
        Time.timeScale = 1;
        state = RunState.Playing;
        player.BeginRun();
    }

    private void ShiftPhase()
    {
        currentPhase = currentPhase == WorldPhase.Moon ? WorldPhase.Dawn : WorldPhase.Moon;
        ApplyPhase();
    }

    private void ApplyPhase()
    {
        foreach (var platform in platforms)
            if (platform != null) platform.ApplyPhase(currentPhase);
    }

    public void CollectFirefly()
    {
        foundFireflies = Mathf.Min(foundFireflies + 1, requiredFireflies);
        if (gate != null) gate.UpdateLock(HasAllFireflies);
    }

    public void TouchGate()
    {
        if (!HasAllFireflies || state != RunState.Playing) return;
        state = RunState.Won;
        player.SetControl(false);
        playerBody.simulated = false;
    }

    public void PlayerHit()
    {
        if (state != RunState.Playing) return;
        LoseHeart("The wisp swept you back.");
    }

    private void LoseHeart(string reason)
    {
        hearts--;
        if (hearts <= 0)
        {
            hearts = 0;
            state = RunState.Lost;
            player.SetControl(false);
            playerBody.simulated = false;
            return;
        }
        player.ResetAt(spawnPoint);
    }

    private void Pause()
    {
        state = RunState.Paused;
        Time.timeScale = 0;
    }

    private void Resume()
    {
        Time.timeScale = 1;
        state = RunState.Playing;
    }

    private void RestartRun()
    {
        Time.timeScale = 1;
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
    }

    private void OnGUI()
    {
        EnsureStyles();
        if (state == RunState.Playing || state == RunState.Paused || state == RunState.Won || state == RunState.Lost)
            DrawHud();
        if (state == RunState.Title) DrawTitle();
        else if (state == RunState.Paused) DrawPause();
        else if (state == RunState.Won) DrawEndCard(true);
        else if (state == RunState.Lost) DrawEndCard(false);
    }

    private void DrawHud()
    {
        float scale = Mathf.Clamp(Screen.height / 900f, .7f, 1.5f);
        Rect card = new Rect(24 * scale, 22 * scale, 305 * scale, 116 * scale);
        GUI.Box(card, GUIContent.none, panelStyle);
        GUI.Label(new Rect(card.x + 20 * scale, card.y + 12 * scale, card.width - 40 * scale, 34 * scale), "FIREFLIES   " + foundFireflies + " / " + requiredFireflies, bodyStyle);
        GUI.Label(new Rect(card.x + 20 * scale, card.y + 46 * scale, card.width - 40 * scale, 32 * scale), "HEARTS   " + new string('♥', hearts) + new string('♡', startingHearts - hearts), bodyStyle);
        string phaseName = currentPhase == WorldPhase.Moon ? "MOON PHASE  ·  Q" : "DAWN PHASE  ·  Q";
        GUI.Label(new Rect(card.x + 20 * scale, card.y + 81 * scale, card.width - 40 * scale, 26 * scale), phaseName, smallStyle);
        GUI.Label(new Rect(Screen.width - 310 * scale, 26 * scale, 285 * scale, 35 * scale), "A / D  move    SPACE  jump    SHIFT  dash", smallStyle);
        GUI.Label(new Rect(Screen.width - 310 * scale, 54 * scale, 285 * scale, 30 * scale), "Q  shift the world    ESC  pause", smallStyle);
        GUI.Label(new Rect(24 * scale, Screen.height - 53 * scale, 420 * scale, 28 * scale), HasAllFireflies ? "The moon gate is awake. Find the arch!" : "The fireflies remember the way home.", smallStyle);
    }

    private void DrawTitle()
    {
        DrawDimmer();
        Rect panel = CenterPanel(650, 500);
        float s = panel.width / 650f;
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 30 * s, panel.y + 34 * s, panel.width - 60 * s, 72 * s), "LUMEN", titleStyle);
        GUI.Label(new Rect(panel.x + 38 * s, panel.y + 106 * s, panel.width - 76 * s, 46 * s), "THE LITTLE LIGHT BETWEEN WORLDS", subtitleStyle);
        GUI.Label(new Rect(panel.x + 58 * s, panel.y + 178 * s, panel.width - 116 * s, 88 * s), "Gather six fireflies and find the moon gate.\nQ shifts the world between Moon and Dawn:\neach phase reveals a different path.", bodyStyle);
        Rect start = new Rect(panel.center.x - 145 * s, panel.y + 294 * s, 290 * s, 60 * s);
        if (GUI.Button(start, "BEGIN THE JOURNEY", buttonStyle)) BeginRun();
        GUI.Label(new Rect(panel.x + 40 * s, panel.y + 380 * s, panel.width - 80 * s, 54 * s), "A / D move     SPACE jump     SHIFT dash\nUse your dash to cross the longer gaps.", smallStyle);
        GUI.Label(new Rect(panel.x + 40 * s, panel.y + panel.height - 45 * s, panel.width - 80 * s, 26 * s), "A small 2D platform game about carrying light.", smallStyle);
    }

    private void DrawPause()
    {
        DrawDimmer();
        Rect panel = CenterPanel(520, 335);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 20, panel.y + 26, panel.width - 40, 60), "TAKE A BREATH", titleStyle);
        GUI.Label(new Rect(panel.x + 35, panel.y + 99, panel.width - 70, 44), "The little light is waiting.", bodyStyle);
        if (GUI.Button(new Rect(panel.center.x - 135, panel.y + 168, 270, 56), "KEEP GOING", buttonStyle)) Resume();
        if (GUI.Button(new Rect(panel.center.x - 135, panel.y + 238, 270, 50), "START OVER  ·  R", buttonStyle)) RestartRun();
    }

    private void DrawEndCard(bool won)
    {
        DrawDimmer();
        Rect panel = CenterPanel(580, 370);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 28, panel.y + 30, panel.width - 56, 66), won ? "LIGHT FINDS A WAY" : "THE NIGHT GREW QUIET", titleStyle);
        GUI.Label(new Rect(panel.x + 44, panel.y + 108, panel.width - 88, 54), won ? "You brought every firefly safely home." : "The lantern has one more journey in it.", bodyStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 164, panel.width - 100, 34), "Fireflies found: " + foundFireflies + " / " + requiredFireflies, subtitleStyle);
        if (GUI.Button(new Rect(panel.center.x - 138, panel.y + 224, 276, 58), "PLAY AGAIN  ·  R", buttonStyle)) RestartRun();
    }

    private void DrawDimmer()
    {
        GUI.color = new Color(.015f, .02f, .08f, .77f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private Rect CenterPanel(float width, float height)
    {
        float fit = Mathf.Min(Screen.width / width, Screen.height / height) * .88f;
        float w = width * fit;
        float h = height * fit;
        return new Rect((Screen.width - w) * .5f, (Screen.height - h) * .5f, w, h);
    }

    private void EnsureStyles()
    {
        if (titleStyle != null && lastStyleHeight == Screen.height) return;
        lastStyleHeight = Screen.height;
        float scale = Mathf.Clamp(Screen.height / 900f, .62f, 1.6f);
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(56 * scale), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        titleStyle.normal.textColor = new Color(1f, .82f, .56f);
        subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(20 * scale), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        subtitleStyle.normal.textColor = new Color(.74f, .8f, 1f);
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(19 * scale), alignment = TextAnchor.MiddleCenter, wordWrap = true };
        bodyStyle.normal.textColor = new Color(.91f, .91f, 1f);
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(14 * scale), alignment = TextAnchor.MiddleCenter, wordWrap = true };
        smallStyle.normal.textColor = new Color(.69f, .73f, .88f);
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(18 * scale), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        buttonStyle.normal.background = SolidTexture(new Color(.38f, .26f, .7f));
        buttonStyle.hover.background = SolidTexture(new Color(.52f, .37f, .84f));
        buttonStyle.active.background = SolidTexture(new Color(.26f, .2f, .55f));
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        panelTexture = SolidTexture(new Color(.055f, .065f, .16f, .97f));
        panelStyle = new GUIStyle(GUI.skin.box) { normal = { background = panelTexture } };
    }

    private Texture2D SolidTexture(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
