using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Types of items that can be displayed in the HUD.
/// </summary>
public enum ItemType
{
    Food,
    Medicine,
    Custom
}

/// <summary>
/// Displays item icons (food, medicine, etc.) dynamically in the HUD.
/// </summary>
public class ItemIconsDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Container where item icons will be spawned")]
    [SerializeField] private Transform iconContainer;

    [Tooltip("Prefab for item icon (should contain Image and optional Text components)")]
    [SerializeField] private GameObject itemIconPrefab;

    [Header("Layout Settings")]
    [Tooltip("Maximum number of items to display")]
    [SerializeField] private int maxItems = 5;

    [Tooltip("Spacing between items")]
    [SerializeField] private float itemSpacing = 10f;

    [Tooltip("Use horizontal layout")]
    [SerializeField] private bool horizontalLayout = true;

    [Header("Display Settings")]
    [Tooltip("Show quantity text on icons")]
    [SerializeField] private bool showQuantity = true;

    [Tooltip("Icon size")]
    [SerializeField] private Vector2 iconSize = new Vector2(50, 50);

    [Tooltip("Scale animation when adding/removing items")]
    [SerializeField] private bool useScaleAnimation = true;

    [Tooltip("Animation duration")]
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Item Type Colors (Optional)")]
    [Tooltip("Tint color for food items")]
    [SerializeField] private Color foodColor = Color.white;

    [Tooltip("Tint color for medicine items")]
    [SerializeField] private Color medicineColor = Color.white;

    private Dictionary<ItemType, ItemIconUI> activeItems = new Dictionary<ItemType, ItemIconUI>();

    private void Awake()
    {
        ValidateComponents();
        SetupLayout();
    }

    private void ValidateComponents()
    {
        if (iconContainer == null)
        {
            Debug.LogError("[ItemIconsDisplay] Icon container is not assigned!", this);
        }

        if (itemIconPrefab == null)
        {
            Debug.LogError("[ItemIconsDisplay] Item icon prefab is not assigned!", this);
        }
    }

    private void SetupLayout()
    {
        if (iconContainer == null) return;

        // Add or configure layout group
        LayoutGroup layoutGroup = iconContainer.GetComponent<LayoutGroup>();

        if (layoutGroup == null)
        {
            if (horizontalLayout)
            {
                HorizontalLayoutGroup hLayout = iconContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                hLayout.spacing = itemSpacing;
                hLayout.childControlWidth = false;
                hLayout.childControlHeight = false;
                hLayout.childForceExpandWidth = false;
                hLayout.childForceExpandHeight = false;
                hLayout.childAlignment = TextAnchor.MiddleLeft;
            }
            else
            {
                VerticalLayoutGroup vLayout = iconContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                vLayout.spacing = itemSpacing;
                vLayout.childControlWidth = false;
                vLayout.childControlHeight = false;
                vLayout.childForceExpandWidth = false;
                vLayout.childForceExpandHeight = false;
                vLayout.childAlignment = TextAnchor.UpperCenter;
            }
        }
    }

    /// <summary>
    /// Adds an item icon to the display.
    /// </summary>
    public void AddItem(ItemType itemType, Sprite icon, int quantity = 1)
    {
        // Check if item already exists
        if (activeItems.ContainsKey(itemType))
        {
            UpdateItemQuantity(itemType, quantity);
            return;
        }

        // Check max items limit
        if (activeItems.Count >= maxItems)
        {
            Debug.LogWarning($"[ItemIconsDisplay] Maximum items ({maxItems}) reached. Cannot add more items.");
            return;
        }

        // Create new item icon
        GameObject iconObj = Instantiate(itemIconPrefab, iconContainer);
        ItemIconUI itemUI = iconObj.GetComponent<ItemIconUI>();

        if (itemUI == null)
        {
            itemUI = iconObj.AddComponent<ItemIconUI>();
        }

        // Setup the item
        itemUI.Setup(icon, quantity, showQuantity, iconSize);
        itemUI.SetColor(GetItemColor(itemType));

        activeItems[itemType] = itemUI;

        // Play add animation
        if (useScaleAnimation)
        {
            StartCoroutine(AnimateItemScale(itemUI, 0f, 1f));
        }
    }

    /// <summary>
    /// Removes an item icon from the display.
    /// </summary>
    public void RemoveItem(ItemType itemType)
    {
        if (!activeItems.ContainsKey(itemType))
        {
            return;
        }

        ItemIconUI itemUI = activeItems[itemType];
        activeItems.Remove(itemType);

        if (useScaleAnimation)
        {
            StartCoroutine(AnimateItemScaleAndDestroy(itemUI));
        }
        else
        {
            Destroy(itemUI.gameObject);
        }
    }

    /// <summary>
    /// Updates the quantity of an existing item.
    /// </summary>
    public void UpdateItemQuantity(ItemType itemType, int quantity)
    {
        if (activeItems.ContainsKey(itemType))
        {
            activeItems[itemType].SetQuantity(quantity);

            // Remove item if quantity reaches 0
            if (quantity <= 0)
            {
                RemoveItem(itemType);
            }
        }
    }

    /// <summary>
    /// Clears all items from the display.
    /// </summary>
    public void ClearAllItems()
    {
        foreach (var kvp in activeItems)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        activeItems.Clear();
    }

    /// <summary>
    /// Gets the number of items currently displayed.
    /// </summary>
    public int GetItemCount()
    {
        return activeItems.Count;
    }

    /// <summary>
    /// Checks if an item type is currently displayed.
    /// </summary>
    public bool HasItem(ItemType itemType)
    {
        return activeItems.ContainsKey(itemType);
    }

    private Color GetItemColor(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Food:
                return foodColor;
            case ItemType.Medicine:
                return medicineColor;
            default:
                return Color.white;
        }
    }

    private System.Collections.IEnumerator AnimateItemScale(ItemIconUI itemUI, float fromScale, float toScale)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float scale = Mathf.Lerp(fromScale, toScale, t);
            itemUI.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        itemUI.transform.localScale = Vector3.one * toScale;
    }

    private System.Collections.IEnumerator AnimateItemScaleAndDestroy(ItemIconUI itemUI)
    {
        yield return AnimateItemScale(itemUI, 1f, 0f);
        Destroy(itemUI.gameObject);
    }

    /// <summary>
    /// Shows or hides the item icons display.
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}

/// <summary>
/// UI component for individual item icons.
/// </summary>
public class ItemIconUI : MonoBehaviour
{
    private Image iconImage;
    private TextMeshProUGUI quantityText;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        // Try to find Image component
        iconImage = GetComponent<Image>();
        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>();
        }
        if (iconImage == null)
        {
            iconImage = gameObject.AddComponent<Image>();
        }

        // Try to find TextMeshProUGUI component
        quantityText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(Sprite icon, int quantity, bool showQuantity, Vector2 size)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = size;
        }

        SetQuantity(quantity);

        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(showQuantity);
        }
    }

    public void SetQuantity(int quantity)
    {
        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    public void SetColor(Color color)
    {
        if (iconImage != null)
        {
            iconImage.color = color;
        }
    }

    public void SetIcon(Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }
    }
}
