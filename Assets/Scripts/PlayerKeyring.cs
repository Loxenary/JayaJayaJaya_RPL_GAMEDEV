using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyring : MonoBehaviour
{
    private HashSet<string> keys = new HashSet<string>();

    public void AddKey(string keyID)
    {
        if (!keys.Contains(keyID))
        {
            keys.Add(keyID);
            Debug.Log("Kunci diambil: " + keyID);
        }
    }

    public bool HasKey(string keyID)
    {
        return keys.Contains(keyID);
    }
}
