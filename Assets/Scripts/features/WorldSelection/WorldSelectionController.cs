using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Orchestrates the world selection map: hover/click on buildings (physics
/// raycast, guarded against UI), the zoom-in choreography when entering a story,
/// the zoom-out when arriving back from one, and the intro replay.
/// </summary>
public class WorldSelectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorldOrbitCameraRig cameraRig;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private StoryInfoPanel infoPanel;
    [Tooltip("Optional: panel used to replay a story's intro from the map.")]
    [SerializeField] private StoryIntroPanel introPanel;

    [Header("Selection")]
    [SerializeField] private LayerMask buildingLayerMask = ~0;
    [SerializeField] private float rayDistance = 500f;
    [Tooltip("Mouse movement (pixels) below which a press+release counts as a click instead of an orbit drag.")]
    [SerializeField] private float clickDragThreshold = 5f;

    [Header("Zoom Choreography")]
    [SerializeField] private float zoomInDuration = 1.2f;
    [SerializeField] private float zoomOutDuration = 1.6f;

    private StoryBuilding[] _buildings = new StoryBuilding[0];
    private StoryBuilding _hovered;
    private Vector2 _pressPosition;
    private bool _pressStartedOverUI;
    private bool _isLeavingScene;
    private bool _isReplayingIntro;
    private FlowManager _flowManager;

    private void Start()
    {
        _flowManager = ServiceLocator.Get<FlowManager>();

        // Gameplay locks the cursor; the map needs it visible.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        _buildings = FindObjectsByType<StoryBuilding>(FindObjectsSortMode.None);
        RefreshBuildingStates();

        if (infoPanel != null)
        {
            infoPanel.Init(EnterStory, ReplayIntro);
        }

        HandleZoomOutEntry();
    }

    private void Update()
    {
        if (_isLeavingScene || _isReplayingIntro)
        {
            return;
        }

        HandleHover();
        HandleClick();
        HandleKeyboard();
    }

    /// <summary>Also callable from a UI back button.</summary>
    public async void BackToMenu()
    {
        if (_isLeavingScene)
        {
            return;
        }
        _isLeavingScene = true;
        await ServiceLocator.Get<SceneService>().LoadScene(SceneEnum.MAIN_MENU, true);
    }

    // ---------------------------------------------------------------------

    private void HandleZoomOutEntry()
    {
        var pendingStory = _flowManager != null ? _flowManager.PendingZoomOutStory : null;
        if (pendingStory == null)
        {
            return;
        }
        _flowManager.ClearPendingZoomOut();

        var building = FindBuilding(pendingStory);
        if (building == null || cameraRig == null)
        {
            return;
        }

        cameraRig.ZoomOutFrom(building.ZoomAnchor, zoomOutDuration);
    }

    private void HandleHover()
    {
        var mouse = Mouse.current;
        if (mouse == null || worldCamera == null)
        {
            return;
        }

        StoryBuilding hit = null;
        if (!IsPointerOverUI())
        {
            Ray ray = worldCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hitInfo, rayDistance, buildingLayerMask))
            {
                hit = hitInfo.collider.GetComponentInParent<StoryBuilding>();
            }
        }

        if (hit == _hovered)
        {
            return;
        }

        _hovered?.SetHovered(false);
        _hovered = hit;
        _hovered?.SetHovered(true);

        if (_hovered != null && infoPanel != null)
        {
            infoPanel.Show(_hovered, GetProgressSafe(_hovered));
        }
    }

    private void HandleClick()
    {
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _pressPosition = mouse.position.ReadValue();
            _pressStartedOverUI = IsPointerOverUI();
        }

        if (!mouse.leftButton.wasReleasedThisFrame || _pressStartedOverUI)
        {
            return;
        }
        if (Vector2.Distance(_pressPosition, mouse.position.ReadValue()) > clickDragThreshold)
        {
            return; // that was an orbit drag, not a click
        }
        if (IsPointerOverUI() || _hovered == null)
        {
            return;
        }

        if (_hovered.IsEnterable)
        {
            EnterStory(_hovered);
        }
        else
        {
            _hovered.PlayLockedFeedback();
            if (infoPanel != null)
            {
                infoPanel.Show(_hovered, GetProgressSafe(_hovered));
            }
        }
    }

    private void HandleKeyboard()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            BackToMenu();
        }
    }

    private void EnterStory(StoryBuilding building)
    {
        if (_isLeavingScene || building == null || !building.IsEnterable || _flowManager == null)
        {
            return;
        }
        _isLeavingScene = true;

        _hovered?.SetHovered(false);
        _hovered = null;
        infoPanel?.Hide();

        var story = building.Story;
        if (cameraRig != null)
        {
            // Zoom toward the building first; the scene transition fade takes over on complete.
            cameraRig.ZoomTo(building.ZoomAnchor, zoomInDuration)
                .OnComplete(() => _ = _flowManager.PlayStory(story));
        }
        else
        {
            _ = _flowManager.PlayStory(story);
        }
    }

    private void ReplayIntro(StoryDefinition story)
    {
        if (introPanel == null || story == null ||
            story.IntroParagraphs == null || story.IntroParagraphs.Length == 0)
        {
            return;
        }

        _isReplayingIntro = true;
        if (cameraRig != null)
        {
            cameraRig.InputEnabled = false;
        }

        introPanel.Play(story.IntroParagraphs, onDone: () =>
        {
            _isReplayingIntro = false;
            if (cameraRig != null && !cameraRig.IsPoseOverridden)
            {
                cameraRig.InputEnabled = true;
            }
        });
    }

    private void RefreshBuildingStates()
    {
        foreach (var building in _buildings)
        {
            bool unlocked = _flowManager != null && _flowManager.IsStoryUnlocked(building.Story);
            building.RefreshState(unlocked, GetProgressSafe(building));
        }
    }

    private StoryProgressEntry GetProgressSafe(StoryBuilding building)
    {
        if (_flowManager == null || building == null || building.Story == null)
        {
            return null;
        }
        return _flowManager.GetProgress(building.Story);
    }

    private StoryBuilding FindBuilding(StoryDefinition story)
    {
        foreach (var building in _buildings)
        {
            if (building.Story == story)
            {
                return building;
            }
        }
        return null;
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
