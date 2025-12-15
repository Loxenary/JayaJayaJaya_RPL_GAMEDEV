using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Advanced tile/prop painter with grid snap, brush radius, erase, rotation/scale controls, and ghost preview.
/// </summary>
public class AdvancedTilePainter : EditorWindow
{
  [Header("Palette")]
  [SerializeField] private List<GameObject> palette = new List<GameObject>();
  [SerializeField] private int selectedIndex = 0;
  [SerializeField] private Transform parent; // optional parent for spawned tiles
  [SerializeField] private bool parentToSelection = true;
  [SerializeField] private bool usePrefabRotation = true;

  [Header("Placement")]
  [SerializeField] private float gridSize = 1f;
  [SerializeField] private bool lockY = true;
  [SerializeField] private float lockedY = 0f;
  [SerializeField] private Vector3 placementOffset = Vector3.zero;
  [SerializeField] private bool ignoreObstructions = true;
  [SerializeField] private float ySnapStep = 1f;
  [SerializeField] private float minHeightDifference = 0.25f;
  [SerializeField] private float lockedYStackStep = 1f;
  [SerializeField] private bool alignToSurfaceNormal = false;
  [SerializeField] private bool snapRotationY = true;
  [SerializeField] private float rotationStep = 90f;
  [SerializeField] private float baseRotationY = 0f;
  [SerializeField] private Vector2 randomRotationYRange = new Vector2(0f, 0f);
  [SerializeField] private Vector2 scaleRange = Vector2.one;

  private enum PaintMode { Brush, Rectangle }

  [Header("Brush/Shape")]
  [SerializeField] private float brushRadius = 0.5f;
  [SerializeField] private bool paintOnDrag = true;
  [SerializeField] private bool eraseMode = false;
  [SerializeField] private float eraseRadius = 0.6f;
  [SerializeField] private PaintMode paintMode = PaintMode.Brush;

  [Header("Visuals")]
  [SerializeField] private Color ghostColor = new Color(0f, 1f, 1f, 0.35f);
  [SerializeField] private Color brushColor = new Color(0f, 1f, 1f, 0.35f);
  [SerializeField] private bool fallbackToLockedPlane = true; // allow preview even when ray misses colliders

  private const string prefsPaletteKey = "AdvancedTilePainter.PaletteGuids";
  private const string prefsSelectedIndexKey = "AdvancedTilePainter.SelectedIndex";
  private const string prefsSettingsKey = "AdvancedTilePainter.Settings";

  private GameObject ghostInstance;
  private Vector3 lastPlacedPos;
  private bool hasLastPos;
  private const float occupancyTolerance = 0.2f;
  private const string paintedPrefix = "[Painted] ";
  private static readonly Dictionary<Vector3Int, GameObject> paintedLookup = new Dictionary<Vector3Int, GameObject>();
  private Vector2 scrollPos;

  // Rectangle tool state
  private bool isDraggingRect;
  private Vector3 rectStartWorld;
  private Vector3 rectEndWorld;

  [MenuItem("Tools/Advanced Tile Painter")]
  public static void ShowWindow() => GetWindow<AdvancedTilePainter>("Tile Painter");

  private void OnEnable()
  {
    LoadSettings();
    LoadPalette();
    SceneView.duringSceneGui += OnSceneGUI;
    wantsMouseMove = true; // keep preview responsive while hovering
    Undo.undoRedoPerformed += OnUndoRedo;
    EnsureGhost();
    RebuildPaintedLookup();
  }

  private void OnDisable()
  {
    SceneView.duringSceneGui -= OnSceneGUI;
    DestroyImmediate(ghostInstance);
    paintedLookup.Clear();
    Undo.undoRedoPerformed -= OnUndoRedo;
    SavePalette();
    SaveSettings();
  }

