using UnityEngine;

/// <summary>
/// Visual placeholder untuk ghost sebelum ada model 3D
/// Menampilkan capsule dengan warna yang berubah berdasarkan state
/// </summary>
[RequireComponent(typeof(GhostAI))]
public class GhostVisualPlaceholder : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private bool createVisualOnStart = true;
    [SerializeField] private float ghostHeight = 2f;
    [SerializeField] private float ghostRadius = 0.5f;

    [Header("State Colors")]
    [SerializeField] private Color idleColor = Color.gray;
    [SerializeField] private Color patrolColor = Color.blue;
    [SerializeField] private Color chaseColor = Color.yellow;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color stunnedColor = Color.green;

    [Header("Sanity-Based Glow")]
    [SerializeField] private bool enableSanityGlow = true;
    [SerializeField] private Color highSanityGlow = new Color(0.5f, 0.5f, 1f, 0.3f);
    [SerializeField] private Color criticalSanityGlow = new Color(1f, 0f, 0f, 0.8f);

    private GhostAI ghostAI;
    private GameObject bodyObject;
    private GameObject glowObject;
    private Renderer bodyRenderer;
    private Renderer glowRenderer;
    private Material bodyMaterial;
    private Material glowMaterial;

    private void Start()
    {
        ghostAI = GetComponent<GhostAI>();

        if (createVisualOnStart)
        {
            CreateVisualRepresentation();
        }

        // Subscribe to state changes
        if (ghostAI != null)
        {
            ghostAI.OnStateChanged += OnGhostStateChanged;
        }
    }

    private void Update()
    {
        UpdateSanityBasedEffects();
        UpdateRotationAnimation();
    }

    private void CreateVisualRepresentation()
    {
        // Create body
        bodyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyObject.name = "GhostBody";
        bodyObject.transform.SetParent(transform);
        bodyObject.transform.localPosition = Vector3.up * (ghostHeight / 2f);
        bodyObject.transform.localScale = new Vector3(ghostRadius * 2f, ghostHeight / 2f, ghostRadius * 2f);

        // Remove collider dari visual (ghost AI akan handle collision)
        var collider = bodyObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        bodyRenderer = bodyObject.GetComponent<Renderer>();
        bodyMaterial = new Material(Shader.Find("Standard"));
        bodyMaterial.color = idleColor;
        bodyRenderer.material = bodyMaterial;

        // Create glow sphere
        if (enableSanityGlow)
        {
            glowObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glowObject.name = "GhostGlow";
            glowObject.transform.SetParent(transform);
            glowObject.transform.localPosition = Vector3.up * (ghostHeight / 2f);
            glowObject.transform.localScale = Vector3.one * (ghostRadius * 3f);

            // Remove collider
            var glowCollider = glowObject.GetComponent<Collider>();
            if (glowCollider != null)
            {
                Destroy(glowCollider);
            }

            glowRenderer = glowObject.GetComponent<Renderer>();
            glowMaterial = new Material(Shader.Find("Standard"));
            glowMaterial.SetFloat("_Mode", 3); // Transparent
            glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            glowMaterial.SetInt("_ZWrite", 0);
            glowMaterial.DisableKeyword("_ALPHATEST_ON");
            glowMaterial.EnableKeyword("_ALPHABLEND_ON");
            glowMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            glowMaterial.renderQueue = 3000;
            glowMaterial.color = highSanityGlow;
            glowRenderer.material = glowMaterial;
        }

        // Add eyes
        CreateEyes();
    }

    private void CreateEyes()
    {
        // Left eye
        GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEye.name = "LeftEye";
        leftEye.transform.SetParent(bodyObject.transform);
        leftEye.transform.localPosition = new Vector3(-0.15f, 0.3f, 0.45f);
        leftEye.transform.localScale = Vector3.one * 0.1f;

        var leftEyeCollider = leftEye.GetComponent<Collider>();
        if (leftEyeCollider != null) Destroy(leftEyeCollider);

        var leftEyeRenderer = leftEye.GetComponent<Renderer>();
        leftEyeRenderer.material.color = Color.red;
        leftEyeRenderer.material.SetColor("_EmissionColor", Color.red);
        leftEyeRenderer.material.EnableKeyword("_EMISSION");

        // Right eye
        GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEye.name = "RightEye";
        rightEye.transform.SetParent(bodyObject.transform);
        rightEye.transform.localPosition = new Vector3(0.15f, 0.3f, 0.45f);
        rightEye.transform.localScale = Vector3.one * 0.1f;

        var rightEyeCollider = rightEye.GetComponent<Collider>();
        if (rightEyeCollider != null) Destroy(rightEyeCollider);

        var rightEyeRenderer = rightEye.GetComponent<Renderer>();
        rightEyeRenderer.material.color = Color.red;
        rightEyeRenderer.material.SetColor("_EmissionColor", Color.red);
        rightEyeRenderer.material.EnableKeyword("_EMISSION");
    }

    private void OnGhostStateChanged(GhostAI.GhostState newState)
    {
        if (bodyMaterial == null) return;

        Color targetColor = idleColor;

        switch (newState)
        {
            case GhostAI.GhostState.Idle:
                targetColor = idleColor;
                break;
            case GhostAI.GhostState.Patrol:
                targetColor = patrolColor;
                break;
            case GhostAI.GhostState.Chase:
                targetColor = chaseColor;
                break;
            case GhostAI.GhostState.Attack:
                targetColor = attackColor;
                break;
            case GhostAI.GhostState.Stunned:
                targetColor = stunnedColor;
                break;
        }

        bodyMaterial.color = targetColor;
    }

    private void UpdateSanityBasedEffects()
    {
        if (!enableSanityGlow || glowMaterial == null) return;

        // Find player sanity
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var playerSanity = player.GetComponent<PlayerSanity>();
        if (playerSanity == null) return;

        // Lerp glow color based on sanity
        float sanityPercentage = playerSanity.SanityPercentage;
        Color targetGlowColor = Color.Lerp(criticalSanityGlow, highSanityGlow, sanityPercentage);
        glowMaterial.color = Color.Lerp(glowMaterial.color, targetGlowColor, Time.deltaTime * 2f);

        // Pulse effect ketika sanity critical
        if (playerSanity.CurrentSanityLevel == PlayerSanity.SanityLevel.Critical)
        {
            float pulse = Mathf.Sin(Time.time * 5f) * 0.3f + 0.7f;
            if (glowObject != null)
            {
                glowObject.transform.localScale = Vector3.one * (ghostRadius * 3f * pulse);
            }
        }
        else
        {
            if (glowObject != null)
            {
                glowObject.transform.localScale = Vector3.Lerp(
                    glowObject.transform.localScale,
                    Vector3.one * (ghostRadius * 3f),
                    Time.deltaTime * 2f
                );
            }
        }
    }

    private void UpdateRotationAnimation()
    {
        // Slow rotation untuk idle/patrol
        if (ghostAI != null && (ghostAI.CurrentState == GhostAI.GhostState.Idle ||
                                 ghostAI.CurrentState == GhostAI.GhostState.Patrol))
        {
            if (bodyObject != null)
            {
                bodyObject.transform.Rotate(Vector3.up, 30f * Time.deltaTime);
            }
        }
    }

    private void OnDestroy()
    {
        if (ghostAI != null)
        {
            ghostAI.OnStateChanged -= OnGhostStateChanged;
        }

        if (bodyMaterial != null) Destroy(bodyMaterial);
        if (glowMaterial != null) Destroy(glowMaterial);
    }
}
