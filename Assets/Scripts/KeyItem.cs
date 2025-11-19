using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public string keyID = "Door_Lab"; // nanti bisa ubah di Inspector

    public void TakeKey(GameObject player)
    {
        player.GetComponent<PlayerKeyring>().AddKey(keyID);
        Destroy(gameObject);
    }
}
