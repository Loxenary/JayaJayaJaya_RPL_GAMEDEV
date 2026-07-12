using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ordered collection of all stories. The list order defines the sequential
/// unlock order: story N+1 unlocks when story N is completed (any ending).
/// </summary>
[CreateAssetMenu(menuName = "Config/Story/Story Database")]
public class StoryDatabase : ScriptableObject
{
    [Tooltip("Order in this list = unlock order.")]
    [SerializeField] private List<StoryDefinition> storiesInOrder = new();

    public IReadOnlyList<StoryDefinition> StoriesInOrder => storiesInOrder;

    public StoryDefinition GetById(string storyId)
    {
        return storiesInOrder.Find(s => s != null && s.StoryId == storyId);
    }

    public int IndexOf(StoryDefinition story)
    {
        return storiesInOrder.IndexOf(story);
    }

    /// <summary>Returns the story unlocked by completing the given one, or null if it is the last.</summary>
    public StoryDefinition GetNext(StoryDefinition story)
    {
        int index = storiesInOrder.IndexOf(story);
        if (index < 0 || index + 1 >= storiesInOrder.Count)
        {
            return null;
        }
        return storiesInOrder[index + 1];
    }
}
