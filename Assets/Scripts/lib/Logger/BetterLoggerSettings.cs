using System;
using UnityEngine;

namespace CustomLogger
{
    /// <summary>
    /// Settings for the BetterLogger system
    /// Create via: Assets > Create > Logger > Logger Settings
    /// Place in Resources folder to be automatically loaded
    /// </summary>
    [CreateAssetMenu(fileName = "BetterLoggerSettings", menuName = "Logger/Logger Settings", order = 1)]
    public class BetterLoggerSettings : ScriptableObject
    {
        [Header("General Settings")]
        [Tooltip("Enable/disable all logging")]
        [SerializeField] private bool isEnabled = true;

        [Header("Format Settings")]
        [Tooltip("Show timestamp in logs")]
        [SerializeField] private bool showTimestamp = true;

        [Tooltip("Show category in logs")]
        [SerializeField] private bool showCategory = true;

        [Tooltip("Show log type (LOG/WARNING/ERROR)")]
        [SerializeField] private bool showLogType = true;

        [Tooltip("Use colored categories")]
        [SerializeField] private bool useColors = true;

        [Header("Category Filters")]
        [Tooltip("Enable/disable specific log categories")]
        [SerializeField] private CategoryFilter[] categoryFilters = new CategoryFilter[]
        {
            new CategoryFilter { category = BetterLogger.LogCategory.General, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.Audio, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.Player, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.UI, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.Network, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.Physics, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.AI, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.Input, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.System, enabled = true },
            new CategoryFilter { category = BetterLogger.LogCategory.Custom, enabled = true },
        };

        [Serializable]
        public class CategoryFilter
        {
            public BetterLogger.LogCategory category;
            public bool enabled;
        }

        // Public properties
        public bool IsEnabled => isEnabled;
        public bool ShowTimestamp => showTimestamp;
        public bool ShowCategory => showCategory;
        public bool ShowLogType => showLogType;
        public bool UseColors => useColors;

        /// <summary>
        /// Check if a specific category is enabled
        /// </summary>
        public bool IsCategoryEnabled(BetterLogger.LogCategory category)
        {
            foreach (var filter in categoryFilters)
            {
                if (filter.category == category)
                {
                    return filter.enabled;
                }
            }
            return true; // Default to enabled if not found
        }

        /// <summary>
        /// Enable/disable a specific category
        /// </summary>
        public void SetCategoryEnabled(BetterLogger.LogCategory category, bool enabled)
        {
            for (int i = 0; i < categoryFilters.Length; i++)
            {
                if (categoryFilters[i].category == category)
                {
                    categoryFilters[i].enabled = enabled;
                    return;
                }
            }
        }

        /// <summary>
        /// Enable all categories
        /// </summary>
        public void EnableAllCategories()
        {
            for (int i = 0; i < categoryFilters.Length; i++)
            {
                categoryFilters[i].enabled = true;
            }
        }

        /// <summary>
        /// Disable all categories
        /// </summary>
        public void DisableAllCategories()
        {
            for (int i = 0; i < categoryFilters.Length; i++)
            {
                categoryFilters[i].enabled = false;
            }
        }
    }
}
