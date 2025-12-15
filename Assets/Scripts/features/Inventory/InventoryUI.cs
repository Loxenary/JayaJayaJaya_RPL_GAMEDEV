using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Data class representing an inventory item.
/// </summary>
[System.Serializable]
public class InventoryItemData
{
  public string itemName;
  public Sprite icon;
  public int id; // For keys, this is the key ID
  public bool isKey;

  public InventoryItemData(string name, Sprite sprite, int itemId, bool key = false)
  {
    itemName = name;
    icon = sprite;
    id = itemId;
    isKey = key;
  }
}

/// <summary>
/// Displays collected items (especially keys) on the left side of the screen.
/// Features:
/// - Vertical list of collected items
/// - Keys always show their ID on the icon
/// - Scroll support to select items and view their names
/// - Items never disappear once collected
/// </summary>
public class InventoryUI : MonoBehaviour
{
  [Header("UI References")]
  [Tooltip("Container for inventory item slots (VerticalLayoutGroup)")]
  [SerializeField] private Transform itemContainer;

  [Tooltip("Prefab for inventory item slot")]
  [SerializeField] private GameObject itemSlotPrefab;

  [Header("Layout & Scroll")]
  [Tooltip("Optional viewport RectTransform for scrolling. If empty, parent of itemContainer will be used.")]
  [SerializeField] private RectTransform containerViewport;

  [Tooltip("Maximum height of the inventory list before enabling vertical scroll")]
  [SerializeField] private float maxContainerHeight = 380f;

  [Tooltip("Limit the container height and enable scrolling when exceeded")]
  [SerializeField] private bool limitContainerHeight = true;

  [Tooltip("Text to display selected item's name")]
  [SerializeField] private TextMeshProUGUI selectedItemNameText;

  [Header("Key Settings")]
  [Tooltip("Default icon for keys")]
  [SerializeField] private Sprite defaultKeyIcon;

  [Tooltip("Key name prefix (e.g., 'Key #')")]
  [SerializeField] private string keyNamePrefix = "Kunci #";

  [Header("Visual Settings")]
  [Tooltip("Size of each item slot")]
  [SerializeField] private Vector2 slotSize = new Vector2(60, 60);

  [Tooltip("Color when item is selected")]
  [SerializeField] private Color selectedColor = Color.white;

