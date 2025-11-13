using System;
using UnityEngine;

[Serializable]
public class BaseSaveData
{
    public virtual string FileName { get; protected set; }

    private Error _error;

    public Error ErrorProperty
    {
        get { return _error; }
        set { _error = value; }
    }

    public bool IsError()
    {
        return _error != null && _error.MessageCode != ErrorCode.Success;
    }

    // This Resetable is used for the Reset by File from the SaveLoadManager Reset function
    public virtual bool IsResetable => true;
    public BaseSaveData(string fileName)
    {
        FileName = fileName;
    }
}