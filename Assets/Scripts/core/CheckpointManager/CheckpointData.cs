using System;
using UnityEngine;

/// <summary>
/// Data class yang merepresentasikan informasi sebuah checkpoint
/// </summary>
[Serializable]
public class CheckpointData
{
    public string checkpointID;
    public Vector3 position;
    public Quaternion rotation;
    public string sceneName;
    public bool isActivated;
    public DateTime activationTime;

    public CheckpointData()
    {
    }

    public CheckpointData(string id, Vector3 pos, Quaternion rot, string scene)
    {
        checkpointID = id;
        position = pos;
        rotation = rot;
        sceneName = scene;
        isActivated = false;
    }
}
