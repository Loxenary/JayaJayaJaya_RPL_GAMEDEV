/// <summary>
/// Published by EndGameListener when the player reaches an ending zone.
/// Any ending counts as completing the current story (deaths do not publish this).
/// FlowManager subscribes and records the completion into StoryProgressSaveData.
/// </summary>
public struct StoryCompletedEvent
{
    public string endingId;
}
