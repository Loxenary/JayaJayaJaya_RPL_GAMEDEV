using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a crosshair that changes when player can interact with objects.
/// Default: subtle circle with hole. When interactable: hand icon.
/// </summary>
public class CrosshairDisplay : MonoBehaviour
{
    [Header("Crosshair Images")]
    [Tooltip("The default crosshair image (circle with hole)")]
    [SerializeField] private Image defaultCrosshair;

    [Tooltip("The interact crosshair image (hand icon)")]
    [SerializeField] private Image interactCrosshair;

    [Header("Visual Settings")]
    [Tooltip("Color for default crosshair (subtle, not too contrasting)")]
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.4f);

    [Tooltip("Color for interact crosshair")]
    [SerializeField] private Color interactColor = new Color(1f, 1f, 1f, 0.8f);

    [Tooltip("Size of default crosshair")]
    [SerializeField] private float defaultSize = 24f;

    [Tooltip("Size of interact crosshair")]
    [SerializeField] private float interactSize = 32f;

    [Header("Animation")]
    [Tooltip("Enable smooth transition between states")]
    [SerializeField] private bool smoothTransition = true;

    [Tooltip("Transition speed")]
    [SerializeField] private float transitionSpeed = 10f;

    [Header("Pulse Effect (Optional)")]
    [Tooltip("Enable pulse effect when interactable")]
    [SerializeField] private bool enablePulse = true;

    [Tooltip("Pulse speed")]
    [SerializeField] private float pulseSpeed = 2f;

    [Tooltip("Pulse scale range")]
    [SerializeField] private float pulseAmount = 0.1f;

    private bool isInteractMode = false;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;
    private RectTransform defaultRect;
    private RectTransform interactRect;

    private void Awake()
    {
        ValidateComponents();

        if (defaultCrosshair != null)
            defaultRect = defaultCrosshair.GetComponent<RectTransform>();

        if (interactCrosshair != null)
            interactRect = interactCrosshair.GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Initialize to default state
        SetDefaultState();
    }

    private void OnEnable()
    {
        // Subscribe to interact events
        EventBus.Subscribe<InteractEventState>(OnInteractEvent);
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        EventBus.Unsubscribe<InteractEventState>(OnInteractEvent);
    }

    private void Update()
    {
        if (smoothTransition)
        {
            UpdateTransition();
        }

        if (enablePulse && isInteractMode)
        {
            UpdatePulse();
        }
    }

    private void ValidateComponents()
    {
        if (defaultCrosshair == null)
        {
            Debug.LogWarning("[CrosshairDisplay] Default crosshair image is not assigned!", this);
        }

        if (interactCrosshair == null)
        {
            Debug.LogWarning("[CrosshairDisplay] Interact crosshair image is not assigned!", this);
        }
    }

    /// <summary>
    /// Handle interact events from InteractableSelector
    /// </summary>
    private void OnInteractEvent(InteractEventState state)
    {
        switch (state)
        {
            case InteractEventState.OnEnter:
                SetInteractState();
                break;

            case InteractEventState.OnExit:
            case InteractEventState.OnInteract:
                SetDefaultState();
                break;
        }
    }

    /// <summary>
    /// Set crosshair to default state (circle)
    /// </summary>
    public void SetDefaultState()
    {
        isInteractMode = false;
        targetAlpha = 0f;

        if (!smoothTransition)
        {
            ApplyDefaultState();
        }
    }

    /// <summary>
    /// Set crosshair to interact state (hand icon)
    /// </summary>
    public void SetInteractState()
    {
        isInteractMode = true;
        targetAlpha = 1f;

        if (!smoothTransition)
        {
            ApplyInteractState();
        }
    }

    private void ApplyDefaultState()
    {
        if (defaultCrosshair != null)
        {
            defaultCrosshair.gameObject.SetActive(true);
            defaultCrosshair.color = defaultColor;
            if (defaultRect != null)
                defaultRect.sizeDelta = new Vector2(defaultSize, defaultSize);
        }

        if (interactCrosshair != null)
        {
            interactCrosshair.gameObject.SetActive(false);
        }
    }

    private void ApplyInteractState()
    {
        if (defaultCrosshair != null)
        {
            defaultCrosshair.gameObject.SetActive(false);
        }

        if (interactCrosshair != null)
        {
            interactCrosshair.gameObject.SetActive(true);
            interactCrosshair.color = interactColor;
            if (interactRect != null)
                interactRect.sizeDelta = new Vector2(interactSize, interactSize);
        }
    }

    private void UpdateTransition()
    {
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * transitionSpeed);

        // Cross-fade between the two crosshairs
        if (defaultCrosshair != null)
        {
            defaultCrosshair.gameObject.SetActive(true);
            Color defColor = defaultColor;
            defColor.a = defaultColor.a * (1f - currentAlpha);
            defaultCrosshair.color = defColor;

            // Hide completely when fully transitioned
            if (currentAlpha > 0.99f)
                defaultCrosshair.gameObject.SetActive(false);
        }

        if (interactCrosshair != null)
        {
            interactCrosshair.gameObject.SetActive(true);
            Color intColor = interactColor;
            intColor.a = interactColor.a * currentAlpha;
            interactCrosshair.color = intColor;

            // Hide completely when fully transitioned
            if (currentAlpha < 0.01f)
                interactCrosshair.gameObject.SetActive(false);
        }
    }

    private void UpdatePulse()
    {
        if (interactRect == null) return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * pulseAmount;
        float size = interactSize * pulse;
        interactRect.sizeDelta = new Vector2(size, size);
    }

    /// <summary>
    /// Show or hide the entire crosshair system
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// Manually set crosshair sprites (useful for runtime changes)
    /// </summary>
    public void SetSprites(Sprite defaultSprite, Sprite interactSprite)
    {
        if (defaultCrosshair != null && defaultSprite != null)
            defaultCrosshair.sprite = defaultSprite;

        if (interactCrosshair != null && interactSprite != null)
            interactCrosshair.sprite = interactSprite;
    }
}
