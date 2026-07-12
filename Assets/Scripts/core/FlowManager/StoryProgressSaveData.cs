using System;
using System.Collections.Generic;

/// <summary>
/// Save data untuk progression antar story.
/// Menyimpan status per-story: sudah lihat intro, sudah tamat, dan ending yang diraih.
/// Unlock story berikutnya diturunkan dari flag 'completed' story sebelumnya
/// (urutan diambil dari StoryDatabase), jadi tidak perlu disimpan eksplisit.
/// </summary>
[Serializable]
public class StoryProgressEntry
{
    public string storyId;
    public bool hasSeenIntro;
    public bool completed;
    public string endingId;
}

[Serializable]
public class StoryProgressSaveData : BaseSaveData
{
    public List<StoryProgressEntry> entries = new();

    public StoryProgressSaveData() : base("StoryProgress.json")
    {
    }

    public StoryProgressEntry GetEntry(string storyId)
    {
        return entries.Find(e => e.storyId == storyId);
    }

    public StoryProgressEntry GetOrCreate(string storyId)
    {
        var entry = GetEntry(storyId);
        if (entry == null)
        {
            entry = new StoryProgressEntry { storyId = storyId };
            entries.Add(entry);
        }
        return entry;
    }
}
