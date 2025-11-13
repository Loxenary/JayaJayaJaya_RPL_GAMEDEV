using UnityEngine;
using UnityEngine.UI; // Required for basic UI Text. For TextMeshPro, use: using TMPro;
using System;
using System.Collections;

/// <summary>
/// Defines the types of easing functions available for animations.
/// </summary>
public enum EasingType
{
    Linear,
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,
    EaseInCubic,
    EaseOutCubic,
    EaseInOutCubic,
    EaseInQuart,
    EaseOutQuart,
    EaseInOutQuart,
    EaseInQuint,
    EaseOutQuint,
    EaseInOutQuint,
    EaseInSine,
    EaseOutSine,
    EaseInOutSine,
    EaseInExpo,
    EaseOutExpo,
    EaseInOutExpo,
    EaseInCirc,
    EaseOutCirc,
    EaseInOutCirc,
    EaseInBack,
    EaseOutBack,
    EaseInOutBack,
    EaseInElastic,
    EaseOutElastic,
    EaseInOutElastic,
    EaseInBounce,
    EaseOutBounce,
    EaseInOutBounce
}

/// <summary>
/// A MonoBehaviour helper class for creating common animations like typing text or tweening float values.
/// Attach this script to a GameObject in your scene.
/// </summary>
public class AnimationScriptHelper : MonoBehaviour
{
    // --- Public Methods to Start Animations ---

    /// <summary>
    /// Animates a UI Text element to display text as if it's being typed out.
    /// </summary>
    /// <param name="uiTextElement">The UI Text component to animate. For TextMeshPro, change type to TextMeshProUGUI.</param>
    /// <param name="targetText">The full string to type out.</param>
    /// <param name="charsPerSecond">The speed of typing, in characters per second.</param>
    /// <param name="onComplete">Optional callback action when the typing animation is complete.</param>
    /// <returns>A Coroutine reference, which can be used to stop the animation manually if needed.</returns>
    public Coroutine AnimateStringTyping(Text uiTextElement, string targetText, float charsPerSecond, Action onComplete = null)
    {
        // For TextMeshPro, you would use:
        // public Coroutine AnimateStringTyping(TextMeshProUGUI uiTextElement, string targetText, float charsPerSecond, Action onComplete = null)
        if (uiTextElement == null)
        {
            Debug.LogError("UI Text Element is null. Cannot animate string.");
            return null;
        }
        if (charsPerSecond <= 0)
        {
            Debug.LogWarning("CharsPerSecond must be positive. Defaulting to 10.");
            charsPerSecond = 10f;
        }
        return StartCoroutine(TypeStringCoroutine(uiTextElement, targetText, charsPerSecond, onComplete));
    }

    /// <summary>
    /// Animates a float value from a start value to an end value over a specified duration using an easing function.
    /// </summary>
    /// <param name="startValue">The initial value of the float.</param>
    /// <param name="endValue">The target value of the float.</param>
    /// <param name="duration">The total time the animation should take, in seconds.</param>
    /// <param name="onUpdate">Action called every frame with the current animated float value.</param>
    /// <param name="easingType">The type of easing function to apply to the animation.</param>
    /// <param name="onComplete">Optional callback action when the float animation is complete.</param>
    /// <returns>A Coroutine reference, which can be used to stop the animation manually if needed.</returns>
    public Coroutine AnimateFloat(float startValue, float endValue, float duration, Action<float> onUpdate, EasingType easingType = EasingType.Linear, Action onComplete = null)
    {
        if (duration <= 0)
        {
            Debug.LogWarning("Duration must be positive. Calling onUpdate with endValue immediately.");
            onUpdate?.Invoke(endValue);
            onComplete?.Invoke();
            return null;
        }
        if (onUpdate == null)
        {
            Debug.LogError("OnUpdate action is null. Cannot animate float without it.");
            return null;
        }
        return StartCoroutine(AnimateFloatCoroutine(startValue, endValue, duration, onUpdate, easingType, onComplete));
    }


