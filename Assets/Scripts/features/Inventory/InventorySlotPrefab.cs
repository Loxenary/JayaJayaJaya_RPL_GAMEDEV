using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Prefab component for inventory slot.
/// Attach this to a UI prefab with Image background, Icon Image, and IDText.
/// </summary>
public class InventorySlotPrefab : MonoBehaviour
{
  [Header("Required References")]
  [SerializeField] private Image backgroundImage;
  [SerializeField] private Image iconImage;
  [SerializeField] private TextMeshProUGUI idText;

  [Header("Optional")]
  [SerializeField] private Image selectionHighlight;

  public Image BackgroundImage => backgroundImage;
  public Image IconImage => iconImage;
  public TextMeshProUGUI IDText => idText;
  public Image SelectionHighlight => selectionHighlight;

  private void Reset()
  {
    // Auto-find components on reset
    backgroundImage = GetComponent<Image>();
    iconImage = transform.Find("Icon")?.GetComponent<Image>();
    idText = transform.Find("IDText")?.GetComponent<TextMeshProUGUI>();
    selectionHighlight = transform.Find("Highlight")?.GetComponent<Image>();
  }

  /// <summary>
  /// Setup the slot with item data
  /// </summary>
  public void SetupSlot(Sprite icon, int id, bool isKey)
  {
    if (iconImage != null && icon != null)
    {
      iconImage.sprite = icon;
      iconImage.enabled = true;
    }

    if (idText != null)
    {
      if (isKey)
      {
        idText.text = id.ToString();
        idText.gameObject.SetActive(true);
      }
      else
      {
        idText.gameObject.SetActive(false);
      }
    }
  }

  /// <summary>
  /// Set selection state
  /// </summary>
  public void SetSelected(bool selected)
  {
    if (selectionHighlight != null)
    {
      selectionHighlight.enabled = selected;
    }
  }
}
