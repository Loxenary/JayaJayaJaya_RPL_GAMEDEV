using UnityEngine;

/// <summary>
/// Helper script to quickly set up a Ghost GameObject in the scene.
/// Can be used to create ghosts at runtime or setup in editor.
/// </summary>
public class GhostSetupHelper : MonoBehaviour
{
    [Header("Ghost Prefab Setup")]
    [Tooltip("If assigned, this will instantiate the ghost prefab. Otherwise creates from scratch.")]
    [SerializeField] private GameObject ghostPrefab;

    [Header("Create Ghost Settings")]
    [SerializeField] private bool createOnStart = false;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private float fearDamageValue = 10f;

    private GameObject currentGhost;

    private void Start()
    {
        if (createOnStart)
        {
            CreateGhost();
        }
    }

    /// <summary>
    /// Create a new ghost in the scene
    /// </summary>
    [ContextMenu("Create Ghost")]
    public GameObject CreateGhost()
    {
        GameObject ghost;

        if (ghostPrefab != null)
        {
            // Use prefab if available
            ghost = Instantiate(ghostPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // Create from scratch
            ghost = CreateGhostFromScratch(spawnPosition);
        }

        // Move ghost to same scene as this helper
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ghost, gameObject.scene);

        currentGhost = ghost;
        return ghost;
    }

    /// <summary>
    /// Create a ghost GameObject from scratch with all necessary components
    /// </summary>
    /// <param name="position">Position to spawn the ghost</param>
    /// <param name="targetScene">Optional: Scene to place the ghost in. If null, uses active scene</param>
    public static GameObject CreateGhostFromScratch(Vector3 position, UnityEngine.SceneManagement.Scene? targetScene = null)
    {
        // Create main ghost object
        GameObject ghost = new GameObject("Ghost");
        ghost.transform.position = position;
        // Don't set tag - Ghost should be untagged (only Player needs "Player" tag)

        // Add visual representation (simple cube for now)
        GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualObj.name = "GhostVisual";
        visualObj.transform.SetParent(ghost.transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualObj.transform.localScale = new Vector3(1f, 2f, 1f); // Make it taller

        // Set material color to make it look ghostly
        Renderer renderer = visualObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.8f, 0.8f, 1f, 0.5f); // Light blue, semi-transparent
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;
        }

        // Remove the collider from visual (we'll use trigger on main object)
        Collider visualCollider = visualObj.GetComponent<Collider>();
        if (visualCollider != null)
        {
            DestroyImmediate(visualCollider);
        }

        // Add collider to main ghost object (as trigger for damage)
        BoxCollider ghostCollider = ghost.AddComponent<BoxCollider>();
        ghostCollider.isTrigger = true;
        ghostCollider.size = new Vector3(1.5f, 2f, 1.5f); // Slightly larger than visual

        // Add Rigidbody for physics interactions
        Rigidbody rb = ghost.AddComponent<Rigidbody>();
        rb.useGravity = false; // Ghost floats
        rb.isKinematic = true; // We control movement via script
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Add Ghost component
        Ghost ghostScript = ghost.AddComponent<Ghost>();

        // Move to target scene if specified
        if (targetScene.HasValue && targetScene.Value.IsValid())
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ghost, targetScene.Value);
        }

        return ghost;
    }

    /// <summary>
    /// Setup the created ghost with configured settings
    /// </summary>
    public void SetupGhost(GameObject ghost)
    {
        Ghost ghostScript = ghost.GetComponent<Ghost>();
        if (ghostScript != null)
        {
            ghostScript.SetActive(true);
        }
    }

    /// <summary>
    /// Destroy the current ghost
    /// </summary>
    [ContextMenu("Destroy Ghost")]
    public void DestroyGhost()
    {
        if (currentGhost != null)
        {
            Destroy(currentGhost);
        }
    }
}
