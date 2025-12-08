using UnityEngine;

[CreateAssetMenu(menuName = "Config/Enemy/Point Configuration")]
public class EnemyAngryPointConfiguration : ScriptableObject
{
    [Tooltip("Points added per item taken by player")]
    [SerializeField] private float pointsPerItemTaken = 10f;

    [Tooltip("Points added per second when player sanity is below threshold")]
    [SerializeField] private float pointsPerSecondLowSanity = 2f;

    [Tooltip("Sanity threshold (0-1) below which anger points accumulate")]
    [SerializeField][Range(0f, 1f)] private float lowSanityThreshold = 0.3f;

    [Tooltip("Points added per second when timer is below threshold")]
    [SerializeField] private float pointsPerSecondLowTimer = 1.5f;

    [Tooltip("Timer threshold (in seconds) below which anger points accumulate")]
    [SerializeField] private float lowTimerThreshold = 60f;

    public float PointsPerItemTaken => pointsPerItemTaken;
    public float PointsPerPerSecondLowSanity => pointsPerSecondLowSanity;
    public float LowSanityThreshold => lowSanityThreshold;
    public float PointsPerSecondLowTimer => pointsPerSecondLowTimer;
    public float LowTimerThreshold => lowTimerThreshold;
}