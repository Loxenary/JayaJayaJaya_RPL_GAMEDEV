using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component that displays images.
/// Can be toggled open or closed via EventBus, with no auto-hide duration.
/// </summary>
public class ImageUI : FadeShowHideProcedural
{
    [Header("Image UI Components")]
    [SerializeField] private Image displayImage;
    [SerializeField] private SfxClipData sfxClipData;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus.Subscribe<ImageShown>(OnImageShown);
        EventBus.Subscribe<ImageHidden>(OnImageHidden);
        EventBus.Subscribe<ImageToggled>(OnImageToggled);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus.Unsubscribe<ImageShown>(OnImageShown);
        EventBus.Unsubscribe<ImageHidden>(OnImageHidden);
        EventBus.Unsubscribe<ImageToggled>(OnImageToggled);
    }

    private void OnImageShown(ImageShown evt)
    {
        // Update image sprite
        if (displayImage != null)
        {
            displayImage.sprite = evt.imageSprite;
        }
        else
        {
            Debug.LogWarning("[ImageUI] Display image component is not assigned!", this);
        }

        // Show the UI (stays open until explicitly hidden)
        ShowUI();
    }

    private void OnImageHidden(ImageHidden evt)
    {
        // Hide the UI
        HideUI();
    }

    private void OnImageToggled(ImageToggled evt)
    {
        // Toggle the UI state
        if (IsVisible)
        {
            HideUI();
        }
        else
        {
            // If we have a sprite to show, update it and show
            if (displayImage != null && evt.imageSprite != null)
            {
                displayImage.sprite = evt.imageSprite;
            }
            ShowUI();
        }

    }

    protected override void ShowUIStart()
    {
        base.ShowUIStart();
        ServiceLocator.Get<AudioManager>().PlaySfx(sfxClipData.SFXId);

    }

    protected override void HideUIStart()
    {
        base.HideUIStart();
        ServiceLocator.Get<AudioManager>().PlaySfx(sfxClipData.SFXId);
    }

    private void OnValidate()
    {
        // Auto-find Image component if not assigned
        if (displayImage == null)
        {
            displayImage = GetComponentInChildren<Image>();
        }
    }
}