  private void OnGUI()
  {
    int previousIndex = selectedIndex;

    EditorGUI.BeginChangeCheck();

    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

    EditorGUILayout.LabelField("Palette", EditorStyles.boldLabel);
    int removeIndex = -1;
    for (int i = 0; i < palette.Count; i++)
    {
      EditorGUILayout.BeginHorizontal();
      GameObject newObj = (GameObject)EditorGUILayout.ObjectField(palette[i], typeof(GameObject), false);
      if (newObj != palette[i])
      {
        palette[i] = newObj;
        if (selectedIndex == i)
        {
          RefreshGhost();
        }
        SavePalette();
      }

      if (GUILayout.Toggle(selectedIndex == i, "Use", GUILayout.Width(50))) selectedIndex = i;
      if (GUILayout.Button("X", GUILayout.Width(24))) removeIndex = i;
      EditorGUILayout.EndHorizontal();
    }
    if (removeIndex >= 0 && removeIndex < palette.Count)
    {
      palette.RemoveAt(removeIndex);
      selectedIndex = Mathf.Clamp(selectedIndex, 0, palette.Count - 1);
      RefreshGhost();
      SavePalette();
    }
    if (GUILayout.Button("+ Add Prefab"))
    {
      palette.Add(null);
      SavePalette();
    }

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
    gridSize = Mathf.Max(0.01f, EditorGUILayout.FloatField("Grid Size", gridSize));
    lockY = EditorGUILayout.Toggle("Lock Y", lockY);
    using (new EditorGUI.DisabledScope(!lockY))
    {
      lockedY = EditorGUILayout.FloatField("Locked Y", lockedY);
    }

    using (new EditorGUI.DisabledScope(lockY))
    {
      ySnapStep = Mathf.Max(0.01f, EditorGUILayout.FloatField("Y Snap Step", ySnapStep));
      minHeightDifference = Mathf.Max(0.01f, EditorGUILayout.FloatField("Min Height Difference", minHeightDifference));
    }

    using (new EditorGUI.DisabledScope(!lockY))
    {
      lockedYStackStep = Mathf.Max(0.01f, EditorGUILayout.FloatField("Locked Y Stack Step", lockedYStackStep));
    }
    ignoreObstructions = EditorGUILayout.Toggle("Ignore Obstructions", ignoreObstructions);
    placementOffset = EditorGUILayout.Vector3Field("Placement Offset", placementOffset);
    alignToSurfaceNormal = EditorGUILayout.Toggle("Align To Surface Normal", alignToSurfaceNormal);
    snapRotationY = EditorGUILayout.Toggle("Snap Rotation Y", snapRotationY);
    if (snapRotationY)
    {
      rotationStep = Mathf.Max(1f, EditorGUILayout.FloatField("Rotation Step (deg)", rotationStep));
      baseRotationY = EditorGUILayout.Slider("Base Rotation Y", baseRotationY, 0f, 360f);
    }
    randomRotationYRange = EditorGUILayout.Vector2Field("Random Rotation Y Range", randomRotationYRange);
    scaleRange = EditorGUILayout.Vector2Field("Uniform Scale Range", scaleRange);

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Brush / Shape", EditorStyles.boldLabel);
    paintMode = (PaintMode)EditorGUILayout.EnumPopup("Mode", paintMode);
    brushRadius = Mathf.Max(0.05f, EditorGUILayout.FloatField("Brush Radius", brushRadius));
    paintOnDrag = EditorGUILayout.Toggle("Paint On Drag", paintOnDrag);
    eraseMode = EditorGUILayout.Toggle("Erase Mode", eraseMode);
    eraseRadius = Mathf.Max(0.05f, EditorGUILayout.FloatField("Erase Radius", eraseRadius));

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
    ghostColor = EditorGUILayout.ColorField("Ghost Color", ghostColor);
    brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);

    parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);

    if (GUILayout.Button("Focus Scene View"))
    {
      SceneView.lastActiveSceneView?.Focus();
    }

    EditorGUILayout.EndScrollView();

    if (selectedIndex != previousIndex)
    {
      RefreshGhost();
      SavePalette();
    }

    if (EditorGUI.EndChangeCheck())
    {
      SaveSettings();
    }
  }

  private void OnSceneGUI(SceneView sceneView)
  {
    // If user is manipulating built-in handles/tools, don't consume events
    if (GUIUtility.hotControl != 0)
    {
      return;
    }

    if (palette.Count == 0 || selectedIndex < 0 || selectedIndex >= palette.Count || palette[selectedIndex] == null)
    {
      ClearGhost();
      return;
    }

    EnsureGhost();

    Event e = Event.current;
    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
    bool hasHit = Physics.Raycast(ray, out RaycastHit hitInfo, 500f);
    Vector3 planePos = Vector3.zero;
    bool hasPlane = false;

    if (lockY && ignoreObstructions)
    {
      Plane plane = new Plane(Vector3.up, new Vector3(0f, lockedY, 0f));
      if (plane.Raycast(ray, out float enterPlane))
      {
        planePos = ray.GetPoint(enterPlane);
        hasPlane = true;
      }
    }

    // Fallback to locked plane when no collider under cursor
    if (!hasHit && fallbackToLockedPlane)
    {
      Plane plane = new Plane(Vector3.up, new Vector3(0f, lockY ? lockedY : 0f, 0f));
      if (plane.Raycast(ray, out float enter))
      {
        hitInfo.point = ray.GetPoint(enter);
        hitInfo.normal = Vector3.up;
        hasHit = true;
      }
    }

    if (!hasHit && !hasPlane)
    {
      ClearGhost();
      return;
    }

    if (hasPlane)
    {
      hitInfo.point = planePos;
      hitInfo.normal = Vector3.up;
      hasHit = true;
    }

    Vector3 targetPos = hitInfo.point;
    if (lockY)
    {
      targetPos.y = lockedY;
    }
    SnapToGrid(ref targetPos);

    Quaternion baseRot = usePrefabRotation && palette[selectedIndex] != null
        ? palette[selectedIndex].transform.rotation
        : Quaternion.identity;

    Quaternion targetRot = baseRot;
    if (alignToSurfaceNormal)
    {
      targetRot = Quaternion.FromToRotation(Vector3.up, hitInfo.normal) * baseRot;
    }

    targetRot *= Quaternion.Euler(0f, baseRotationY, 0f);
    if (snapRotationY)
    {
      float snapped = Mathf.Round(targetRot.eulerAngles.y / rotationStep) * rotationStep;
      targetRot = Quaternion.Euler(targetRot.eulerAngles.x, snapped, targetRot.eulerAngles.z);
    }

    // Ghost preview
    PositionGhost(targetPos, targetRot);

    // Brush/shape visuals
    Handles.color = brushColor;
    if (paintMode == PaintMode.Brush)
    {
      Handles.DrawWireDisc(targetPos, Vector3.up, brushRadius);
    }
    else if (paintMode == PaintMode.Rectangle && isDraggingRect)
    {
      DrawRectanglePreview(targetPos);
    }

    // Interaction
    bool leftDown = e.type == EventType.MouseDown && e.button == 0 && !e.alt;
    bool leftDrag = e.type == EventType.MouseDrag && e.button == 0 && !e.alt;
    bool leftUp = e.type == EventType.MouseUp && e.button == 0 && !e.alt;

    if (paintMode == PaintMode.Brush)
    {
      bool action = leftDown || (paintOnDrag && leftDrag);
      if (action)
      {
        if (eraseMode)
          EraseAt(targetPos);
        else
          PlaceAt(targetPos, targetRot, bypassSpacing: false, bypassRadius: false);

        e.Use();
      }
    }
    else if (paintMode == PaintMode.Rectangle)
    {
      if (leftDown)
      {
        isDraggingRect = true;
        rectStartWorld = targetPos;
        rectEndWorld = targetPos;
        e.Use();
      }
      else if (leftDrag && isDraggingRect)
      {
        rectEndWorld = targetPos;
        e.Use();
      }
      else if (leftUp && isDraggingRect)
      {
        rectEndWorld = targetPos;
        ApplyRectangle(targetRot);
        isDraggingRect = false;
        e.Use();
      }
    }

    if (e.type == EventType.MouseMove)
    {
      SceneView.RepaintAll(); // keep ghost following cursor smoothly
    }
  }

  private void SnapToGrid(ref Vector3 pos)
  {
    Vector3 p = pos - placementOffset;
    p.x = Mathf.Round(p.x / gridSize) * gridSize;
    p.z = Mathf.Round(p.z / gridSize) * gridSize;
    if (!lockY)
    {
      p.y = Mathf.Round(p.y / ySnapStep) * ySnapStep;
    }
    else
    {
      p.y = Mathf.Round(p.y / lockedYStackStep) * lockedYStackStep;
    }
    pos = p + placementOffset;
  }

  private void PositionGhost(Vector3 pos, Quaternion rot)
  {
    if (ghostInstance == null) return;
    ghostInstance.transform.position = pos;
    ghostInstance.transform.rotation = rot;

    // Apply prefab default scale as base, then mid-range uniform factor
    Vector3 baseScale = Vector3.one;
    if (palette.Count > 0 && selectedIndex >= 0 && selectedIndex < palette.Count && palette[selectedIndex] != null)
    {
      baseScale = palette[selectedIndex].transform.localScale;
    }
    float previewScale = Mathf.Lerp(scaleRange.x, scaleRange.y, 0.5f);
    ghostInstance.transform.localScale = baseScale * previewScale;

    foreach (var r in ghostInstance.GetComponentsInChildren<Renderer>())
    {
      if (r.sharedMaterial != null)
      {
        Material mat = r.sharedMaterial;
        if (mat.HasProperty("_Color"))
        {
          Color c = ghostColor;
          c.a = ghostColor.a;
          mat.color = c;
        }
      }
    }
  }

  private void PlaceAt(Vector3 pos, Quaternion rot, bool bypassSpacing, bool bypassRadius)
  {
    if (palette.Count == 0 || selectedIndex < 0 || selectedIndex >= palette.Count) return;
    GameObject prefab = palette[selectedIndex];
    if (prefab == null) return;

    // Optional spacing throttle removed to avoid patterned gaps; grid/cell occupancy handles duplicates

    // Skip if a painted tile already occupies this grid cell
    Vector3Int cell = WorldToCell(pos);
    if (paintedLookup.TryGetValue(cell, out GameObject existing))
    {
      if (existing == null)
      {
        paintedLookup.Remove(cell); // stale entry from undo
      }
      else
      {
        return;
      }
    }

    // Radius overlap check removed; grid occupancy is authoritative

    float extraY = Random.Range(randomRotationYRange.x, randomRotationYRange.y);
    Quaternion finalRot = rot * Quaternion.Euler(0f, extraY, 0f);

    float uniformScale = Random.Range(scaleRange.x, scaleRange.y);

    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    Undo.RegisterCreatedObjectUndo(instance, "Paint Tile");

    instance.transform.position = pos;
    instance.transform.rotation = finalRot;
    instance.transform.localScale = prefab.transform.localScale * uniformScale;

    Transform chosenParent = parent;
    if (chosenParent == null && parentToSelection && Selection.activeTransform != null)
    {
      chosenParent = Selection.activeTransform;
    }
    if (chosenParent != null) instance.transform.SetParent(chosenParent);

    if (!instance.name.StartsWith(paintedPrefix))
    {
      instance.name = paintedPrefix + instance.name;
    }

    paintedLookup[cell] = instance;

    lastPlacedPos = pos;
    hasLastPos = true;
  }

  private void EraseAt(Vector3 pos)
  {
    List<Vector3Int> toRemove = new List<Vector3Int>();
    foreach (var kvp in paintedLookup)
    {
      GameObject go = kvp.Value;
      if (go == null)
      {
        toRemove.Add(kvp.Key);
        continue;
      }

      if (Vector3.Distance(go.transform.position, pos) <= eraseRadius)
      {
        Undo.DestroyObjectImmediate(go);
        toRemove.Add(kvp.Key);
      }
    }

    foreach (var cell in toRemove)
    {
      paintedLookup.Remove(cell);
    }
  }

  private void EnsureGhost()
  {
    if (ghostInstance != null) return;
    if (palette.Count == 0 || selectedIndex < 0 || selectedIndex >= palette.Count) return;
    GameObject prefab = palette.Count > 0 ? palette[Mathf.Clamp(selectedIndex, 0, palette.Count - 1)] : null;
    if (prefab == null) return;

    ghostInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    if (ghostInstance != null)
    {
      SceneVisibilityManager.instance.Hide(ghostInstance, true);
      ghostInstance.hideFlags = HideFlags.HideAndDontSave;
      foreach (var comp in ghostInstance.GetComponentsInChildren<Collider>())
      {
        comp.enabled = false;
      }
    }
  }

  private void ClearGhost()
  {
    if (ghostInstance != null)
    {
      DestroyImmediate(ghostInstance);
      ghostInstance = null;
    }
  }

  private void RefreshGhost()
  {
    ClearGhost();
    EnsureGhost();
    SceneView.RepaintAll();
  }

  private void SavePalette()
  {
    List<string> guids = new List<string>();
    foreach (var obj in palette)
    {
      string guid = obj != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)) : string.Empty;
      guids.Add(guid);
    }
    string payload = string.Join("|", guids);
    EditorPrefs.SetString(prefsPaletteKey, payload);
    EditorPrefs.SetInt(prefsSelectedIndexKey, selectedIndex);
  }

  private void LoadPalette()
  {
    if (!EditorPrefs.HasKey(prefsPaletteKey))
      return;

    string payload = EditorPrefs.GetString(prefsPaletteKey, string.Empty);
    string[] guids = payload.Split('|');
    palette.Clear();
    foreach (var guid in guids)
    {
      if (string.IsNullOrEmpty(guid))
      {
        palette.Add(null);
      }
      else
      {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        palette.Add(obj);
      }
    }

    selectedIndex = Mathf.Clamp(EditorPrefs.GetInt(prefsSelectedIndexKey, selectedIndex), 0, Mathf.Max(0, palette.Count - 1));
    RefreshGhost();
  }

  [System.Serializable]
  private struct PainterSettings
  {
    public float gridSize;
    public bool lockY;
    public float lockedY;
    public Vector3 placementOffset;
    public bool ignoreObstructions;
    public float ySnapStep;
    public float minHeightDifference;
    public float lockedYStackStep;
    public bool alignToSurfaceNormal;
    public bool snapRotationY;
    public float rotationStep;
    public float baseRotationY;
    public Vector2 randomRotationYRange;
    public Vector2 scaleRange;
    public float brushRadius;
    public bool paintOnDrag;
    public bool eraseMode;
    public float eraseRadius;
    public PaintMode paintMode;
    public bool fallbackToLockedPlane;
    public bool usePrefabRotation;
    public bool parentToSelection;
    public Color ghostColor;
    public Color brushColor;
  }

  private void SaveSettings()
  {
    PainterSettings data = new PainterSettings
    {
      gridSize = gridSize,
      lockY = lockY,
      lockedY = lockedY,
      placementOffset = placementOffset,
      ignoreObstructions = ignoreObstructions,
      ySnapStep = ySnapStep,
      minHeightDifference = minHeightDifference,
      lockedYStackStep = lockedYStackStep,
      alignToSurfaceNormal = alignToSurfaceNormal,
      snapRotationY = snapRotationY,
      rotationStep = rotationStep,
      baseRotationY = baseRotationY,
      randomRotationYRange = randomRotationYRange,
      scaleRange = scaleRange,
      brushRadius = brushRadius,
      paintOnDrag = paintOnDrag,
      eraseMode = eraseMode,
      eraseRadius = eraseRadius,
      paintMode = paintMode,
      fallbackToLockedPlane = fallbackToLockedPlane,
      usePrefabRotation = usePrefabRotation,
      parentToSelection = parentToSelection,
      ghostColor = ghostColor,
      brushColor = brushColor
    };

    string json = JsonUtility.ToJson(data);
    EditorPrefs.SetString(prefsSettingsKey, json);
  }

  private void LoadSettings()
  {
    if (!EditorPrefs.HasKey(prefsSettingsKey))
      return;

    string json = EditorPrefs.GetString(prefsSettingsKey, string.Empty);
    if (string.IsNullOrEmpty(json))
      return;

    PainterSettings data = JsonUtility.FromJson<PainterSettings>(json);

    gridSize = data.gridSize;
    lockY = data.lockY;
    lockedY = data.lockedY;
    placementOffset = data.placementOffset;
    ignoreObstructions = data.ignoreObstructions;
    ySnapStep = data.ySnapStep;
    minHeightDifference = data.minHeightDifference;
    lockedYStackStep = data.lockedYStackStep;
    alignToSurfaceNormal = data.alignToSurfaceNormal;
    snapRotationY = data.snapRotationY;
    rotationStep = data.rotationStep;
    baseRotationY = data.baseRotationY;
    randomRotationYRange = data.randomRotationYRange;
    scaleRange = data.scaleRange;
    brushRadius = data.brushRadius;
    paintOnDrag = data.paintOnDrag;
    eraseMode = data.eraseMode;
    eraseRadius = data.eraseRadius;
    paintMode = data.paintMode;
    fallbackToLockedPlane = data.fallbackToLockedPlane;
    usePrefabRotation = data.usePrefabRotation;
    parentToSelection = data.parentToSelection;
    ghostColor = data.ghostColor;
    brushColor = data.brushColor;
  }

  private Vector3Int WorldToCell(Vector3 pos)
  {
    Vector3 p = pos - placementOffset;
    int x = Mathf.RoundToInt(p.x / gridSize);
    int z = Mathf.RoundToInt(p.z / gridSize);

    int y;
    if (lockY)
    {
      float step = Mathf.Max(lockedYStackStep, minHeightDifference);
      y = Mathf.RoundToInt(p.y / step);
    }
    else
    {
      float step = Mathf.Max(ySnapStep, minHeightDifference);
      y = Mathf.RoundToInt(p.y / step);
    }
    return new Vector3Int(x, y, z);
  }

  private Vector3 CellToWorld(Vector3Int cell)
  {
    float yPos;
    if (lockY)
    {
      float step = Mathf.Max(lockedYStackStep, minHeightDifference);
      yPos = cell.y * step;
    }
    else
    {
      float step = Mathf.Max(ySnapStep, minHeightDifference);
      yPos = cell.y * step;
    }
    return new Vector3(cell.x * gridSize, yPos, cell.z * gridSize) + placementOffset;
  }

  private void ApplyRectangle(Quaternion baseRot)
  {
    Vector3 a = rectStartWorld;
    Vector3 b = rectEndWorld;

    float minX = Mathf.Min(a.x, b.x);
    float maxX = Mathf.Max(a.x, b.x);
    float minZ = Mathf.Min(a.z, b.z);
    float maxZ = Mathf.Max(a.z, b.z);

    for (float x = minX; x <= maxX + 0.001f; x += gridSize)
    {
      for (float z = minZ; z <= maxZ + 0.001f; z += gridSize)
      {
        Vector3 pos = new Vector3(x, lockY ? lockedY : a.y, z);
        SnapToGrid(ref pos);
        PlaceAt(pos, baseRot, bypassSpacing: true, bypassRadius: true);
      }
    }
  }

  private void DrawRectanglePreview(Vector3 currentPos)
  {
    Vector3 a = rectStartWorld;
    Vector3 b = currentPos;
    Vector3 c = new Vector3(a.x, a.y, b.z);
    Vector3 d = new Vector3(b.x, a.y, a.z);
    Handles.DrawPolyLine(a, c, b, d, a);
  }

  private void RebuildPaintedLookup()
  {
    paintedLookup.Clear();
    foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
    {
      Traverse(root.transform);
    }

    void Traverse(Transform t)
    {
      if (t.name.StartsWith(paintedPrefix))
      {
        paintedLookup[WorldToCell(t.position)] = t.gameObject;
      }

      for (int i = 0; i < t.childCount; i++)
      {
        Traverse(t.GetChild(i));
      }
    }
  }

  private void OnUndoRedo()
  {
    RebuildPaintedLookup();
    SceneView.RepaintAll();
  }
}
