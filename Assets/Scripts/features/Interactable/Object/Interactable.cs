using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for interactable objects. Uses raycast detection via child InteractableZone (collider).
/// </summary>
public class Interactable : MonoBehaviour
{
  [Header("Event")]
  public UnityEvent onInteract;

#if UNITY_EDITOR

  [Header("Debugging Section")]
  [ReadOnly]
  [SerializeField] private bool _isInteract => isInteract;

#endif

  protected bool isInteract;

  // Child InteractableZone handles collider + layer. No setup needed here.

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
