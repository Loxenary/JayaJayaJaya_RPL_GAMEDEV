using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    public string doorID = "Door_Lab";

    public GameObject doorClosed;
    public GameObject doorOpenLeft;
    public GameObject doorOpenRight;

    private bool isOpen = false;
    public bool isLocked = true;

    public void TryOpenDoor(GameObject player)
    {
        if (isOpen) return;

        var keyring = player.GetComponent<PlayerKeyring>();

        if (isLocked)
        {
            if (keyring.HasKey(doorID))
            {
                UnlockAndOpen();
            }
            else
            {
                Debug.Log("Pintu terkunci! Butuh kunci: " + doorID);
            }
        }
        else
        {
            OpenDoor();
        }
    }

    private void UnlockAndOpen()
    {
        isLocked = false;
        OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;

        doorClosed.SetActive(false);
        doorOpenLeft.SetActive(true);
        doorOpenRight.SetActive(true);

        Debug.Log("Pintu terbuka: " + doorID);
    }
}
