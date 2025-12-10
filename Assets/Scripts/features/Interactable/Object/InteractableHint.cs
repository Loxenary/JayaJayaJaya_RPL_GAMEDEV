using UnityEngine;

/// <summary>
/// Interactable object that displays a hint message when interacted with.
/// Publishes HintShown event via EventBus.
/// </summary>
public class InteractableHint : Interactable
{
    [Header("Hint Settings")]
    [TextArea(2, 5)]
    [SerializeField] private string hintText = "This is a hint message";

    [Tooltip("How long to display the hint in seconds (0 = use default from UI)")]
    [SerializeField] private float displayDuration = 3f;

    public override void InteractObject()
    {
        base.InteractObject();

        // Publish hint event via EventBus
        EventBus.Publish(new HintShown
        {
            hintText = hintText,
            displayDuration = displayDuration
        });
    }

    public string HintText => hintText;
    public float DisplayDuration => displayDuration;
}
