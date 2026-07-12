using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// One-click builder for the WorldSelection blockout scene.
/// Menu: Tools > World Selection > Build Blockout Scene.
/// Idempotent: wipes the scene's root objects and rebuilds everything, so it can
/// be re-run safely until real diorama art replaces the blockout.
/// </summary>
public static class WorldSelectionSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Selection/WorldSelection.unity";
    private const string MaterialsFolder = "Assets/Scenes/Selection/Materials";

    private const string Story1Path = "Assets/Resources/Config/Story/Story_1_RumahNoni.asset";
    private const string Story2Path = "Assets/Resources/Config/Story/Story_2_Placeholder.asset";
    private const string Story3Path = "Assets/Resources/Config/Story/Story_3_Placeholder.asset";

    private class BlockoutMaterials
    {
        public Material Ground, Building, Roof, Door, Locked, Marker;
    }

    [MenuItem("Tools/World Selection/Build Blockout Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
        {
            Object.DestroyImmediate(root);
        }

        var materials = CreateMaterials();

        BuildLighting();
        BuildGround(materials);
        var (rig, camera) = BuildCameraRig();

        BuildBuilding("Building_Story1", Story1Path, new Vector3(-14f, 0f, 8f), 25f, materials);
        BuildBuilding("Building_Story2", Story2Path, new Vector3(11f, 0f, 15f), -20f, materials);
        BuildBuilding("Building_Story3", Story3Path, new Vector3(4f, 0f, -16f), 160f, materials);

        var (infoPanel, introPanel, backButton) = BuildUI();
        var controller = BuildController(rig, camera, infoPanel, introPanel);

        UnityEventTools.AddPersistentListener(backButton.onClick, new UnityAction(controller.BackToMenu));

        BuildEventSystem();
        EnsureSceneInBuildSettings();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[WorldSelectionSceneBuilder] WorldSelection blockout built and saved.");
    }

    // ------------------------------------------------------------------ world

    private static void BuildLighting()
    {
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.87f, 0.75f);
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
    }

    private static void BuildGround(BlockoutMaterials materials)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(90f, 1f, 90f);
        ground.GetComponent<MeshRenderer>().sharedMaterial = materials.Ground;
    }

    private static (WorldOrbitCameraRig rig, Camera camera) BuildCameraRig()
    {
        var rigGO = new GameObject("CameraRig");
        rigGO.transform.position = Vector3.zero;
        var rig = rigGO.AddComponent<WorldOrbitCameraRig>();

        var cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        var camera = cameraGO.AddComponent<Camera>();
        camera.fieldOfView = 50f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 500f;
        cameraGO.AddComponent<AudioListener>();

        SetRef(rig, "cameraTransform", cameraGO.transform);
        return (rig, camera);
    }

    private static void BuildBuilding(string name, string storyAssetPath, Vector3 position, float yaw, BlockoutMaterials materials)
    {
        var story = AssetDatabase.LoadAssetAtPath<StoryDefinition>(storyAssetPath);
        if (story == null)
        {
            Debug.LogWarning($"[WorldSelectionSceneBuilder] Story asset not found: {storyAssetPath}");
        }

        var root = new GameObject(name);
        root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, 3.5f, 0f);
        body.transform.localScale = new Vector3(7f, 7f, 7f);
        body.GetComponent<MeshRenderer>().sharedMaterial = materials.Building;

        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(root.transform, false);
        roof.transform.localPosition = new Vector3(0f, 7.6f, 0f);
        roof.transform.localScale = new Vector3(8f, 1.2f, 8f);
        roof.GetComponent<MeshRenderer>().sharedMaterial = materials.Roof;
        Object.DestroyImmediate(roof.GetComponent<Collider>());

        var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(root.transform, false);
        door.transform.localPosition = new Vector3(0f, 1.4f, -3.55f);
        door.transform.localScale = new Vector3(1.6f, 2.8f, 0.25f);
        door.GetComponent<MeshRenderer>().sharedMaterial = materials.Door;
        Object.DestroyImmediate(door.GetComponent<Collider>());

        // Camera pose used by the zoom-in/zoom-out choreography: in front of the door,
        // looking slightly down at it.
        var anchor = new GameObject("ZoomCameraAnchor");
        anchor.transform.SetParent(root.transform, false);
        anchor.transform.localPosition = new Vector3(0f, 2.8f, -10.5f);
        anchor.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);

        var locked = GameObject.CreatePrimitive(PrimitiveType.Cube);
        locked.name = "LockedVisual";
        locked.transform.SetParent(root.transform, false);
        locked.transform.localPosition = new Vector3(0f, 3.5f, 0f);
        locked.transform.localScale = new Vector3(7.3f, 7.3f, 7.3f);
        locked.GetComponent<MeshRenderer>().sharedMaterial = materials.Locked;
        Object.DestroyImmediate(locked.GetComponent<Collider>());

        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "CompletedMarker";
        marker.transform.SetParent(root.transform, false);
        marker.transform.localPosition = new Vector3(0f, 9.4f, 0f);
        marker.transform.localScale = Vector3.one * 1.4f;
        marker.GetComponent<MeshRenderer>().sharedMaterial = materials.Marker;
        Object.DestroyImmediate(marker.GetComponent<Collider>());
        marker.SetActive(false);

        // RequireComponent pulls OutlineObj (QuickOutline) onto the body automatically.
        var highlight = body.AddComponent<HiglightObject>();

        var storyBuilding = root.AddComponent<StoryBuilding>();
        SetRef(storyBuilding, "story", story);
        SetRef(storyBuilding, "zoomCameraAnchor", anchor.transform);
        SetRef(storyBuilding, "highlight", highlight);
        SetRef(storyBuilding, "lockedVisual", locked);
        SetRef(storyBuilding, "completedMarker", marker);
        SetRef(storyBuilding, "punchTarget", body.transform);
    }

    // ------------------------------------------------------------------ UI

    private static (StoryInfoPanel infoPanel, StoryIntroPanel introPanel, Button backButton) BuildUI()
    {
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // --- controls hint (bottom-right)
        var controlsHint = CreateText("ControlsHint", canvasGO.transform,
            "Drag kiri: putar  ·  Drag kanan: geser  ·  Scroll: zoom  ·  Esc: menu",
            16, FontStyles.Normal, TextAlignmentOptions.BottomRight);
        controlsHint.color = new Color(1f, 1f, 1f, 0.55f);
        SetRect(controlsHint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-24f, 18f), new Vector2(760f, 26f));

        // --- back button (top-left)
        var backButton = CreateButton("BackToMenuButton", canvasGO.transform, "< Menu");
        SetRect(((RectTransform)backButton.transform), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(24f, -24f), new Vector2(150f, 46f));

        // --- story info panel (bottom-left)
        var infoGO = CreateUIObject("StoryInfoPanel", canvasGO.transform);
        SetRect((RectTransform)infoGO.transform, Vector2.zero, Vector2.zero, Vector2.zero,
            new Vector2(40f, 40f), new Vector2(480f, 310f));
        var infoBG = infoGO.AddComponent<Image>();
        infoBG.color = new Color(0.04f, 0.04f, 0.06f, 0.86f);
        var infoGroup = infoGO.AddComponent<CanvasGroup>();
        infoGroup.alpha = 0f;
        infoGroup.interactable = false;
        infoGroup.blocksRaycasts = false;
        var infoPanel = infoGO.AddComponent<StoryInfoPanel>();

        var title = CreateText("TitleText", infoGO.transform, "Judul Cerita", 30, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        SetStretchTop(title.rectTransform, 40f, -16f);
        var status = CreateText("StatusText", infoGO.transform, "Status", 20, FontStyles.Italic, TextAlignmentOptions.TopLeft);
        status.color = new Color(0.95f, 0.82f, 0.45f);
        SetStretchTop(status.rectTransform, 26f, -60f);
        var description = CreateText("DescriptionText", infoGO.transform, "Deskripsi cerita.", 19, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        description.rectTransform.anchorMin = new Vector2(0f, 0f);
        description.rectTransform.anchorMax = new Vector2(1f, 1f);
        description.rectTransform.offsetMin = new Vector2(20f, 74f);
        description.rectTransform.offsetMax = new Vector2(-20f, -94f);

        var enterButton = CreateButton("EnterButton", infoGO.transform, "Masuki Cerita");
        SetRect((RectTransform)enterButton.transform, Vector2.zero, Vector2.zero, Vector2.zero,
            new Vector2(20f, 14f), new Vector2(208f, 46f));
        var replayButton = CreateButton("ReplayIntroButton", infoGO.transform, "Baca Ulang Narasi");
        SetRect((RectTransform)replayButton.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-20f, 14f), new Vector2(220f, 46f));

        SetRef(infoPanel, "canvasGroup", infoGroup);
        SetRef(infoPanel, "titleText", title);
        SetRef(infoPanel, "descriptionText", description);
        SetRef(infoPanel, "statusText", status);
        SetRef(infoPanel, "enterButton", enterButton);
        SetRef(infoPanel, "replayIntroButton", replayButton);

        // --- story intro panel (fullscreen, above everything; used for intro replay)
        var introPanel = BuildIntroPanel(canvasGO.transform);

        return (infoPanel, introPanel, backButton);
    }

    /// <summary>
    /// Builds a fullscreen StoryIntroPanel under the given canvas transform.
    /// Also used by GameplayStoryIntegrationBuilder to inject the panel into the
    /// "In Game UI" scene for the first-visit intro.
    /// </summary>
    public static StoryIntroPanel BuildIntroPanel(Transform canvasParent)
    {
        var introGO = CreateUIObject("StoryIntroPanel", canvasParent);
        var introRect = (RectTransform)introGO.transform;
        introRect.anchorMin = Vector2.zero;
        introRect.anchorMax = Vector2.one;
        introRect.offsetMin = Vector2.zero;
        introRect.offsetMax = Vector2.zero;
        var introBG = introGO.AddComponent<Image>();
        introBG.color = new Color(0f, 0f, 0f, 0.98f);
        var introGroup = introGO.AddComponent<CanvasGroup>();
        introGroup.alpha = 0f;
        introGroup.interactable = false;
        introGroup.blocksRaycasts = false;
        var typingSource = introGO.AddComponent<AudioSource>();
        typingSource.playOnAwake = false;
        var introPanel = introGO.AddComponent<StoryIntroPanel>();

        var narrative = CreateText("NarrativeText", introGO.transform, string.Empty, 30, FontStyles.Normal, TextAlignmentOptions.Center);
        narrative.rectTransform.anchorMin = Vector2.zero;
        narrative.rectTransform.anchorMax = Vector2.one;
        narrative.rectTransform.offsetMin = new Vector2(260f, 160f);
        narrative.rectTransform.offsetMax = new Vector2(-260f, -160f);
        var hint = CreateText("HintText", introGO.transform, "Klik untuk lanjut  ·  [Esc] Lewati", 18, FontStyles.Normal, TextAlignmentOptions.Center);
        hint.color = new Color(1f, 1f, 1f, 0.5f);
        SetRect(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 42f), new Vector2(900f, 30f));

        SetRef(introPanel, "canvasGroup", introGroup);
        SetRef(introPanel, "narrativeText", narrative);
        SetRef(introPanel, "hintText", hint);
        SetRef(introPanel, "typingAudioSource", typingSource);

        return introPanel;
    }

    private static WorldSelectionController BuildController(WorldOrbitCameraRig rig, Camera camera,
        StoryInfoPanel infoPanel, StoryIntroPanel introPanel)
    {
        var controllerGO = new GameObject("WorldSelection");
        var controller = controllerGO.AddComponent<WorldSelectionController>();
        controllerGO.AddComponent<SelectionInstaller>();

        SetRef(controller, "cameraRig", rig);
        SetRef(controller, "worldCamera", camera);
        SetRef(controller, "infoPanel", infoPanel);
        SetRef(controller, "introPanel", introPanel);
        return controller;
    }

    private static void BuildEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    // ------------------------------------------------------------------ helpers

    private static BlockoutMaterials CreateMaterials()
    {
        if (!AssetDatabase.IsValidFolder(MaterialsFolder))
        {
            AssetDatabase.CreateFolder("Assets/Scenes/Selection", "Materials");
        }

        return new BlockoutMaterials
        {
            Ground = GetOrCreateMaterial("WS_Ground", new Color(0.16f, 0.2f, 0.16f)),
            Building = GetOrCreateMaterial("WS_Building", new Color(0.65f, 0.58f, 0.48f)),
            Roof = GetOrCreateMaterial("WS_Roof", new Color(0.35f, 0.2f, 0.16f)),
            Door = GetOrCreateMaterial("WS_Door", new Color(0.2f, 0.13f, 0.09f)),
            Locked = GetOrCreateMaterial("WS_Locked", new Color(0.09f, 0.09f, 0.11f)),
            Marker = GetOrCreateMaterial("WS_CompletedMarker", new Color(1f, 0.85f, 0.4f), emissive: true),
        };
    }

    private static Material GetOrCreateMaterial(string name, Color color, bool emissive = false)
    {
        string path = $"{MaterialsFolder}/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetColor("_BaseColor", color);
        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.SetColor("_EmissionColor", color * 2.2f);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string content,
        float size, FontStyles style, TextAlignmentOptions alignment)
    {
        var go = CreateUIObject(name, parent);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label)
    {
        var go = CreateUIObject(name, parent);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.27f, 0.23f, 0.19f, 1f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var text = CreateText("Label", go.transform, label, 20, FontStyles.Normal, TextAlignmentOptions.Center);
        text.raycastTarget = false;
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    /// <summary>Stretch horizontally, pinned to the panel top with the given height/offset.</summary>
    private static void SetStretchTop(RectTransform rect, float height, float yOffset)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(-40f, height);
        rect.anchoredPosition = new Vector2(0f, yOffset);
    }

    private static void SetRef(Object target, string fieldName, Object value)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogError($"[WorldSelectionSceneBuilder] Field '{fieldName}' not found on {target.GetType().Name}.");
            return;
        }
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureSceneInBuildSettings()
    {
        if (EditorBuildSettings.scenes.Any(s => s.path == ScenePath))
        {
            return;
        }
        EditorBuildSettings.scenes = EditorBuildSettings.scenes
            .Append(new EditorBuildSettingsScene(ScenePath, true))
            .ToArray();
    }
}
