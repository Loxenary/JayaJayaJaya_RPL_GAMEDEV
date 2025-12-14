using UnityEngine;

/// <summary>
/// Interactable object that displays a hint message when interacted with.
/// Publishes HintShown event via EventBus.
/// </summary>
public class InteractableVisualHint : Interactable
{
    [Header("Hint Settings")]
    [SerializeField] private Sprite hintVisual;
    public override void InteractObject()
    {
        // base.InteractObject();
        onInteract?.Invoke();

        // Publish hint event via EventBus
        EventBus.Publish(new ImageShown()
        {
            imageSprite = hintVisual
        });
    }

}