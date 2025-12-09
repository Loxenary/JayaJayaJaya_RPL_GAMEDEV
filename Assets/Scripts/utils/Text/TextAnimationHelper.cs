using System;
using System.Collections;
using TMPro;
using UnityEngine;

public static class TextAnimationHelper
{
    public static IEnumerator RevealText(TextMeshProUGUI textMesh, string message, float characterRevealSpeed, Action onFinished = null)
    {
        textMesh.text = message;
        textMesh.ForceMeshUpdate();
        yield return null;

        TMP_TextInfo textInfo = textMesh.textInfo;
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

        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

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

            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            if (characterRevealSpeed > 0)
            {
                yield return new WaitForSeconds(characterRevealSpeed);
            }
        }

        onFinished?.Invoke();
    }

    /// <summary>
    /// Reveals text character by character with typing sound effect support.
    /// Uses optimized sound playback to prevent performance issues.
    /// </summary>
    public static IEnumerator RevealTextWithTypingSound(
        TextMeshProUGUI textMesh,
        string message,
        float characterRevealSpeed,
        SfxClipData typingSfx,
        AudioSource typingAudioSource,
        AudioManager audioManager,
        int soundEveryNCharacters = 2,
        Action onFinished = null)
    {
        textMesh.text = message;
        textMesh.ForceMeshUpdate();
        yield return null;

        TMP_TextInfo textInfo = textMesh.textInfo;
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

        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Reveal characters one by one with optimized sound playback
        int soundCounter = 0;
        for (int i = 0; i < totalVisibleCharacters; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            int materialIndex = charInfo.materialReferenceIndex;
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            int vertexIndex = charInfo.vertexIndex;

            // Reveal this character by setting alpha to 255
            for (int v = 0; v < 4; v++)
            {
                Color32 color = newVertexColors[vertexIndex + v];
                color.a = 255;
                newVertexColors[vertexIndex + v] = color;
            }

            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // Play typing sound at intervals to prevent performance issues
            // Only play for non-whitespace characters
            char currentChar = message[i];
            bool isWhitespace = char.IsWhiteSpace(currentChar);

            if (!isWhitespace && typingSfx != null && audioManager != null && typingAudioSource != null)
            {
                if (soundCounter % soundEveryNCharacters == 0)
                {
                    // Use PlayOneShot for better performance
                    audioManager.PlaySfx(typingSfx.SFXId, typingAudioSource);
                }
                soundCounter++;
            }

            if (characterRevealSpeed > 0)
            {
                yield return new WaitForSecondsRealtime(characterRevealSpeed);
            }
        }

        onFinished?.Invoke();
    }


    /// <summary>
    /// Animates the text by fading it in over a specified duration.
    /// </summary>
    public static IEnumerator FadeInText(TextMeshProUGUI textMesh, string message, float duration, Action onFinished = null)
    {
        textMesh.text = message;

        textMesh.alpha = 0f;

        if (duration <= 0)
        {
            textMesh.alpha = 1f;
            onFinished?.Invoke();
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float newAlpha = Mathf.Clamp01(elapsedTime / duration);
            textMesh.alpha = newAlpha;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textMesh.alpha = 1f;

        onFinished?.Invoke();
    }

    /// <summary>
    /// Instantly reveals the entire text message by setting all character alphas to 255.
    /// </summary>
    public static void RevealAllTextInstantly(TextMeshProUGUI textMesh)
    {
        textMesh.maxVisibleCharacters = int.MaxValue;
        textMesh.alpha = 1f;
    }
}
