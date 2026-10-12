#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LumenGameSetup
{
    private const string PlatformPrefabPath = "Assets/Prefabs/PhasePlatform.prefab";
    private const string FireflyPrefabPath = "Assets/Prefabs/Firefly.prefab";
    private const string ScenePath = "Assets/Scenes/Lumen.unity";

    [MenuItem("Lumen/Build 2D Game Scene")]
    public static void BuildGameScene()
    {
        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        CreatePlatformPrefab();
        CreateFireflyPrefab();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6.3f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.035f, .045f, .115f);
        cameraObject.AddComponent<AudioListener>();

        var gameObject = new GameObject("Lumen - Game Manager");
        var game = gameObject.AddComponent<LumenGame>();
        game.platformPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlatformPrefabPath);
        game.fireflyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FireflyPrefabPath);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Lumen 2D scene and reusable Prefabs created. Press Play to begin.");
    }

    private static void CreatePlatformPrefab()
    {
        var root = new GameObject("Phase Platform Prefab");
        root.AddComponent<SpriteRenderer>();
        root.AddComponent<BoxCollider2D>();
        root.AddComponent<PhasePlatform2D>();
        PrefabUtility.SaveAsPrefabAsset(root, PlatformPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void CreateFireflyPrefab()
    {
        var root = new GameObject("Firefly Prefab");
        root.AddComponent<SpriteRenderer>();
        var collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        root.AddComponent<FireflyPickup>();
        PrefabUtility.SaveAsPrefabAsset(root, FireflyPrefabPath);
        Object.DestroyImmediate(root);
    }
}
#endif
