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
  [SerializeField] private bool alignToSurfaceNormal = false;
  [SerializeField] private bool snapRotationY = true;
  [SerializeField] private float rotationStep = 90f;
  [SerializeField] private float baseRotationY = 0f;
  [SerializeField] private Vector2 randomRotationYRange = new Vector2(0f, 0f);
  [SerializeField] private Vector2 scaleRange = Vector2.one;

  [Header("Brush")]
  [SerializeField] private float brushRadius = 0.5f;
  [SerializeField] private bool paintOnDrag = true;
  [SerializeField] private bool eraseMode = false;
  [SerializeField] private float eraseRadius = 0.6f;

  [Header("Visuals")]
  [SerializeField] private Color ghostColor = new Color(0f, 1f, 1f, 0.35f);
  [SerializeField] private Color brushColor = new Color(0f, 1f, 1f, 0.35f);
  [SerializeField] private bool fallbackToLockedPlane = true; // allow preview even when ray misses colliders

  private GameObject ghostInstance;
  private Vector3 lastPlacedPos;
  private bool hasLastPos;
  private const float occupancyTolerance = 0.2f;
  private const string paintedPrefix = "[Painted] ";
  private static readonly Dictionary<Vector3Int, GameObject> paintedLookup = new Dictionary<Vector3Int, GameObject>();

  [MenuItem("Tools/Advanced Tile Painter")]
  public static void ShowWindow() => GetWindow<AdvancedTilePainter>("Tile Painter");

  private void OnEnable()
  {
    SceneView.duringSceneGui += OnSceneGUI;
    EnsureGhost();
    wantsMouseMove = true; // keep preview responsive while hovering
    RebuildPaintedLookup();
  }

  private void OnDisable()
  {
    SceneView.duringSceneGui -= OnSceneGUI;
    DestroyImmediate(ghostInstance);
    paintedLookup.Clear();
  }

  private void OnGUI()
  {
    EditorGUILayout.LabelField("Palette", EditorStyles.boldLabel);
    int removeIndex = -1;
    for (int i = 0; i < palette.Count; i++)
    {
      EditorGUILayout.BeginHorizontal();
      palette[i] = (GameObject)EditorGUILayout.ObjectField(palette[i], typeof(GameObject), false);
      if (GUILayout.Toggle(selectedIndex == i, "Use", GUILayout.Width(50))) selectedIndex = i;
      if (GUILayout.Button("X", GUILayout.Width(24))) removeIndex = i;
      EditorGUILayout.EndHorizontal();
    }
    if (removeIndex >= 0 && removeIndex < palette.Count)
    {
      palette.RemoveAt(removeIndex);
      selectedIndex = Mathf.Clamp(selectedIndex, 0, palette.Count - 1);
    }
    if (GUILayout.Button("+ Add Prefab")) palette.Add(null);

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
    gridSize = Mathf.Max(0.01f, EditorGUILayout.FloatField("Grid Size", gridSize));
    lockY = EditorGUILayout.Toggle("Lock Y", lockY);
    using (new EditorGUI.DisabledScope(!lockY))
    {
      lockedY = EditorGUILayout.FloatField("Locked Y", lockedY);
    }
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
    EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
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
  }

  private void OnSceneGUI(SceneView sceneView)
  {
    if (palette.Count == 0 || selectedIndex < 0 || selectedIndex >= palette.Count || palette[selectedIndex] == null)
    {
      ClearGhost();
      return;
    }

    EnsureGhost();

    Event e = Event.current;
    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
    bool hasHit = Physics.Raycast(ray, out RaycastHit hitInfo, 500f);

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

    if (!hasHit)
    {
      ClearGhost();
      return;
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

    // Brush visuals
    Handles.color = brushColor;
    Handles.DrawWireDisc(targetPos, Vector3.up, brushRadius);

    // Interaction
    bool leftClick = e.type == EventType.MouseDown && e.button == 0 && !e.alt;
    bool dragPaint = paintOnDrag && e.type == EventType.MouseDrag && e.button == 0 && !e.alt;
    bool action = leftClick || dragPaint;

    if (action)
    {
      if (eraseMode)
      {
        EraseAt(targetPos);
      }
      else
      {
        PlaceAt(targetPos, targetRot);
      }
      e.Use();
    }
    else if (e.type == EventType.MouseMove)
    {
      SceneView.RepaintAll(); // keep ghost following cursor smoothly
    }
  }

  private void SnapToGrid(ref Vector3 pos)
  {
    pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
    pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
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

  private void PlaceAt(Vector3 pos, Quaternion rot)
  {
    if (palette.Count == 0 || selectedIndex < 0 || selectedIndex >= palette.Count) return;
    GameObject prefab = palette[selectedIndex];
    if (prefab == null) return;

    // Avoid double placement at the same spot when dragging quickly
    if (hasLastPos && Vector3.Distance(lastPlacedPos, pos) < Mathf.Max(0.05f, gridSize * 0.25f))
      return;

    // Skip if a painted tile already occupies this grid cell
    Vector3Int cell = WorldToCell(pos);
    if (paintedLookup.ContainsKey(cell))
      return;

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
      if (Vector3.Distance(CellToWorld(kvp.Key), pos) <= eraseRadius)
      {
        if (kvp.Value != null)
        {
          Undo.DestroyObjectImmediate(kvp.Value);
        }
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

  private Vector3Int WorldToCell(Vector3 pos)
  {
    int x = Mathf.RoundToInt(pos.x / gridSize);
    int y = lockY ? Mathf.RoundToInt(lockedY / Mathf.Max(0.0001f, gridSize)) : Mathf.RoundToInt(pos.y / gridSize);
    int z = Mathf.RoundToInt(pos.z / gridSize);
    return new Vector3Int(x, y, z);
  }

  private Vector3 CellToWorld(Vector3Int cell)
  {
    return new Vector3(cell.x * gridSize, (lockY ? lockedY : cell.y * gridSize), cell.z * gridSize);
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
}
