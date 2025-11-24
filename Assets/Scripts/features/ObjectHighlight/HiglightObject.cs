using CustomLogger;
using UnityEngine;

[RequireComponent(typeof(OutlineObj))]
public class HiglightObject : MonoBehaviour, IHighlight
{
    private OutlineObj _outline;

    private void Awake()
    {
        _outline = GetComponent<OutlineObj>();
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