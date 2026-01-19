
using System.Collections;
using UnityEngine;

public class FlashlightHint : MonoBehaviour
{

    private bool _isFlashlightTriggered = false;
    private IEnumerator _flashlightHintCoroutine;

    private float _flashlightHintDuration = 7f;

    PlayerInputHandler inputHandler;

    private void OnEnable()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        inputHandler.OnFlashlightPerformed += FlashLightTriggered;

        _flashlightHintCoroutine = FlashLightHintEnumerate();
        StartCoroutine(_flashlightHintCoroutine);
    }

    private void OnDisable()
    {
        _flashlightHintCoroutine = null;
        inputHandler.OnFlashlightPerformed -= FlashLightTriggered;
    }

    void FlashLightTriggered()
    {
        if (_isFlashlightTriggered && _flashlightHintCoroutine != null)
        {
            StopCoroutine(_flashlightHintCoroutine);
            _flashlightHintCoroutine = null;
            return;
        }
        _isFlashlightTriggered = true;
    }

    private IEnumerator FlashLightHintEnumerate()
    {
        yield return new WaitForSeconds(_flashlightHintDuration);
        EventBus.Publish(new HintShown()
        {
            displayDuration = 3F,
            hintText = $"Press {"F"} to toggle the Flashlight"
        });
    }
}