  [Tooltip("Color when item is not selected")]
  [SerializeField] private Color unselectedColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);

  [Tooltip("Scale when selected")]
  [SerializeField] private float selectedScale = 1.2f;

  [Tooltip("Animation duration")]
  [SerializeField] private float animDuration = 0.2f;

  [Header("Scroll Settings")]
  [Tooltip("Time before auto-hiding selection (0 = never hide)")]
  [SerializeField] private float selectionHideDelay = 3f;

  // Runtime data
  private List<InventoryItemData> collectedItems = new List<InventoryItemData>();
  private List<InventorySlot> itemSlots = new List<InventorySlot>();
  private int selectedIndex = -1;
  private float lastScrollTime;
  private bool isSelectionVisible = false;

  // Input
  private PlayerInputHandler inputHandler;

  private void Awake()
  {
    ValidateReferences();
    SetupContainer();
  }

  private void OnEnable()
  {
    // Subscribe to key collection events
    InteractableKey.onFoundKey += OnKeyCollected;
  }

  private void OnDisable()
  {
    InteractableKey.onFoundKey -= OnKeyCollected;
  }

  private void Start()
  {
    // Try to find player input handler
    var player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    {
      inputHandler = player.GetComponent<PlayerInputHandler>();
    }

    // Hide selection text initially
    if (selectedItemNameText != null)
    {
      selectedItemNameText.gameObject.SetActive(false);
    }
  }

  private void Update()
  {
    HandleScrollInput();
    HandleSelectionTimeout();
  }

  private void ValidateReferences()
  {
    if (itemContainer == null)
    {
      Debug.LogWarning("[InventoryUI] Item container not assigned!");
    }

    if (itemSlotPrefab == null)
    {
      Debug.LogWarning("[InventoryUI] Item slot prefab not assigned! Will create basic slots.");
    }
  }

  private void SetupContainer()
  {
    if (itemContainer == null) return;

    EnsureViewportAndScroll();

    // Ensure vertical layout
    var layoutGroup = itemContainer.GetComponent<VerticalLayoutGroup>();
    if (layoutGroup == null)
    {
      layoutGroup = itemContainer.gameObject.AddComponent<VerticalLayoutGroup>();
      layoutGroup.spacing = 8f;
      layoutGroup.childControlWidth = false;
      layoutGroup.childControlHeight = false;
      layoutGroup.childForceExpandWidth = false;
      layoutGroup.childForceExpandHeight = false;
      layoutGroup.childAlignment = TextAnchor.UpperLeft;
      layoutGroup.padding = new RectOffset(10, 10, 10, 10);
    }

    // Add content size fitter
    var sizeFitter = itemContainer.GetComponent<ContentSizeFitter>();
    if (sizeFitter == null)
    {
      sizeFitter = itemContainer.gameObject.AddComponent<ContentSizeFitter>();
      sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    ClampContainerHeight();
  }

  /// <summary>
  /// Called when a key is collected
  /// </summary>
  private void OnKeyCollected(int keyId)
  {
    // Check if key already collected
    if (collectedItems.Exists(item => item.isKey && item.id == keyId))
    {
      Debug.Log($"[InventoryUI] Key #{keyId} already in inventory");
      return;
    }

    // Fallback icon if none assigned
    if (defaultKeyIcon == null)
    {
      Debug.LogWarning("[InventoryUI] Default key icon not assigned. Creating fallback white icon.");
      defaultKeyIcon = CreateFallbackIcon();
    }

    // Create key item data
    var keyItem = new InventoryItemData(
        keyNamePrefix + keyId,
        defaultKeyIcon,
        keyId,
        true
    );

    AddItem(keyItem);
    Debug.Log($"[InventoryUI] Key #{keyId} added to inventory");
  }

  /// <summary>
  /// Create a simple white square sprite as fallback icon
  /// </summary>
  private Sprite CreateFallbackIcon()
  {
    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
    tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
    tex.Apply();
    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
  }

  /// <summary>
  /// Add an item to the inventory
  /// </summary>
  public void AddItem(InventoryItemData item)
  {
    collectedItems.Add(item);
    CreateItemSlot(item);

    // Auto-select first item
    if (collectedItems.Count == 1)
    {
      SelectItem(0);
    }
  }

  /// <summary>
  /// Create a visual slot for an item
  /// </summary>
  private void CreateItemSlot(InventoryItemData item)
  {
    if (itemContainer == null) return;

    GameObject slotObj;

    if (itemSlotPrefab != null)
    {
      slotObj = Instantiate(itemSlotPrefab, itemContainer);
    }
    else
    {
      // Create basic slot if no prefab
      slotObj = CreateBasicSlot();
    }

    var slot = slotObj.GetComponent<InventorySlot>();
    if (slot == null)
    {
      slot = slotObj.AddComponent<InventorySlot>();
    }

    slot.Setup(item, slotSize);
    itemSlots.Add(slot);

    // Animate entry
    slotObj.transform.localScale = Vector3.zero;
    slotObj.transform.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack);

    ClampContainerHeight();
  }

  /// <summary>
  /// Create a basic inventory slot if no prefab is assigned
  /// </summary>
  private GameObject CreateBasicSlot()
  {
    var slotObj = new GameObject("InventorySlot");
    slotObj.transform.SetParent(itemContainer, false);

    // Add RectTransform
    var rectTransform = slotObj.AddComponent<RectTransform>();
    rectTransform.sizeDelta = slotSize;

    // Add background image
    var bgImage = slotObj.AddComponent<Image>();
    bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    // Add icon child
    var iconObj = new GameObject("Icon");
    iconObj.transform.SetParent(slotObj.transform, false);
    var iconRect = iconObj.AddComponent<RectTransform>();
    iconRect.anchorMin = Vector2.zero;
    iconRect.anchorMax = Vector2.one;
    iconRect.offsetMin = new Vector2(5, 5);
    iconRect.offsetMax = new Vector2(-5, -5);
    var iconImage = iconObj.AddComponent<Image>();

    // Add ID text child (for keys)
    var textObj = new GameObject("IDText");
    textObj.transform.SetParent(slotObj.transform, false);
    var textRect = textObj.AddComponent<RectTransform>();
    textRect.anchorMin = Vector2.zero;
    textRect.anchorMax = Vector2.one;
    textRect.offsetMin = Vector2.zero;
    textRect.offsetMax = Vector2.zero;
    var idText = textObj.AddComponent<TextMeshProUGUI>();
    idText.alignment = TextAlignmentOptions.Center;
    idText.fontSize = 24;
    idText.fontStyle = FontStyles.Bold;
    idText.color = Color.white;
    idText.textWrappingMode = TextWrappingModes.NoWrap;

    // Add outline for better visibility
    var outline = textObj.AddComponent<Outline>();
    outline.effectColor = Color.black;
    outline.effectDistance = new Vector2(1, -1);

    return slotObj;
  }

  /// <summary>
  /// Handle scroll wheel input for item selection
  /// </summary>
  private void HandleScrollInput()
  {
    if (collectedItems.Count == 0) return;

    float scrollDelta = Input.mouseScrollDelta.y;

    if (Mathf.Abs(scrollDelta) > 0.1f)
    {
      int direction = scrollDelta > 0 ? -1 : 1;
      int newIndex = selectedIndex + direction;

      // Wrap around
      if (newIndex < 0) newIndex = collectedItems.Count - 1;
      if (newIndex >= collectedItems.Count) newIndex = 0;

      SelectItem(newIndex);
      lastScrollTime = Time.time;
      ShowSelection();
    }
  }

  /// <summary>
  /// Handle auto-hiding selection after timeout
  /// </summary>
  private void HandleSelectionTimeout()
  {
    if (!isSelectionVisible || selectionHideDelay <= 0) return;

    if (Time.time - lastScrollTime > selectionHideDelay)
    {
      HideSelection();
    }
  }

  /// <summary>
  /// Select an item by index
  /// </summary>
  public void SelectItem(int index)
  {
    if (index < 0 || index >= collectedItems.Count) return;

    // Deselect previous
    if (selectedIndex >= 0 && selectedIndex < itemSlots.Count)
    {
      itemSlots[selectedIndex].SetSelected(false, unselectedColor, 1f, animDuration);
    }

    selectedIndex = index;

    // Select new
    if (selectedIndex >= 0 && selectedIndex < itemSlots.Count)
    {
      itemSlots[selectedIndex].SetSelected(true, selectedColor, selectedScale, animDuration);
    }

    // Update name display
    UpdateSelectedItemName();

    // Move selected label near the selected slot
    PositionSelectedLabel();
  }

  /// <summary>
  /// Update the displayed item name
  /// </summary>
  private void UpdateSelectedItemName()
  {
    if (selectedItemNameText == null) return;

    if (selectedIndex >= 0 && selectedIndex < collectedItems.Count)
    {
      selectedItemNameText.text = collectedItems[selectedIndex].itemName;
    }
  }

  /// <summary>
  /// Position the selected item label near the selected slot (align Y).
  /// </summary>
  private void PositionSelectedLabel()
  {
    if (selectedItemNameText == null) return;
    if (selectedIndex < 0 || selectedIndex >= itemSlots.Count) return;

    var slot = itemSlots[selectedIndex];
    var slotRT = slot.GetComponent<RectTransform>();
    var labelRT = selectedItemNameText.rectTransform;

    if (slotRT == null || labelRT == null) return;

    // Compute world center of slot regardless of pivot/anchors
    Vector3 slotLocalCenter = new Vector3(
      (0.5f - slotRT.pivot.x) * slotRT.rect.width,
      (0.5f - slotRT.pivot.y) * slotRT.rect.height,
      0f);

    Vector3 slotWorldCenter = slotRT.TransformPoint(slotLocalCenter);

    var labelWorldPos = labelRT.position;
    labelWorldPos.y = slotWorldCenter.y;
    labelRT.position = labelWorldPos;
  }

  /// <summary>
  /// Show selection UI
  /// </summary>
  private void ShowSelection()
  {
    isSelectionVisible = true;

    if (selectedItemNameText != null)
    {
      selectedItemNameText.gameObject.SetActive(true);
      selectedItemNameText.DOFade(1f, animDuration);
    }
  }

  /// <summary>
  /// Hide selection UI
  /// </summary>
  private void HideSelection()
  {
    isSelectionVisible = false;

    if (selectedItemNameText != null)
    {
      selectedItemNameText.DOFade(0f, animDuration).OnComplete(() =>
      {
        selectedItemNameText.gameObject.SetActive(false);
      });
    }
  }

  /// <summary>
  /// Ensure viewport, scrollrect, and mask exist when limiting height.
  /// </summary>
  private void EnsureViewportAndScroll()
  {
    if (!limitContainerHeight || maxContainerHeight <= 0f) return;

    // Try to resolve viewport
    if (containerViewport == null && itemContainer != null && itemContainer.parent != null)
    {
      containerViewport = itemContainer.parent as RectTransform;
    }

    if (containerViewport == null) return;

    // If parent already has a ScrollRect, reuse it
    var existingScroll = containerViewport.GetComponent<ScrollRect>();

    // Add RectMask2D to clip overflowing content
    var mask = containerViewport.GetComponent<RectMask2D>();
    if (mask == null)
    {
      mask = containerViewport.gameObject.AddComponent<RectMask2D>();
    }

    // Add or configure ScrollRect on viewport
    var scroll = existingScroll != null ? existingScroll : containerViewport.gameObject.AddComponent<ScrollRect>();

    scroll.horizontal = false;
    scroll.vertical = true;
    scroll.content = itemContainer as RectTransform;
    scroll.viewport = containerViewport;
    scroll.scrollSensitivity = 15f;
    scroll.movementType = ScrollRect.MovementType.Clamped;

    // Ensure content anchors/pivot are top-left for vertical scrolling
    var contentRT = itemContainer as RectTransform;
    if (contentRT != null)
    {
      contentRT.anchorMin = new Vector2(0f, 1f);
      contentRT.anchorMax = new Vector2(1f, 1f);
      contentRT.pivot = new Vector2(0.5f, 1f);
      contentRT.anchoredPosition = Vector2.zero;
    }

    // Optional: make viewport respect max height via LayoutElement
    var le = containerViewport.GetComponent<LayoutElement>();
    if (le == null)
    {
      le = containerViewport.gameObject.AddComponent<LayoutElement>();
    }
    le.preferredHeight = maxContainerHeight;
    le.flexibleHeight = 0f;
  }

  /// <summary>
  /// Clamp the container height to maxContainerHeight while keeping preferred height for layout.
  /// </summary>
  private void ClampContainerHeight()
  {
    if (!limitContainerHeight || maxContainerHeight <= 0f) return;

    // Limit the VIEWPORT height; keep content preferred height so scrolling works
    if (containerViewport != null)
    {
      containerViewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxContainerHeight);

      var le = containerViewport.GetComponent<LayoutElement>();
      if (le == null) le = containerViewport.gameObject.AddComponent<LayoutElement>();
      le.preferredHeight = maxContainerHeight;
      le.flexibleHeight = 0f;
    }
  }

  /// <summary>
  /// Get all collected items
  /// </summary>
  public List<InventoryItemData> GetCollectedItems() => collectedItems;

  /// <summary>
  /// Check if a specific key has been collected
  /// </summary>
  public bool HasKey(int keyId)
  {
    return collectedItems.Exists(item => item.isKey && item.id == keyId);
  }

  /// <summary>
  /// Add a custom item (non-key)
  /// </summary>
  public void AddCustomItem(string name, Sprite icon, int id = 0)
  {
    var item = new InventoryItemData(name, icon, id, false);
    AddItem(item);
  }
}

