using CustomLogger;
using UnityEngine;

[RequireComponent(typeof(OutlineObj))]
public class HiglightObject : MonoBehaviour, IHighlight
{
    [SerializeField] bool firstState;
    private OutlineObj _outline;

    private void Awake()
    {
        _outline = GetComponent<OutlineObj>();

        _outline.enabled = firstState;
    }
    public virtual void Highlight()
    {
        _outline.enabled = true;
    }

    public virtual void UnHighlight()
    {
        _outline.enabled = false;
    }
}