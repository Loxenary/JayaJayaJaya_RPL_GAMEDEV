using UnityEngine;
using DG.Tweening;

namespace EnemyAI
{
    /// <summary>
    /// Handles visual effects for enemy angry level changes.
    /// Attach this to enemies to add visual feedback when they level up.
    /// </summary>
    public class EnemyAngryVisuals : MonoBehaviour
    {
        [Header("Color Settings")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private bool changeColor = true;
        [SerializeField] private Color level1Color = Color.white;
        [SerializeField] private Color level2Color = new Color(1f, 0.8f, 0.2f); // Yellow
        [SerializeField] private Color level3Color = new Color(1f, 0.4f, 0.1f); // Orange
        [SerializeField] private Color level4Color = new Color(1f, 0.1f, 0.1f); // Red
        [SerializeField] private float colorTransitionDuration = 0.5f;

        [Header("Scale Settings")]
        [SerializeField] private bool pulseOnLevelUp = true;
        [SerializeField] private float pulseDuration = 0.3f;
        [SerializeField] private float pulseScale = 1.2f;

        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem levelUpParticles;
        [SerializeField] private bool spawnParticlesOnLevelUp = true;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip levelUpSound;
        [SerializeField] private bool playLevelUpSound = true;

        [Header("Light Settings")]
        [SerializeField] private Light angryLight;
        [SerializeField] private bool adjustLightIntensity = false;
        [SerializeField] private float baseLightIntensity = 1f;
        [SerializeField] private float maxLevelIntensityMultiplier = 3f;

        private BaseEnemyAI enemyAI;
        private Material targetMaterial;
        private Vector3 originalScale;
        private EnemyLevel currentVisualLevel = EnemyLevel.FIRST;

        private void Awake()
        {
            enemyAI = GetComponent<BaseEnemyAI>();

            if (targetRenderer != null)
            {
                // Create instance of material to avoid affecting all enemies
                targetMaterial = targetRenderer.material;
            }

            originalScale = transform.localScale;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            EnemyAngrySystem.OnGlobalAngryLevelChanged += OnAngryLevelChanged;
        }

        private void OnDisable()
        {
            EnemyAngrySystem.OnGlobalAngryLevelChanged -= OnAngryLevelChanged;
        }

        private void OnAngryLevelChanged(EnemyLevel newLevel)
        {
            if (currentVisualLevel == newLevel) return;

            currentVisualLevel = newLevel;

            // Apply visual effects
            if (changeColor)
            {
                UpdateColor(newLevel);
            }

            if (pulseOnLevelUp)
            {
                PulseEffect();
            }

            if (spawnParticlesOnLevelUp && levelUpParticles != null)
            {
                levelUpParticles.Play();
            }

            if (playLevelUpSound && levelUpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(levelUpSound);
            }

            if (adjustLightIntensity && angryLight != null)
            {
                UpdateLightIntensity(newLevel);
            }
        }

        private void UpdateColor(EnemyLevel level)
        {
            if (targetMaterial == null) return;

            Color targetColor = GetColorForLevel(level);

            // Smooth color transition using DOTween
            targetMaterial.DOColor(targetColor, colorTransitionDuration)
                .SetEase(Ease.OutQuad);
        }

        private Color GetColorForLevel(EnemyLevel level)
        {
            switch (level)
            {
                case EnemyLevel.FIRST:
                    return level1Color;
                case EnemyLevel.SECOND:
                    return level2Color;
                case EnemyLevel.THIRD:
                    return level3Color;
                case EnemyLevel.FOURTH:
                    return level4Color;
                default:
                    return Color.white;
            }
        }

        private void PulseEffect()
        {
            // Kill any existing pulse animations
            transform.DOKill();

            // Pulse effect using DOTween
            transform.DOScale(originalScale * pulseScale, pulseDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    transform.DOScale(originalScale, pulseDuration * 0.5f)
                        .SetEase(Ease.InQuad);
                });
        }

        private void UpdateLightIntensity(EnemyLevel level)
        {
            if (angryLight == null) return;

            // Calculate intensity based on level (1.0 to maxLevelIntensityMultiplier)
            float levelIndex = (int)level;
            float maxLevelIndex = System.Enum.GetValues(typeof(EnemyLevel)).Length - 1;
            float normalizedLevel = levelIndex / maxLevelIndex;

            float targetIntensity = baseLightIntensity + (baseLightIntensity * (maxLevelIntensityMultiplier - 1f) * normalizedLevel);

            // Smooth transition
            DOTween.To(() => angryLight.intensity, x => angryLight.intensity = x, targetIntensity, colorTransitionDuration)
                .SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Manually set visual level (useful for testing)
        /// </summary>
        public void SetVisualLevel(EnemyLevel level)
        {
            OnAngryLevelChanged(level);
        }

        private void OnDestroy()
        {
            // Clean up DOTween animations
            transform.DOKill();
            if (targetMaterial != null)
            {
                targetMaterial.DOKill();
            }
        }
    }
}
