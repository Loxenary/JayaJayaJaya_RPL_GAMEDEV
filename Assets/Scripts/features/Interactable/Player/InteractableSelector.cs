using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Raycast-based interaction system. Player can only interact with objects they're looking at.
/// </summary>
[DisallowMultipleComponent]
public class InteractableSelector : MonoBehaviour
{
  [Header("Raycast Settings")]
  [SerializeField] private Transform raycastOrigin; // Assign camera or head transform
  [SerializeField] private float interactDistance = 3f;
  [SerializeField] private LayerMask targetLayer = -1;
  [SerializeField] private float raycastBackOffset = 0.2f;
  [SerializeField] private LayerMask blockingMask = ~0; // Layers that can block interaction (player layer removed at runtime)

  private Interactable currentObject;

  private InputSystem_Actions input;
  private Camera playerCamera;
  private Interactable previousObject;

  private void Awake()
  {
    input = new InputSystem_Actions();
    input.Player.Interact.performed += OnPressInteract;

    // Remove player layer from blocker mask so we don't block ourselves
    int playerLayer = LayerMask.NameToLayer("Player");
    if (playerLayer >= 0)
    {
      blockingMask &= ~(1 << playerLayer);
    }

    // Try to find camera if raycastOrigin not assigned
    if (raycastOrigin == null)
    {
      playerCamera = Camera.main;
      if (playerCamera != null)
      {
        raycastOrigin = playerCamera.transform;
      }
    }
  }

  private void OnEnable()
  {
    input.Enable();
  }

  private void OnDisable()
  {
    input.Disable();
  }

  private void Update()
  {
    CheckForInteractable();
  }

  /// <summary>
  /// Raycast from camera to detect interactable objects player is looking at
  /// </summary>
  private void CheckForInteractable()
  {
    if (raycastOrigin == null) return;

    Vector3 origin = raycastOrigin.position - raycastOrigin.forward * raycastBackOffset;
    Ray ray = new Ray(origin, raycastOrigin.forward);
    RaycastHit hit;
    if (!Physics.Raycast(ray, out hit, interactDistance, blockingMask, QueryTriggerInteraction.Collide))
    {
      ClearCurrentObject();
      return;
    }

    // Nearest hit decides: if it's not an interactable, interaction is blocked
    Interactable interactableHit = hit.collider.GetComponentInParent<Interactable>();
    if (interactableHit == null || !interactableHit.IsInteractable)
    {
      ClearCurrentObject();
      return;
    }

    // New object detected
    if (interactableHit != currentObject)
    {
      // Unhighlight previous object
      if (currentObject != null)
      {
        UnhighlightObject(currentObject.gameObject);
        EventBus.Publish(InteractEventState.OnExit);
      }

      // Highlight new object
      currentObject = interactableHit;
      previousObject = interactableHit;
      HighlightObject(currentObject.gameObject);
      EventBus.Publish(InteractEventState.OnEnter);
    }
  }

  private void ClearCurrentObject()
  {
    if (currentObject != null)
    {
      UnhighlightObject(currentObject.gameObject);
      EventBus.Publish(InteractEventState.OnExit);
      currentObject = null;
    }
  }

  private void HighlightObject(GameObject obj)
  {
    IHighlight highlight = obj.GetComponent<IHighlight>();
    if (highlight != null)
    {
      highlight.Highlight();
    }
  }

  private void UnhighlightObject(GameObject obj)
  {
    IHighlight highlight = obj.GetComponent<IHighlight>();
    if (highlight != null)
    {
      highlight.UnHighlight();
    }
  }

  private void OnPressInteract(InputAction.CallbackContext context)
  {
    if (currentObject != null)
    {
      currentObject.InteractObject();
      EventBus.Publish(InteractEventState.OnInteract);

      // Clear after interaction (object might be destroyed/disabled)
      currentObject = null;
    }
  }

  /// <summary>
  /// Get the object player is currently looking at
  /// </summary>
  public Interactable GetCurrentInteractable() => currentObject;

  /// <summary>
  /// Check if player is looking at any interactable
  /// </summary>
  public bool HasInteractable() => currentObject != null;

  private void OnDrawGizmosSelected()
  {
    if (raycastOrigin != null)
    {
      Gizmos.color = Color.cyan;
      Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactDistance);
      Gizmos.DrawWireSphere(raycastOrigin.position + raycastOrigin.forward * interactDistance, 0.1f);
    }
  }
}

public enum InteractEventState
{
  OnEnter,
  OnExit,
  OnInteract
}
