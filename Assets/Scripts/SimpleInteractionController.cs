using UnityEngine;

public class SimpleInteractionController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 3f))
            {
                // Cek pintu
                SimpleDoor door = hit.collider.GetComponentInParent<SimpleDoor>();
                if (door != null)
                {
                    door.TryOpenDoor(this.gameObject);
                    return;
                }

                // Cek item kunci
                KeyItem key = hit.collider.GetComponentInParent<KeyItem>();
                if (key != null)
                {
                    key.TakeKey(this.gameObject);
                    return;
                }
            }
        }
    }
}
