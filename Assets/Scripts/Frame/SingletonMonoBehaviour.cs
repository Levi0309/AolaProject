using UnityEngine;

namespace EnjoyJob
{
    // Scene-level singleton. It does not auto-create or persist across scenes.
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Duplicate singleton instance found: {typeof(T).Name}", this);
                return;
            }

            Instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
