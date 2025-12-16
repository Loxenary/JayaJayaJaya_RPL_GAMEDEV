using UnityEngine;

public class Debugging_Event : MonoBehaviour
{
    public void CallMe()
    {
        Debug.Log("I AM Called With "+gameObject.name);
    }
}
