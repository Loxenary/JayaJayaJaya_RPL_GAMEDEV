using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private int characterCountToSound = 2;

    // Changed to protected so child classes (MultipleParagraph) can access
    protected bool _isTyping = false;
    protected Queue<string> _guideDatasQueue = new();

    private Coroutine _typingCoroutine;
    protected Coroutine _waitCoroutine;

    public struct OpenDialogNarrtiveUI
    {
        public string content;
    }

    private TextAnimationHelper textAnimationHelper;

    protected override void OnEnable()
    {
        EventBus.Subscribe<OpenDialogNarrtiveUI>(OnOpenDialogNarrtiveUI);
        base.OnEnable();
        textAnimationHelper = new(contentTextDialog);
        _isTyping = false;
        _guideDatasQueue.Clear();
    }

    protected override void OnDisable()
    {
        EventBus.Unsubscribe<OpenDialogNarrtiveUI>(OnOpenDialogNarrtiveUI);
        base.OnDisable();
    }

    private void OnOpenDialogNarrtiveUI(OpenDialogNarrtiveUI evt)
    {
        Assert.IsNotNull(contentTextDialog, "Content Dialog null");

        // 1. ADD TO QUEUE
        _guideDatasQueue.Enqueue(evt.content);

        // 2. CHECK STATE
        // If we are currently typing OR waiting for the next one, 
        // we do nothing. The queue system will pick it up automatically.
        if (_isTyping || _waitCoroutine != null) return;

        // 3. START IF IDLE
        PlayNextDialogInQueue();
    }

    private void PlayNextDialogInQueue()
    {
        if (_guideDatasQueue.Count == 0) return;

        // Peek the content (Do NOT dequeue yet)
        string content = _guideDatasQueue.Peek();

        // Ensure UI is visible
        // If it's already visible, this does nothing (which is what we want!)
        ShowUI();

        _isTyping = true;

        // Safety stop
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

        var audioManager = ServiceLocator.Get<AudioManager>();

        _typingCoroutine = StartCoroutine(textAnimationHelper.RevealTextWithTypingSound(
            content,
            characterRevealSpeed,
            typingSfx,
            audioSourcesForTypingSound,
            audioManager,
            characterCountToSound,
            () =>
            {
                if (this.isActiveAndEnabled) OnDialogFinishedShowing();
            }
        ));
    }

    protected virtual void OnDialogFinishedShowing()
    {
        _isTyping = false;
        // Start the wait timer
        _waitCoroutine = StartCoroutine(WaitForDialogToFinishShowing());
    }

    private IEnumerator WaitForDialogToFinishShowing()
    {
        yield return new WaitForSeconds(characterRevealWaitTime);

        // 1. We are done with the current text. Remove it.
        _guideDatasQueue.Dequeue();
        _waitCoroutine = null;

        // 2. CHECK IF MORE EXIST
        if (_guideDatasQueue.Count > 0)
        {
            // --- THE FIX ---
            // We do NOT call HideUI(). 
            // We do NOT call OnOpenDialogNarrtiveUI().
            // We simply clear the text and play the next one.
            // This keeps the UI Alpha at 1, preventing the animation glitch.
            contentTextDialog.text = "";
            PlayNextDialogInQueue();
        }
        else
        {
            // 3. Only Hide if the queue is actually empty
            HideUI();
            contentTextDialog.text = "";
        }
    }

    private void Awake()
    {
        HideUI();
    }
}