    public Coroutine AnimateInt(int startValue, int endValue, float duration, Action<int> onUpdate, EasingType easingType = EasingType.Linear, Action onComplete = null)
    {
        if (duration <= 0)
        {
            onUpdate?.Invoke(endValue);
            onComplete?.Invoke();
            return null;
        }
        return StartCoroutine(AnimateIntCoroutine(startValue, endValue, duration, onUpdate, easingType, onComplete));
    }

    private IEnumerator AnimateIntCoroutine(int startValue, int endValue, float duration, Action<int> onUpdate, EasingType easingType, Action onComplete)
    {
        float elapsedTime = 0f;
        int lastValue = startValue;
        onUpdate(startValue); // Call once with the starting value

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EasingFunctions.Evaluate(t, easingType);

            // Lerp between the float versions of the integers
            float floatValue = Mathf.LerpUnclamped((float)startValue, (float)endValue, easedT);

            // Round to the nearest integer
            int currentValue = Mathf.RoundToInt(floatValue);

            // Only invoke the update action if the integer value has changed
            if (currentValue != lastValue)
            {
                onUpdate(currentValue);
                lastValue = currentValue;
            }

            yield return null;
        }

        // Ensure the final value is set precisely
        if (lastValue != endValue)
        {
            onUpdate(endValue);
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// Stops all animations (coroutines) started by this instance of AnimationScriptHelper.
    /// </summary>
    public void StopAllManagedAnimations()
    {
        StopAllCoroutines(); // Stops all coroutines started by this MonoBehaviour
        // If you need finer control, you would store Coroutine references from AnimateString/AnimateFloat
        // and call StopCoroutine(coroutineReference) individually.
    }


    // --- Private Coroutines for Animations ---

    private IEnumerator TypeStringCoroutine(Text uiTextElement, string targetText, float charsPerSecond, Action onComplete)
    {
        uiTextElement.text = "";
        float delay = 1f / charsPerSecond;
        for (int i = 0; i < targetText.Length; i++)
        {
            uiTextElement.text += targetText[i];
            yield return new WaitForSeconds(delay);
        }
        onComplete?.Invoke();
    }

    private IEnumerator AnimateFloatCoroutine(float startValue, float endValue, float duration, Action<float> onUpdate, EasingType easingType, Action onComplete)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration); // Normalized time (0 to 1)
            float easedT = EasingFunctions.Evaluate(t, easingType);
            float currentValue = Mathf.LerpUnclamped(startValue, endValue, easedT); // Use LerpUnclamped for easing functions that might go houtside 0-1 range (e.g., elastic, back)

            onUpdate(currentValue);
            yield return null; // Wait for the next frame
        }
        onUpdate(endValue); // Ensure the final value is set precisely
        onComplete?.Invoke();
    }
}

/// <summary>
/// Static class containing various easing functions.
/// Each function takes a normalized time 't' (0 to 1) and returns an eased value.
/// </summary>
public static class EasingFunctions
{
    public static float Evaluate(float t, EasingType type)
    {
        switch (type)
        {
            case EasingType.Linear: return Linear(t);
            case EasingType.EaseInQuad: return EaseInQuad(t);
            case EasingType.EaseOutQuad: return EaseOutQuad(t);
            case EasingType.EaseInOutQuad: return EaseInOutQuad(t);
            case EasingType.EaseInCubic: return EaseInCubic(t);
            case EasingType.EaseOutCubic: return EaseOutCubic(t);
            case EasingType.EaseInOutCubic: return EaseInOutCubic(t);
            case EasingType.EaseInQuart: return EaseInQuart(t);
            case EasingType.EaseOutQuart: return EaseOutQuart(t);
            case EasingType.EaseInOutQuart: return EaseInOutQuart(t);
            case EasingType.EaseInQuint: return EaseInQuint(t);
            case EasingType.EaseOutQuint: return EaseOutQuint(t);
            case EasingType.EaseInOutQuint: return EaseInOutQuint(t);
            case EasingType.EaseInSine: return EaseInSine(t);
            case EasingType.EaseOutSine: return EaseOutSine(t);
            case EasingType.EaseInOutSine: return EaseInOutSine(t);
            case EasingType.EaseInExpo: return EaseInExpo(t);
            case EasingType.EaseOutExpo: return EaseOutExpo(t);
            case EasingType.EaseInOutExpo: return EaseInOutExpo(t);
            case EasingType.EaseInCirc: return EaseInCirc(t);
            case EasingType.EaseOutCirc: return EaseOutCirc(t);
            case EasingType.EaseInOutCirc: return EaseInOutCirc(t);
            case EasingType.EaseInBack: return EaseInBack(t);
            case EasingType.EaseOutBack: return EaseOutBack(t);
            case EasingType.EaseInOutBack: return EaseInOutBack(t);
            case EasingType.EaseInElastic: return EaseInElastic(t);
            case EasingType.EaseOutElastic: return EaseOutElastic(t);
            case EasingType.EaseInOutElastic: return EaseInOutElastic(t);
            case EasingType.EaseInBounce: return EaseInBounce(t);
            case EasingType.EaseOutBounce: return EaseOutBounce(t);
            case EasingType.EaseInOutBounce: return EaseInOutBounce(t);
            default: return Linear(t);
        }
    }

