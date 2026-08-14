using UnityEngine;

namespace HimoHito
{
    /// <summary>
    /// Stores simple weave-thread tokens earned through deliberate rope releases.
    /// </summary>
    public sealed class WeaveResource : MonoBehaviour
    {
        [SerializeField, Min(0)] private int currentThreads;

        public int CurrentThreads => currentThreads;

        public void AddThread()
        {
            currentThreads++;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || amount > currentThreads)
            {
                return false;
            }

            currentThreads -= amount;
            return true;
        }

        public void ResetThreads()
        {
            currentThreads = 0;
        }

        public void RestoreThreads(int amount)
        {
            currentThreads = Mathf.Max(0, amount);
        }
    }
}
