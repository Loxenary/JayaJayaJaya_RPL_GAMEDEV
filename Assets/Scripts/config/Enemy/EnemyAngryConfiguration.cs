using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAngryStats", menuName = "Config/Enemy/Angry Stats Configuration")]
public class EnemyAngryConfiguration : ScriptableObject
{
    [SerializeField] private EnemyConfigurationRecord[] enemyRecords;

    /// <summary>
    /// Get configurations sorted by angry point baseline (lowest to highest)
    /// </summary>
    public EnemyConfigurationRecord[] GetSortedConfigurations()
    {
        if (enemyRecords == null || enemyRecords.Length == 0)
        {
            Debug.LogError("[EnemyAngryConfiguration] No enemy records to sort!", this);
            return new EnemyConfigurationRecord[0];
        }

        return enemyRecords.OrderBy(x => x.angryPointBaseline).ToArray();
    }

    /// <summary>
    /// Get configuration for a specific level
    /// </summary>
    public EnemyConfigurationRecord GetConfigurationForLevel(EnemyLevel level)
    {
        if (enemyRecords == null) return null;

        return enemyRecords.FirstOrDefault(x => x.level == level);
    }

    /// <summary>
    /// Get the number of configured levels
    /// </summary>
    public int GetLevelCount()
    {
        return enemyRecords?.Length ?? 0;
    }

    private void OnValidate()
    {
        // Nullity Check
        if (enemyRecords == null)
        {
            Debug.LogError("[EnemyAngryConfiguration] No enemy records found!", this);
            return;
        }
        else
        {

            foreach (EnemyConfigurationRecord enemyRecord in enemyRecords)
            {
                if (enemyRecord.enemyStats == null)
                {
                    Debug.LogError($"[EnemyAngryConfiguration] No enemy stats found for {enemyRecord.level}!", this);
                }
            }
        }


        // Check if no duplicate level
        var duplicateRecords = enemyRecords.GroupBy(x => x.level).Where(x => x.Count() > 1).ToArray();
        if (duplicateRecords.Length > 0)
        {
            Debug.LogError($"[EnemyAngryConfiguration] Duplicate levels found: {string.Join(", ", duplicateRecords.Select(x => x.Key))}", this);
            return;
        }

        // Check Angry Point Baseline Logic
        var sortedRecords = enemyRecords.OrderBy(x => x.angryPointBaseline).ToArray();

        var brokenAngryPoints = sortedRecords.Skip(1).Where(x => x.angryPointBaseline < sortedRecords[0].angryPointBaseline).ToArray();
        if (brokenAngryPoints.Length > 0)
        {
            Debug.LogError($"[EnemyAngryConfiguration] Broken angry point baselines found: {string.Join(", ", brokenAngryPoints.Select(x => x.level))}", this);
            return;
        }
    }
}

