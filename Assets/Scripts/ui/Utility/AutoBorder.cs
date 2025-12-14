using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Generates UI borders (top/bottom/left/right) as child Images automatically.
/// Toggle each side, set thickness, inset, and color. Works in Edit Mode for live preview.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class AutoBorder : MonoBehaviour
{
  private const string TopName = "BorderTop";
  private const string BottomName = "BorderBottom";
  private const string LeftName = "BorderLeft";
  private const string RightName = "BorderRight";

  [Header("Sides")]
  [SerializeField] private bool enableTop = true;
  [SerializeField] private bool enableBottom = true;
  [SerializeField] private bool enableLeft = true;
  [SerializeField] private bool enableRight = true;

  [Header("Style")]
  [SerializeField] private float thickness = 5f;
  [SerializeField] private float inset = 0f; // reduce width/height from edges
  [SerializeField] private Color borderColor = Color.white;
  [SerializeField] private bool raycastTarget = false;

  [Header("Overlap & Corners")]
  [Tooltip("Trim border length so corners don't overlap (helps when alpha < 1)")]
  [SerializeField] private bool trimCorners = true;

  [Tooltip("Which pair is trimmed to avoid overlap (Horizontal trims Top/Bottom; Vertical trims Left/Right)")]
  [SerializeField] private TrimPair trimPair = TrimPair.Horizontal;

  [Tooltip("Use sliced sprite for rounded corners")]
  [SerializeField] private bool roundedCorners = false;

  [Tooltip("Rounded (sliced) sprite for borders")]
  [SerializeField] private Sprite roundedSprite;

  private RectTransform rectTransform;

  private void Awake()
  {
    rectTransform = GetComponent<RectTransform>();
    RebuildBorders();
  }

  private void OnEnable()
  {
    rectTransform = GetComponent<RectTransform>();
    RebuildBorders();
  }

  private void OnValidate()
  {
    rectTransform = GetComponent<RectTransform>();

#if UNITY_EDITOR
    // Delay to avoid prefab serialization issues
    if (!Application.isPlaying)
    {
      EditorApplication.delayCall += () =>
      {
        if (this == null) return;
        // Skip when editing prefab asset itself to avoid unwanted overrides
        if (IsEditingPrefabAsset()) return;
        RebuildBorders();
      };
    }
#endif
  }

  /// <summary>
  /// Public call to force rebuild at runtime.
  /// </summary>
  public void ForceRebuild()
  {
    RebuildBorders();
  }

  private void RebuildBorders()
  {
#if UNITY_EDITOR
    // Skip when editing prefab asset directly
    if (!Application.isPlaying && IsEditingPrefabAsset()) return;
#endif

    CleanupChildren(Application.isPlaying ? false : true);

    if (enableTop) CreateBorder(TopName, BorderSide.Top);
    if (enableBottom) CreateBorder(BottomName, BorderSide.Bottom);
    if (enableLeft) CreateBorder(LeftName, BorderSide.Left);
    if (enableRight) CreateBorder(RightName, BorderSide.Right);
  }

  private enum BorderSide { Top, Bottom, Left, Right }
  private enum TrimPair { Horizontal, Vertical }

  private void CreateBorder(string name, BorderSide side)
  {
    GameObject go = new GameObject(name);
    go.transform.SetParent(transform, false);
    var img = go.AddComponent<Image>();
    img.color = borderColor;
    img.raycastTarget = raycastTarget;

    // Apply rounded corners sprite if provided
    if (roundedCorners && roundedSprite != null)
    {
      img.sprite = roundedSprite;
      img.type = Image.Type.Sliced;
      img.preserveAspect = false;
    }

    var rt = img.rectTransform;

    // Compute trims based on selected trim pair and enabled adjacent borders
    float leftTrim = 0f, rightTrim = 0f, topTrim = 0f, bottomTrim = 0f;

    if (trimCorners)
    {
      if (trimPair == TrimPair.Horizontal && (side == BorderSide.Top || side == BorderSide.Bottom))
      {
        leftTrim = enableLeft ? thickness : 0f;
        rightTrim = enableRight ? thickness : 0f;
      }
      else if (trimPair == TrimPair.Vertical && (side == BorderSide.Left || side == BorderSide.Right))
      {
        topTrim = enableTop ? thickness : 0f;
        bottomTrim = enableBottom ? thickness : 0f;
      }
    }

    switch (side)
    {
      case BorderSide.Top:
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, thickness);
        rt.anchoredPosition = new Vector2(0f, -inset);
        // inset width
        rt.offsetMin = new Vector2(inset + leftTrim, rt.offsetMin.y);
        rt.offsetMax = new Vector2(-(inset + rightTrim), rt.offsetMax.y);
        break;
      case BorderSide.Bottom:
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(0f, thickness);
        rt.anchoredPosition = new Vector2(0f, inset);
        rt.offsetMin = new Vector2(inset + leftTrim, rt.offsetMin.y);
        rt.offsetMax = new Vector2(-(inset + rightTrim), rt.offsetMax.y);
        break;
      case BorderSide.Left:
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(thickness, 0f);
        rt.anchoredPosition = new Vector2(inset, 0f);
        rt.offsetMin = new Vector2(rt.offsetMin.x, topTrim);
        rt.offsetMax = new Vector2(rt.offsetMax.x, -bottomTrim);
        break;
      case BorderSide.Right:
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(thickness, 0f);
        rt.anchoredPosition = new Vector2(-inset, 0f);
        rt.offsetMin = new Vector2(rt.offsetMin.x, topTrim);
        rt.offsetMax = new Vector2(rt.offsetMax.x, -bottomTrim);
        break;
    }

#if UNITY_EDITOR
    if (!Application.isPlaying)
    {
      go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontSave;
      img.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.DontSave;
    }
#endif
  }

  private void CleanupChildren(bool immediate)
  {
    for (int i = transform.childCount - 1; i >= 0; i--)
    {
      var child = transform.GetChild(i);
      if (child == null) continue;
      // Only remove auto-generated borders (by name prefix Border)
      if (child.name.StartsWith("Border"))
      {
#if UNITY_EDITOR
        if (immediate)
          DestroyImmediate(child.gameObject);
        else
          Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
      }
    }
  }

#if UNITY_EDITOR
  private bool IsEditingPrefabAsset()
  {
    return PrefabUtility.IsPartOfPrefabAsset(gameObject);
  }
#endif
}
