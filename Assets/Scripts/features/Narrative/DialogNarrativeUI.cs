using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class DialogNarrativeUI : FadeShowHideProcedural
{
    [SerializeField] private TextMeshProUGUI contentTextDialog;

    [Header("Configuraiton")]
    [SerializeField] private float characterRevealSpeed = 0.5f;
    [SerializeField] private SfxClipData typingSfx;
    [SerializeField] private float characterRevealWaitTime = 1f;
    [SerializeField] private AudioSource audioSourcesForTypingSound;

    public struct OpenDialogNarrtiveUI
    {
        public string content;
    }

    protected override void OnEnable()
    {
        EventBus.Subscribe<OpenDialogNarrtiveUI>(OnOpenDialogNarrtiveUI);
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        EventBus.Unsubscribe<OpenDialogNarrtiveUI>(OnOpenDialogNarrtiveUI);
        base.OnDisable();
    }

    private void OnOpenDialogNarrtiveUI(OpenDialogNarrtiveUI evt)
    {
        Assert.IsNotNull(contentTextDialog, "Content Dialog null for Dialog Narrative UI");
        Assert.IsNotNull(typingSfx, "Typing SFX aren't configured for Dialog Narrative UI");
        Assert.IsNotNull(audioSourcesForTypingSound, "Audio Sources aren't configured for Dialog Narrative UI");

        var audioManager = ServiceLocator.Get<AudioManager>();
        Assert.IsNotNull(audioManager, "Audio Manager isn't configured for Dialog Narrative UI");

        // Show the UI
        ShowUI();

        // Reveal the text with typing animation
        StartCoroutine(TextAnimationHelper.RevealTextWithTypingSound(contentTextDialog, evt.content, characterRevealSpeed, typingSfx, audioSourcesForTypingSound, audioManager, 2, OnDialogFinishedShowing));
    }

    protected virtual void OnDialogFinishedShowing()
    {
        StartCoroutine(WaitForDialogToFinishShowing());
    }

    private IEnumerator WaitForDialogToFinishShowing()
    {
        yield return new WaitForSeconds(characterRevealWaitTime);
        HideUI();
    }


    private void Awake()
    {
        HideUI();
    }
}