/// <summary>
/// Individual inventory slot UI component
/// </summary>
public class InventorySlot : MonoBehaviour
{
  private Image backgroundImage;
  private Image iconImage;
  private TextMeshProUGUI idText;
  private InventoryItemData itemData;
  private RectTransform rectTransform;
  private InventorySlotPrefab prefabRefs;

  public void Setup(InventoryItemData data, Vector2 size)
  {
    itemData = data;

    rectTransform = GetComponent<RectTransform>();
    if (rectTransform != null)
    {
      rectTransform.sizeDelta = size;
    }

    // Find components
    prefabRefs = GetComponent<InventorySlotPrefab>();

    if (prefabRefs != null)
    {
      backgroundImage = prefabRefs.BackgroundImage;
      iconImage = prefabRefs.IconImage;
      idText = prefabRefs.IDText;
    }
    else
    {
      backgroundImage = GetComponent<Image>();
      iconImage = transform.Find("Icon")?.GetComponent<Image>();
      idText = transform.Find("IDText")?.GetComponent<TextMeshProUGUI>();
    }

    // Fallback: try GetComponentInChildren
    if (iconImage == null)
    {
      var images = GetComponentsInChildren<Image>();
      if (images.Length > 1)
        iconImage = images[1];
    }

    if (idText == null)
    {
      idText = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Set icon
    if (iconImage != null && data.icon != null)
    {
      iconImage.sprite = data.icon;
      iconImage.preserveAspect = true;
      iconImage.enabled = true;
      var c = iconImage.color;
      c.a = 1f;
      iconImage.color = c;
    }

    // Set ID text for keys (always visible)
    if (idText != null)
    {
      if (data.isKey)
      {
        idText.text = data.id.ToString();
        idText.gameObject.SetActive(true);
      }
      else
      {
        idText.gameObject.SetActive(false);
      }
    }
  }

  public void SetSelected(bool selected, Color color, float scale, float duration)
  {
    // Animate scale
    transform.DOScale(Vector3.one * scale, duration).SetEase(Ease.OutQuad);

    // Animate color
    if (backgroundImage != null)
    {
      backgroundImage.DOColor(color, duration);
    }

    if (iconImage != null)
    {
      iconImage.DOColor(selected ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f), duration);
    }
  }

  public InventoryItemData GetItemData() => itemData;
}
