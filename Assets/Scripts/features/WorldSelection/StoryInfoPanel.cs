using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Info panel on the world selection map showing the hovered building's story:
/// title, description, status (locked / not played / completed + ending),
/// with buttons to enter the story or replay its intro narrative.
/// </summary>
public class StoryInfoPanel : FadeShowHideProcedural
{
    [Header("Story Info Section")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button enterButton;
    [SerializeField] private Button replayIntroButton;

    private StoryBuilding _current;
    private Action<StoryBuilding> _onEnter;
    private Action<StoryDefinition> _onReplayIntro;

    public StoryBuilding CurrentBuilding => _current;

    /// <summary>Wires the panel's buttons. Called once by WorldSelectionController.</summary>
    public void Init(Action<StoryBuilding> onEnter, Action<StoryDefinition> onReplayIntro)
    {
        _onEnter = onEnter;
        _onReplayIntro = onReplayIntro;

        if (enterButton != null)
        {
            enterButton.onClick.AddListener(() =>
            {
                if (_current != null)
                {
                    _onEnter?.Invoke(_current);
                }
            });
        }

        if (replayIntroButton != null)
        {
            replayIntroButton.onClick.AddListener(() =>
            {
                if (_current != null)
                {
                    _onReplayIntro?.Invoke(_current.Story);
                }
            });
        }
    }

    public void Show(StoryBuilding building, StoryProgressEntry progress)
    {
        if (building == null || building.Story == null)
        {
            return;
        }

        _current = building;
        var story = building.Story;

        if (titleText != null)
        {
            titleText.text = building.IsUnlocked ? story.Title : "???";
        }
        if (descriptionText != null)
        {
            descriptionText.text = building.IsUnlocked
                ? story.Description
                : "Selesaikan cerita sebelumnya untuk membukanya.";
        }
        if (statusText != null)
        {
            statusText.text = BuildStatusText(building, progress);
        }

        bool hasIntro = story.IntroParagraphs != null && story.IntroParagraphs.Length > 0;
        if (enterButton != null)
        {
            enterButton.gameObject.SetActive(building.IsEnterable);
        }
        if (replayIntroButton != null)
        {
            replayIntroButton.gameObject.SetActive(
                building.IsEnterable && hasIntro && progress != null && progress.hasSeenIntro);
        }

        ShowUI();
    }

    public void Hide()
    {
        _current = null;
        HideUI();
    }

    private static string BuildStatusText(StoryBuilding building, StoryProgressEntry progress)
    {
        if (!building.IsUnlocked)
        {
            return "Terkunci";
        }
        if (!building.IsEnterable)
        {
            return "Segera hadir";
        }
        if (progress != null && progress.completed)
        {
            return string.IsNullOrEmpty(progress.endingId)
                ? "Selesai"
                : $"Selesai — Ending: {progress.endingId}";
        }
        return "Belum dimainkan";
    }
}
