using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-click setup for the story-flow pieces that live inside the existing
/// gameplay scenes. Menu: Tools > World Selection > Setup Gameplay Scenes.
///
/// - "In Game UI": adds a fullscreen StoryIntroPanel + StoryIntroController
///   (first-visit narrative intro).
/// - "In Game": adds a StoryExitTrigger next to the InteractableEndGameDoor.
///   The trigger object is created INACTIVE on purpose: its exact position
///   (just outside the door threshold) must be verified by hand in the scene
///   view before activating it, otherwise it could fire at spawn.
///
/// Idempotent: skips anything that already exists.
/// </summary>
public static class GameplayStoryIntegrationBuilder
{
    private const string InGameUIScenePath = "Assets/Scenes/InGame/In Game UI.unity";
    private const string InGameScenePath = "Assets/Scenes/InGame/In Game.unity";

    [MenuItem("Tools/World Selection/Setup Gameplay Scenes (Intro + Exit Trigger)")]
    public static void Setup()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        SetupIntroInGameUI();
        SetupExitTriggerInGame();
    }

    private static void SetupIntroInGameUI()
    {
        var scene = EditorSceneManager.OpenScene(InGameUIScenePath, OpenSceneMode.Single);

        if (Object.FindFirstObjectByType<StoryIntroController>(FindObjectsInactive.Include) != null)
        {
            Debug.Log("[GameplayStoryIntegrationBuilder] 'In Game UI' already has a StoryIntroController - skipped.");
            return;
        }

        var canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(c => c.isRootCanvas);
        if (canvas == null)
        {
            Debug.LogError("[GameplayStoryIntegrationBuilder] No root Canvas found in 'In Game UI' - intro panel not added.");
            return;
        }

        var introPanel = WorldSelectionSceneBuilder.BuildIntroPanel(canvas.transform);
        // Last sibling: the intro must draw above the HUD.
        introPanel.transform.SetAsLastSibling();

        var controllerGO = new GameObject("StoryIntroController");
        var controller = controllerGO.AddComponent<StoryIntroController>();
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("introPanel").objectReferenceValue = introPanel;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameplayStoryIntegrationBuilder] StoryIntroPanel + StoryIntroController added to 'In Game UI'.");
    }

    private static void SetupExitTriggerInGame()
    {
        var scene = EditorSceneManager.OpenScene(InGameScenePath, OpenSceneMode.Single);

        if (Object.FindFirstObjectByType<StoryExitTrigger>(FindObjectsInactive.Include) != null)
        {
            Debug.Log("[GameplayStoryIntegrationBuilder] 'In Game' already has a StoryExitTrigger - skipped.");
            return;
        }

        var door = Object.FindFirstObjectByType<InteractableEndGameDoor>(FindObjectsInactive.Include);

        var triggerGO = new GameObject("StoryExitTrigger");
        var collider = triggerGO.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(3f, 3f, 2f);

        if (door != null)
        {
            // Rough guess just outside the door; MUST be verified/adjusted by hand.
            triggerGO.transform.SetPositionAndRotation(
                door.transform.position + door.transform.forward * 2.5f + Vector3.up * 1.5f,
                door.transform.rotation);
        }

        var trigger = triggerGO.AddComponent<StoryExitTrigger>();
        var serialized = new SerializedObject(trigger);
        serialized.FindProperty("exitDoor").objectReferenceValue = door;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        // Inactive on purpose - see class summary.
        triggerGO.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.LogWarning(
            "[GameplayStoryIntegrationBuilder] StoryExitTrigger added to 'In Game' but left INACTIVE. " +
            "Position it just OUTSIDE the exit door threshold (so it cannot touch the player's spawn), " +
            "then activate the GameObject.");
    }
}
