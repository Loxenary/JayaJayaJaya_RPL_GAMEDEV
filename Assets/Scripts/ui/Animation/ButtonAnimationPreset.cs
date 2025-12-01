using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "ButtonAnimationPreset", menuName = "Config/UI/Button Animation Preset")]
public class ButtonAnimationPreset : ScriptableObject
{
    [Header("Hover Animation")]
    [Tooltip("Enable hover animation when pointer enters the button")]
    public bool enableHover = true;

    [Tooltip("Target scale when hovering (e.g., 1.1 = 110% size)")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);

    [Tooltip("Duration of the hover animation in seconds")]
    public float hoverDuration = 0.2f;

    [Tooltip("Easing function for hover animation (OutBack gives a bouncy feel)")]
    public Ease hoverEase = Ease.OutBack;

[Tooltip("Rotation offset when hovering (e.g., Vector3(0, 0, 5) rotates 5 degrees on Z axis)")]
    public Vector3 hoverRotation = Vector3.zero;

    [Tooltip("Position offset when hovering (relative to original position)")]
    public Vector3 hoverPosition = Vector3.zero;

    [Header("Click Animation")]
    [Tooltip("Enable click animation when button is pressed")]
    public bool enableClick = true;

    [Tooltip("Target scale when clicking (e.g., 0.9 = 90% size for a squish effect)")]
    public Vector3 clickScale = new Vector3(0.9f, 0.9f, 0.9f);

    [Tooltip("Duration of the click animation in seconds")]
    public float clickDuration = 0.1f;

    [Tooltip("Easing function for click animation")]
    public Ease clickEase = Ease.InOutQuad;

    [Tooltip("Rotation offset when clicking")]
    public Vector3 clickRotation = Vector3.zero;

    [Tooltip("Use punch effect instead of scale (creates elastic bouncy effect)")]
    public bool usePunchOnClick = false;

    [Tooltip("Punch effect strength (how much it bounces, typically 0.1-0.5)")]
    [Range(0.1f, 0.5f)]
    public float punchStrength = 0.2f;

    [Tooltip("Punch vibration count (how many times it oscillates, typically 5-10)")]
    [Range(5, 10f)]
    public int punchVibrato = 5;

    [Tooltip("Punch elasticity (how springy the bounce is, 0=no spring, 1=very springy)")]
    [Range(0f, 1f)]
    public float punchElasticity = 0.5f;

    [Header("Fade Effects")]
    [Tooltip("Enable opacity fade on hover (requires CanvasGroup component)")]
    public bool enableFadeOnHover = false;

    [Tooltip("Target opacity when hovering (0=invisible, 1=fully visible)")]
    public float hoverAlpha = 1f;

    [Tooltip("Enable opacity fade on click (requires CanvasGroup component)")]
    public bool enableFadeOnClick = false;

    [Tooltip("Target opacity when clicking (0=invisible, 1=fully visible)")]
    public float clickAlpha = 0.8f;

    [Header("Color Tint")]
    [Tooltip("Enable color tinting on hover/click (requires Image component)")]
    public bool enableColorTint = false;

    [Tooltip("Target color when hovering")]
    public Color hoverColor = Color.white;

    [Tooltip("Target color when clicking")]
    public Color clickColor = Color.gray;

    [Header("Audio")]
    [Tooltip("Sound effect to play when hovering over the button")]
    public SfxClipData hoverSFX;

    [Tooltip("Sound effect to play when clicking the button")]
    public SfxClipData clickSFX;
}
