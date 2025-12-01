using UnityEngine;

public class AddCollectibles : MonoBehaviour
{
    [SerializeField] CollectibleType type;


    public void AddCollectible()
    {
        EventBus.Publish(type);
    }
}
