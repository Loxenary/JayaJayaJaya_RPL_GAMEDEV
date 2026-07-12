using UnityEngine;

/// <summary>
/// Data definition for a single story (one building in the world selection map).
/// The unlock order is defined by the ordering inside <see cref="StoryDatabase"/>.
/// </summary>
[CreateAssetMenu(menuName = "Config/Story/Story Definition")]
public class StoryDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique and stable id. Used as the save-data key, so never change it after release.")]
    [SerializeField] private string storyId;
    [SerializeField] private string title;
    [SerializeField, TextArea(3, 6)] private string description;

    [Header("Narrative Intro")]
    [Tooltip("Shown once on the first visit (and replayable from the world map). Leave empty for no intro.")]
    [SerializeField, TextArea(5, 10)] private string[] introParagraphs;

    [Header("Gameplay")]
    [Tooltip("Scene group loaded when this story is entered. Leave null for a 'coming soon' placeholder building.")]
    [SerializeField] private SceneGroup gameplayGroup;

    public string StoryId => storyId;
    public string Title => title;
    public string Description => description;
    public string[] IntroParagraphs => introParagraphs;
    public SceneGroup GameplayGroup => gameplayGroup;

    /// <summary>A story without a gameplay group is a placeholder and cannot be entered.</summary>
    public bool IsPlayable => gameplayGroup != null;
}
