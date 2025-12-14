using UnityEngine;

/// <summary>
/// Child collider used as interaction zone. Set layer to Interactable and forward to parent Interactable.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractableZone : MonoBehaviour
{
  private void Reset()
  {
    ApplyLayer();
    EnsureNonTrigger();
  }

  private void OnValidate()
  {
    ApplyLayer();
    EnsureNonTrigger();
  }

  private void Awake()
  {
    ApplyLayer();
  }

  private void ApplyLayer()
  {
    gameObject.layer = LayerMask.NameToLayer("Interactable");
  }

  private void EnsureNonTrigger()
  {
    var col = GetComponent<Collider>();
    if (col != null && col.isTrigger)
    {
      Debug.LogWarning($"[InteractableZone] {name}: Collider is trigger; raycast may miss. Consider non-trigger for interaction raycast.", this);
    }
  }
}
