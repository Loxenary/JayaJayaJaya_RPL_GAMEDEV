
using System;
using EnemyAI;
using UnityEngine;

[Serializable]
public class EnemyConfigurationRecord
{
    [Tooltip("Enemy Stats")]
    public EnemyStats enemyStats;

    [Tooltip("The Level identifier")]
    public EnemyLevel level;
    public SfxClipData enemyIncreaseSfx;

    [Tooltip("The amount of angry points to reach this level")]
    public float angryPointBaseline = 0;
}