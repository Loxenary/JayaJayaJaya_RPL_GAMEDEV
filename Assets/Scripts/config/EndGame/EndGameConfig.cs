using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/EndGameConfig")]
public class EndGameConfig : ScriptableObject
{
    [Serializable]
    public class EndGameRecord
    {
        [TextArea(3, 10)]
        public string Area;
        public int collectibleCount;
    }

    [SerializeField] private EndGameRecord[] endGames;

    public EndGameRecord[] EndGames => endGames;

    public int[] CollectibleCounts => endGames.Select(x => x.collectibleCount).OrderBy(x => x).ToArray();

    public EndGameRecord GetNearestEndGame(int currentPuzzleCount)
    {
        // Find the highest collectible count that is less than or equal to currentPuzzleCount
        var validEndGames = endGames
            .Where(x => x.collectibleCount <= currentPuzzleCount)
            .OrderByDescending(x => x.collectibleCount)
            .ToArray();

        // If there are valid endgames, return the one with the highest collectible count that's <= currentPuzzleCount
        // Otherwise, return the one with the lowest collectible count
        if (validEndGames.Length > 0)
        {
            return validEndGames.First();
        }
        else
        {
            return endGames.OrderBy(x => x.collectibleCount).First();
        }
    }
    
}