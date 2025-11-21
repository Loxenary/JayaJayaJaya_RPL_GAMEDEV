using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SimpleButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Preset")]
    [SerializeField] private ButtonAnimationPreset preset;

    [Header("References")]
    [Tooltip("If not defined, the button will be used as the target object")]
    [SerializeField] private GameObject targetObject;

    [Tooltip("If not defined, the button will be used as the canvas group")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Tooltip("If not defined, the button will be used as the image")]
    [SerializeField] private Image targetImage;

    private Button _button;
    private AudioManager _audioManager;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Color _originalColor;
    private Sequence _currentSequence;
    private bool _isHovered;
    private bool _isPressed;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _audioManager = ServiceLocator.Get<AudioManager>();

        if (targetObject == null)
            targetObject = gameObject;

        if (canvasGroup == null)
            canvasGroup = targetObject.GetComponent<CanvasGroup>();

        if (targetImage == null)
            targetImage = targetObject.GetComponent<Image>();

        _originalScale = targetObject.transform.localScale;
        _originalPosition = targetObject.transform.localPosition;
        _originalRotation = targetObject.transform.localRotation;

        if (targetImage != null)
            _originalColor = targetImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable || preset == null || !preset.enableHover) return;

        _isHovered = true;
        KillCurrentAnimation();

        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(targetObject.transform.DOScale(preset.hoverScale, preset.hoverDuration).SetEase(preset.hoverEase));

        if (preset.hoverRotation != Vector3.zero)
            _currentSequence.Join(targetObject.transform.DOLocalRotate(preset.hoverRotation, preset.hoverDuration).SetEase(preset.hoverEase));

        if (preset.hoverPosition != Vector3.zero)
            _currentSequence.Join(targetObject.transform.DOLocalMove(_originalPosition + preset.hoverPosition, preset.hoverDuration).SetEase(preset.hoverEase));

        if (preset.enableFadeOnHover && canvasGroup != null)
            _currentSequence.Join(canvasGroup.DOFade(preset.hoverAlpha, preset.hoverDuration));

        if (preset.enableColorTint && targetImage != null)
            _currentSequence.Join(targetImage.DOColor(preset.hoverColor, preset.hoverDuration));

        _currentSequence.SetUpdate(true);

        if (_audioManager != null && preset.hoverSFX != null)
            _audioManager.PlaySfx(preset.hoverSFX.SFXId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        _isHovered = false;
        if (_isPressed) return;

        ResetToOriginal();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable || preset == null || !preset.enableClick) return;

        _isPressed = true;
        KillCurrentAnimation();

        _currentSequence = DOTween.Sequence();

        if (preset.usePunchOnClick)
        {
            _currentSequence.Append(targetObject.transform.DOPunchScale(Vector3.one * preset.punchStrength, preset.clickDuration, preset.punchVibrato, preset.punchElasticity));
        }
        else
        {
            _currentSequence.Append(targetObject.transform.DOScale(preset.clickScale, preset.clickDuration).SetEase(preset.clickEase));
        }

        if (preset.clickRotation != Vector3.zero)
            _currentSequence.Join(targetObject.transform.DOLocalRotate(preset.clickRotation, preset.clickDuration).SetEase(preset.clickEase));

        if (preset.enableFadeOnClick && canvasGroup != null)
            _currentSequence.Join(canvasGroup.DOFade(preset.clickAlpha, preset.clickDuration));

        if (preset.enableColorTint && targetImage != null)
            _currentSequence.Join(targetImage.DOColor(preset.clickColor, preset.clickDuration));

        _currentSequence.SetUpdate(true);

        if (_audioManager != null && preset.clickSFX != null)
            _audioManager.PlaySfx(preset.clickSFX.SFXId);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        _isPressed = false;

        if (_isHovered && preset != null)
        {
            KillCurrentAnimation();
            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(targetObject.transform.DOScale(preset.hoverScale, preset.clickDuration).SetEase(Ease.OutBack));
            _currentSequence.SetUpdate(true);
        }
        else
        {
            ResetToOriginal();
        }
    }

    private void ResetToOriginal()
    {
        if (preset == null) return;

        KillCurrentAnimation();
        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(targetObject.transform.DOScale(_originalScale, preset.hoverDuration).SetEase(preset.hoverEase));
        _currentSequence.Join(targetObject.transform.DOLocalMove(_originalPosition, preset.hoverDuration).SetEase(preset.hoverEase));
        _currentSequence.Join(targetObject.transform.DOLocalRotateQuaternion(_originalRotation, preset.hoverDuration).SetEase(preset.hoverEase));

        if (preset.enableFadeOnHover && canvasGroup != null)
            _currentSequence.Join(canvasGroup.DOFade(1f, preset.hoverDuration));

        if (preset.enableColorTint && targetImage != null)
            _currentSequence.Join(targetImage.DOColor(_originalColor, preset.hoverDuration));

        _currentSequence.SetUpdate(true);
    }

    private void KillCurrentAnimation()
    {
        _currentSequence?.Kill();
    }

    private void OnDisable()
    {
        KillCurrentAnimation();
        if (targetObject != null)
        {
            targetObject.transform.localScale = _originalScale;
            targetObject.transform.localPosition = _originalPosition;
            targetObject.transform.localRotation = _originalRotation;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (targetImage != null)
            targetImage.color = _originalColor;
    }
}
