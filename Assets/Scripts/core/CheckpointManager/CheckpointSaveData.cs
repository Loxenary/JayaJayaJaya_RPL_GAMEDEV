using System;
using UnityEngine;

/// <summary>
/// Save data untuk sistem checkpoint
/// Menyimpan informasi checkpoint terakhir yang diaktivasi player
/// </summary>
[Serializable]
public class CheckpointSaveData : BaseSaveData
{
    public string lastCheckpointID;
    public Vector3 lastPosition;
    public Quaternion lastRotation;
    public string lastSceneName;
    public DateTime lastActivationTime;

    public CheckpointSaveData() : base("CheckpointData.json")
    {
    }
}
