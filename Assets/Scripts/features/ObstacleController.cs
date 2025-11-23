using UnityEngine;
using System.Collections;

namespace JayaJayaJaya.Features
{
    /// <summary>
    /// Controller untuk mengontrol obstacle (box) yang menghalangi jalur
    /// Obstacle akan hilang/dihapus ketika dipanggil untuk membuka jalur
    /// </summary>
    public class ObstacleController : MonoBehaviour
    {
        [Header("Obstacle Settings")]
        [SerializeField] private float disappearDuration = 1f;
        [SerializeField] private bool useAnimation = true;

        [Header("Animation Settings")]
        [SerializeField] private AnimationType animationType = AnimationType.FadeOut;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Effects")]
        [SerializeField] private GameObject disappearEffect;
        [SerializeField] private AudioClip disappearSound;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private bool isDisappearing = false;
        private bool hasDisappeared = false;

        public enum AnimationType
        {
            Instant,
            FadeOut,
            ScaleDown,
            SlideDown
        }

        /// <summary>
        /// Menghilangkan obstacle dan membuka jalur
        /// </summary>
        public void RemoveObstacle()
        {
            if (hasDisappeared || isDisappearing)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[ObstacleController] Obstacle '{gameObject.name}' sudah dalam proses menghilang atau sudah hilang.");
                }
                return;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[ObstacleController] Menghilangkan obstacle '{gameObject.name}'");
            }

            // Spawn effect jika ada
            if (disappearEffect != null)
            {
                Instantiate(disappearEffect, transform.position, transform.rotation);
            }

            // Play sound jika ada
            if (disappearSound != null)
            {
                AudioSource.PlayClipAtPoint(disappearSound, transform.position);
            }

            if (useAnimation && animationType != AnimationType.Instant)
            {
                StartCoroutine(AnimateDisappear());
            }
            else
            {
                // Langsung hapus
                Destroy(gameObject);
                hasDisappeared = true;
            }
        }

        private IEnumerator AnimateDisappear()
        {
            isDisappearing = true;
            float elapsedTime = 0f;

            Vector3 originalScale = transform.localScale;
            Vector3 originalPosition = transform.position;

            // Get renderer untuk fade effect
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            Material[] materials = null;

            if (animationType == AnimationType.FadeOut && renderers.Length > 0)
            {
                // Siapkan material untuk fade
                materials = new Material[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    materials[i] = renderers[i].material;
                    // Enable transparent mode jika perlu
                    materials[i].SetFloat("_Mode", 3);
                    materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    materials[i].SetInt("_ZWrite", 0);
                    materials[i].DisableKeyword("_ALPHATEST_ON");
                    materials[i].EnableKeyword("_ALPHABLEND_ON");
                    materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    materials[i].renderQueue = 3000;
                }
            }

            while (elapsedTime < disappearDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / disappearDuration);
                float curveValue = scaleCurve.Evaluate(progress);

                switch (animationType)
                {
                    case AnimationType.FadeOut:
                        if (materials != null)
                        {
                            foreach (var mat in materials)
                            {
                                Color color = mat.color;
                                color.a = 1f - progress;
                                mat.color = color;
                            }
                        }
                        // Juga scale down sedikit
                        transform.localScale = originalScale * (1f - progress * 0.3f);
                        break;

                    case AnimationType.ScaleDown:
                        transform.localScale = originalScale * curveValue;
                        break;

                    case AnimationType.SlideDown:
                        transform.position = originalPosition + Vector3.down * progress * 2f;
                        transform.localScale = originalScale * (1f - progress * 0.5f);
                        break;
                }

                yield return null;
            }

            // Hapus obstacle
            Destroy(gameObject);
            hasDisappeared = true;
            isDisappearing = false;
        }

        private void OnDrawGizmos()
        {
            // Visualisasi obstacle di editor
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, transform.localScale);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
