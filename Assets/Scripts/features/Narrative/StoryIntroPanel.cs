using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Full-screen narrative intro panel: shows a story's intro paragraphs with a
/// typewriter effect. Click/space advances (or reveals the paragraph instantly
/// while typing), Esc skips the whole intro.
///
/// Deliberately independent from DialogNarrativeUI and its OpenDialogNarrtiveUI
/// event, which belongs to the in-game journal/puzzle narration (NarrativeSystem) -
/// sharing that event would make journal pickups pop this panel open.
///
/// The same prefab is used in the gameplay UI scene (first-visit intro, driven by
/// StoryIntroController) and in the world selection scene (replay from the map).
/// </summary>
public class StoryIntroPanel : FadeShowHideProcedural
{
    [Header("Story Intro Section")]
    [SerializeField] private TextMeshProUGUI narrativeText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private float characterRevealSpeed = 0.03f;
    [SerializeField] private SfxClipData typingSfx;
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private int soundEveryNCharacters = 2;

    private string[] _paragraphs;
    private int _index;
    private Action _onDone;
    private bool _isPlaying;
    private bool _isTyping;
    private Coroutine _typingRoutine;

    /// <summary>Starts the intro. onDone fires once, on finish or skip.</summary>
    public void Play(string[] paragraphs, Action onDone)
    {
        if (_isPlaying || paragraphs == null || paragraphs.Length == 0)
        {
            return;
        }

        _paragraphs = paragraphs;
        _index = 0;
        _onDone = onDone;
        _isPlaying = true;

        if (hintText != null)
        {
            hintText.text = "Klik untuk lanjut  ·  [Esc] Lewati";
        }

        ShowUI();
        StartParagraph();
    }

    private void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Finish();
            return;
        }

        bool advancePressed =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame));

        if (!advancePressed)
        {
            return;
        }

        if (_isTyping)
        {
            RevealCurrentParagraphInstantly();
        }
        else if (_index + 1 < _paragraphs.Length)
        {
            _index++;
            StartParagraph();
        }
        else
        {
            Finish();
        }
    }

    private void StartParagraph()
    {
        StopTyping();
        _isTyping = true;

        _typingRoutine = StartCoroutine(TextAnimationHelper.RevealTextWithTypingSound(
            narrativeText,
            _paragraphs[_index],
            characterRevealSpeed,
            typingSfx,
            typingAudioSource,
            ServiceLocator.Get<AudioManager>(),
            soundEveryNCharacters,
            onFinished: () => _isTyping = false));
    }

    private void RevealCurrentParagraphInstantly()
    {
        StopTyping();

        // Re-assigning the text rebuilds the TMP mesh with full alpha, undoing the
        // per-vertex hiding done by the reveal coroutine.
        narrativeText.text = string.Empty;
        narrativeText.text = _paragraphs[_index];
        narrativeText.ForceMeshUpdate();
    }

    private void StopTyping()
    {
        if (_typingRoutine != null)
        {
            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }
        _isTyping = false;
    }

    private void Finish()
    {
        if (!_isPlaying)
        {
            return;
        }

        StopTyping();
        _isPlaying = false;
        HideUI();

        var onDone = _onDone;
        _onDone = null;
        onDone?.Invoke();
    }
}
