using UnityEngine;

// "Light" is the correct Unity component class (Point is just a type setting within it)
[RequireComponent(typeof(Light))]
public class BreathingLight : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float breatheSpeed = 2f; // How fast it breathes

    private Light _targetLight;

    private void Awake()
    {
        _targetLight = GetComponent<Light>();
    }

    private void Update()
    {
        // 1. Calculate a value between -1 and 1 using Sine
        float sineValue = Mathf.Sin(Time.time * breatheSpeed);

        // 2. Normalize that value to be between 0 and 1
        // (Sine + 1) makes it 0 to 2. Dividing by 2 makes it 0 to 1.
        float normalizedValue = (sineValue + 1f) / 2f;

        // 3. Interpolate (Lerp) between min and max based on that 0-1 value
        _targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedValue);
    }
}