    public static float Linear(float t) => t;

    public static float EaseInQuad(float t) => t * t;
    public static float EaseOutQuad(float t) => t * (2 - t);
    public static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

    public static float EaseInCubic(float t) => t * t * t;
    public static float EaseOutCubic(float t) => (--t) * t * t + 1;
    public static float EaseInOutCubic(float t) => t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

    public static float EaseInQuart(float t) => t * t * t * t;
    public static float EaseOutQuart(float t) => 1 - (--t) * t * t * t;
    public static float EaseInOutQuart(float t) => t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;

    public static float EaseInQuint(float t) => t * t * t * t * t;
    public static float EaseOutQuint(float t) => 1 + (--t) * t * t * t * t;
    public static float EaseInOutQuint(float t) => t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;

    public static float EaseInSine(float t) => 1 - Mathf.Cos(t * Mathf.PI / 2);
    public static float EaseOutSine(float t) => Mathf.Sin(t * Mathf.PI / 2);
    public static float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1) / 2;

    public static float EaseInExpo(float t) => t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));
    public static float EaseOutExpo(float t) => t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
    public static float EaseInOutExpo(float t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;
        if ((t /= 0.5f) < 1) return 0.5f * Mathf.Pow(2, 10 * (t - 1));
        return 0.5f * (-Mathf.Pow(2, -10 * --t) + 2);
    }

    public static float EaseInCirc(float t) => -(Mathf.Sqrt(1 - t * t) - 1);
    public static float EaseOutCirc(float t) => Mathf.Sqrt(1 - (--t) * t);
    public static float EaseInOutCirc(float t)
    {
        if ((t /= 0.5f) < 1) return -0.5f * (Mathf.Sqrt(1 - t * t) - 1);
        return 0.5f * (Mathf.Sqrt(1 - (t -= 2) * t) + 1);
    }

    private const float c1 = 1.70158f;
    private const float c2 = c1 * 1.525f;
    private const float c3 = c1 + 1f;

    public static float EaseInBack(float t) => c3 * t * t * t - c1 * t * t;
    public static float EaseOutBack(float t) => 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    public static float EaseInOutBack(float t)
    {
        return t < 0.5f
          ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
          : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }

    private const float c4 = (2 * Mathf.PI) / 3;
    private const float c5 = (2 * Mathf.PI) / 4.5f;

    public static float EaseInElastic(float t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;
        return -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * c4);
    }
    public static float EaseOutElastic(float t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;
    }
    public static float EaseInOutElastic(float t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;
        float sinVal = Mathf.Sin((20 * t - 11.125f) * c5);
        return t < 0.5f
          ? -(Mathf.Pow(2, 20 * t - 10) * sinVal) / 2
          : (Mathf.Pow(2, -20 * t + 10) * sinVal) / 2 + 1;
    }

    public static float EaseInBounce(float t) => 1 - EaseOutBounce(1 - t);
    public static float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1 / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2 / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        else if (t < 2.5 / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        else
        {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }
    public static float EaseInOutBounce(float t)
    {
        return t < 0.5f
          ? (1 - EaseOutBounce(1 - 2 * t)) / 2
          : (1 + EaseOutBounce(2 * t - 1)) / 2;
    }
}