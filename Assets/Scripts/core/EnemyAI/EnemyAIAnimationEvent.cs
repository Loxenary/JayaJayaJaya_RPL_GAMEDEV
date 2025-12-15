namespace EnemyAI
{
    /// <summary>
    /// Event published when an enemy animation should be played
    /// </summary>
    public struct EnemyAnimationPlay
    {
        /// <summary>
        /// The name of the animation state or trigger to play
        /// </summary>
        public string animationName;

        /// <summary>
        /// Optional: The layer index in the animator (default: -1 for all layers)
        /// </summary>
        public int layerIndex;

        /// <summary>
        /// Optional: Normalized time to start playing from (0-1, default: 0)
        /// </summary>
        public float normalizedTime;

        /// <summary>
        /// If true, use CrossFade instead of Play for smoother transitions
        /// </summary>
        public bool useCrossFade;

        /// <summary>
        /// CrossFade duration in seconds (only used if useCrossFade is true)
        /// </summary>
        public float crossFadeDuration;

        /// <summary>
        /// Create a simple animation play event
        /// </summary>
        public static EnemyAnimationPlay Simple(string animationName)
        {
            return new EnemyAnimationPlay
            {
                animationName = animationName,
                layerIndex = -1,
                normalizedTime = 0f,
                useCrossFade = false,
                crossFadeDuration = 0.1f
            };
        }

        /// <summary>
        /// Create a crossfade animation play event
        /// </summary>
        public static EnemyAnimationPlay WithCrossFade(string animationName, float duration = 0.1f)
        {
            return new EnemyAnimationPlay
            {
                animationName = animationName,
                layerIndex = -1,
                normalizedTime = 0f,
                useCrossFade = true,
                crossFadeDuration = duration
            };
        }
    }

    /// <summary>
    /// Event published when an animator trigger should be set
    /// </summary>
    public struct EnemyAnimationTrigger
    {
        /// <summary>
        /// The name of the trigger parameter
        /// </summary>
        public string triggerName;

        /// <summary>
        /// Create a trigger event
        /// </summary>
        public static EnemyAnimationTrigger Create(string triggerName)
        {
            return new EnemyAnimationTrigger { triggerName = triggerName };
        }
    }

    /// <summary>
    /// Event published when an animator bool parameter should be set
    /// </summary>
    public struct EnemyAnimationBool
    {
        public string parameterName;
        public bool value;

        public static EnemyAnimationBool Create(string parameterName, bool value)
        {
            return new EnemyAnimationBool { parameterName = parameterName, value = value };
        }
    }

    /// <summary>
    /// Event published when an animator float parameter should be set
    /// </summary>
    public struct EnemyAnimationFloat
    {
        public string parameterName;
        public float value;

        public static EnemyAnimationFloat Create(string parameterName, float value)
        {
            return new EnemyAnimationFloat { parameterName = parameterName, value = value };
        }
    }

    /// <summary>
    /// Event published when an animator int parameter should be set
    /// </summary>
    public struct EnemyAnimationInt
    {
        public string parameterName;
        public int value;

        public static EnemyAnimationInt Create(string parameterName, int value)
        {
            return new EnemyAnimationInt { parameterName = parameterName, value = value };
        }
    }
}
