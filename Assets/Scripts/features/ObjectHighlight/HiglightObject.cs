using CustomLogger;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class HiglightObject : MonoBehaviour, IHighlight
{
    private Outline _outline;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }
    public void Highlight()
    {
        _outline.enabled = true;
    }

    public void UnHighlight()
    {
        _outline.enabled = false;
    }
}