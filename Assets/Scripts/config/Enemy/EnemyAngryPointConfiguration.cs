using UnityEngine;

[CreateAssetMenu(menuName = "Config/Enemy/Point Configuration")]
public class EnemyAngryPointConfiguration : ScriptableObject
{
    [Header("Game Parameters")]
    [Tooltip("Total number of puzzle items in the game")]
    [SerializeField] private int totalPuzzleItems = 5;

    [Header("Dynamic Calculation Weights")]
    [Tooltip("How much items contribute to anger (0-1). Higher = items matter more")]
    [SerializeField][Range(0f, 1f)] private float itemWeight = 0.6f;

    [Tooltip("How much sanity contributes to anger (0-1). Higher = sanity matters more")]
    [SerializeField][Range(0f, 1f)] private float sanityWeight = 0.4f;

    [Header("Exponential Curve Settings")]
    [Tooltip("Exponential power for item progression (1 = linear, 2 = quadratic, 3 = cubic)")]
    [SerializeField][Range(1f, 3f)] private float itemExponent = 1.5f;

    [Tooltip("Exponential power for sanity effect (1 = linear, 2 = quadratic, 3 = cubic)")]
    [SerializeField][Range(1f, 3f)] private float sanityExponent = 2f;

    [Header("Point Scaling")]
    [Tooltip("Maximum points achievable (when all items taken and sanity at 0)")]
    [SerializeField] private float maxAngryPoints = 100f;

    [Header("Time-Based Accumulation")]
    [Tooltip("Points added per second (multiplied by dynamic calculation)")]
    [SerializeField] private float basePointsPerSecond = 0.5f;

    // Public Properties
    public int TotalPuzzleItems => totalPuzzleItems;
    public float ItemWeight => itemWeight;
    public float SanityWeight => sanityWeight;
    public float ItemExponent => itemExponent;
    public float SanityExponent => sanityExponent;
    public float MaxAngryPoints => maxAngryPoints;
    public float BasePointsPerSecond => basePointsPerSecond;

    /// <summary>
    /// Calculate dynamic angry points based on current game state
    /// Formula: AngryPoints = MaxPoints * (ItemWeight * ItemFactor^itemExp + SanityWeight * SanityFactor^sanityExp)
    ///
    /// ItemFactor = itemsTaken / totalItems (0-1)
    /// SanityFactor = (maxSanity - currentSanity) / maxSanity (0-1, inverted so low sanity = high factor)
    /// </summary>
    /// <param name="itemsTaken">Number of items collected</param>
    /// <param name="currentSanity">Current sanity value</param>
    /// <param name="maxSanity">Maximum sanity value (from PlayerAttributes)</param>
    public float CalculateAngryPoints(int itemsTaken, float currentSanity, float maxSanity)
    {
        // Normalize item progress (0-1)
        float itemFactor = totalPuzzleItems > 0 ? Mathf.Clamp01((float)itemsTaken / totalPuzzleItems) : 0f;

        // Normalize sanity (0-1, inverted - low sanity = high anger)
        float sanityFactor = maxSanity > 0 ? Mathf.Clamp01((maxSanity - currentSanity) / maxSanity) : 0f;

        // Apply exponential curves
        float itemContribution = Mathf.Pow(itemFactor, itemExponent);
        float sanityContribution = Mathf.Pow(sanityFactor, sanityExponent);

        // Weighted combination
        float normalizedPoints = (itemWeight * itemContribution) + (sanityWeight * sanityContribution);

        // Scale to max points
        return normalizedPoints * maxAngryPoints;
    }

    /// <summary>
    /// Calculate points per second based on current state
    /// More aggressive when both items are taken and sanity is low
    /// </summary>
    /// <param name="itemsTaken">Number of items collected</param>
    /// <param name="currentSanity">Current sanity value</param>
    /// <param name="maxSanity">Maximum sanity value (from PlayerAttributes)</param>
    public float CalculatePointsPerSecond(int itemsTaken, float currentSanity, float maxSanity)
    {
        float basePoints = CalculateAngryPoints(itemsTaken, currentSanity, maxSanity);

        // The more angry the base calculation, the faster points accumulate
        float multiplier = Mathf.Clamp01(basePoints / maxAngryPoints);

        return basePointsPerSecond * multiplier;
    }

    private void OnValidate()
    {
        // Ensure weights sum to reasonable values
        if (itemWeight + sanityWeight > 2f)
        {
            Debug.LogWarning($"[EnemyAngryPointConfiguration] Item weight ({itemWeight}) + Sanity weight ({sanityWeight}) exceeds 2.0. Consider balancing.", this);
        }

        if (totalPuzzleItems <= 0)
        {
            Debug.LogError("[EnemyAngryPointConfiguration] Total puzzle items must be greater than 0!", this);
        }
    }
}