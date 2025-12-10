using UnityEngine;

public class HiglightObjectPlus : HiglightObject
{
 
    [SerializeField] private bool isDiable;

    public override void Highlight()
    {
        if (isDiable) return;
        base.Highlight();
    }
    public override void UnHighlight()
    {
        if (isDiable) return;
        base.UnHighlight();
    }
    public void SetDisable(bool disable = true)
    {
        isDiable= disable;
    }
}
