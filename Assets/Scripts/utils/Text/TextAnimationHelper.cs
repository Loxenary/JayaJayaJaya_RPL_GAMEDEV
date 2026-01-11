using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnimationHelper
{
    private TextMeshProUGUI _textMesh;

    public TextAnimationHelper(TextMeshProUGUI textMesh)
    {
        _textMesh = textMesh;
    }

    public IEnumerator RevealText(string message, float characterRevealSpeed, Action onFinished = null)
    {
        _textMesh.text = message;
        _textMesh.ForceMeshUpdate();
        yield return null;

        TMP_TextInfo textInfo = _textMesh.textInfo;
        int totalVisibleCharacters = textInfo.characterCount;
        Color32[] newVertexColors;

        // Initialize all characters as invisible
        for (int i = 0; i < totalVisibleCharacters; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            int materialIndex = charInfo.materialReferenceIndex;
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            int vertexIndex = charInfo.vertexIndex;

            for (int v = 0; v < 4; v++)
            {
                Color32 color = newVertexColors[vertexIndex + v];
                color.a = 0;
                newVertexColors[vertexIndex + v] = color;
            }
        }

        _textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Reveal characters one by one
        for (int i = 0; i < totalVisibleCharacters; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            int materialIndex = charInfo.materialReferenceIndex;
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            int vertexIndex = charInfo.vertexIndex;

            for (int v = 0; v < 4; v++)
            {
                Color32 color = newVertexColors[vertexIndex + v];
                color.a = 255;
                newVertexColors[vertexIndex + v] = color;
            }

            _textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            if (characterRevealSpeed > 0)
            {
                yield return new WaitForSeconds(characterRevealSpeed);
            }
        }

        onFinished?.Invoke();
    }

    public IEnumerator RevealTextWithTypingSoundUnscaledTime(
        string message,
        float characterRevealSpeed,
        SfxClipData typingSfx,
        AudioSource typingAudioSource,
        AudioManager audioManager,
        int soundEveryNCharacters = 2,
        Action onFinished = null)
    {
        // 1. Set the text
        _textMesh.text = message;

        // 2. Hide everything immediately using the built-in property.
        // This persists even if Unity rebuilds the layout or regenerates the mesh.
        _textMesh.maxVisibleCharacters = 0;

        // Force update so TMP knows the total character count
        _textMesh.ForceMeshUpdate();

        int totalVisibleCharacters = _textMesh.textInfo.characterCount;
        int visibleCount = 0;
        int soundCounter = 0;

        // 3. Reveal Loop
        while (visibleCount <= totalVisibleCharacters)
        {
            // Set how many characters are visible
            _textMesh.maxVisibleCharacters = visibleCount;

            // Play Sound Logic
            // We check if the *current* character we just revealed is not a space
            if (visibleCount > 0 && visibleCount <= totalVisibleCharacters)
            {
                // Get info for the character we just revealed (index is count - 1)
                var charInfo = _textMesh.textInfo.characterInfo[visibleCount - 1];

                // Only play sound if it's visible and not a space/control char

                char c = charInfo.character;


                if (soundCounter % soundEveryNCharacters == 0 && typingSfx != null && !char.IsWhiteSpace(c) && c != 0)
                {
                    audioManager?.PlaySfx(typingSfx.SFXId, typingAudioSource);
                }
                soundCounter++;

            }

            visibleCount++;

            // Delay
            if (characterRevealSpeed > 0)
            {
                yield return new WaitForSecondsRealtime(characterRevealSpeed);
            }
            else
            {
                // If speed is 0, show all instantly
                _textMesh.maxVisibleCharacters = totalVisibleCharacters;
                break;
            }
        }

        // Ensure all shown at the end
        _textMesh.maxVisibleCharacters = int.MaxValue;
        onFinished?.Invoke();
    }

    /// <summary>
    /// Reveals text character by character with typing sound effect support.
    /// Uses optimized sound playback to prevent performance issues.
    /// </summary>
    public IEnumerator RevealTextWithTypingSound(
     string message,
     float characterRevealSpeed,
     SfxClipData typingSfx,
     AudioSource typingAudioSource,
     AudioManager audioManager,
     int soundEveryNCharacters = 2,
     Action onFinished = null)
    {
        // 1. Set the text
        _textMesh.text = message;

        // 2. Hide everything immediately using the built-in property.
        // This persists even if Unity rebuilds the layout or regenerates the mesh.
        _textMesh.maxVisibleCharacters = 0;

        // Force update so TMP knows the total character count
        _textMesh.ForceMeshUpdate();

        int totalVisibleCharacters = _textMesh.textInfo.characterCount;
        int visibleCount = 0;
        int soundCounter = 0;

        // 3. Reveal Loop
        while (visibleCount <= totalVisibleCharacters)
        {
            // Set how many characters are visible
            _textMesh.maxVisibleCharacters = visibleCount;

            // Play Sound Logic
            // We check if the *current* character we just revealed is not a space
            if (visibleCount > 0 && visibleCount <= totalVisibleCharacters)
            {
                // Get info for the character we just revealed (index is count - 1)
                var charInfo = _textMesh.textInfo.characterInfo[visibleCount - 1];

                // Only play sound if it's visible and not a space/control char


                if (soundCounter % soundEveryNCharacters == 0 && typingSfx != null)
                {
                    audioManager?.PlaySfx(typingSfx.SFXId, typingAudioSource);
                }
                soundCounter++;

            }

            visibleCount++;

            // Delay
            if (characterRevealSpeed > 0)
            {
                yield return new WaitForSeconds(characterRevealSpeed);
            }
            else
            {
                // If speed is 0, show all instantly
                _textMesh.maxVisibleCharacters = totalVisibleCharacters;
                break;
            }
        }

        // Ensure all shown at the end
        _textMesh.maxVisibleCharacters = int.MaxValue;
        onFinished?.Invoke();
    }

    /// <summary>
    /// Animates the text by fading it in over a specified duration.
    /// </summary>
    public IEnumerator FadeInText(string message, float duration, Action onFinished = null)
    {
        _textMesh.text = message;

        _textMesh.alpha = 0f;

        if (duration <= 0)
        {
            _textMesh.alpha = 1f;
            onFinished?.Invoke();
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float newAlpha = Mathf.Clamp01(elapsedTime / duration);
            _textMesh.alpha = newAlpha;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _textMesh.alpha = 1f;

        onFinished?.Invoke();
    }

    /// <summary>
    /// Instantly reveals the entire text message by setting all character alphas to 255.
    /// </summary>
    public void RevealAllTextInstantly()
    {
        _textMesh.maxVisibleCharacters = int.MaxValue;
        _textMesh.alpha = 1f;
    }

}
