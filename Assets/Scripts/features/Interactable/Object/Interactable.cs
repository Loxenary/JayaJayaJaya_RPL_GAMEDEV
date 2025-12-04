using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for interactable objects. Uses raycast detection (no trigger required).
/// Collider is needed for raycast hit detection.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Header("Event")]
    public UnityEvent onInteract;

    [Header("Debugging Section")]
    [ReadOnly]
    [SerializeField] protected bool isInteract;
    [ReadOnly]
    [SerializeField] protected new Collider collider;

    private void OnValidate()
    {
        SetupInteractable();
    }

    private void Awake()
    {
        SetupInteractable();
    }

    private void SetupInteractable()
    {
        // Set layer to Interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // Get collider reference
        if (collider == null)
        {
            collider = GetComponent<Collider>();
        }

        // Collider should NOT be trigger for raycast detection
        // If you need trigger for other purposes, use a separate collider
        if (collider != null && collider.isTrigger)
        {
            Debug.LogWarning($"[Interactable] {gameObject.name}: Collider is set as trigger. " +
                "Raycast requires non-trigger collider. Consider using a separate collider for triggers.", this);
        }
    }

    public virtual void InteractObject()
    {
        if (isInteract)
            return;

        isInteract = true;
        onInteract?.Invoke();
    }

    /// <summary>
    /// Reset interaction state (useful for reusable objects)
    /// </summary>
    public virtual void ResetInteraction()
    {
        isInteract = false;
    }

    /// <summary>
    /// Check if object has already been interacted with
    /// </summary>
    public bool HasBeenInteracted => isInteract;
}
