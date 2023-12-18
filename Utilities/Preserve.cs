using UnityEngine;
using MelonLoader;

namespace CoolHome.Utilities
{
    [RegisterTypeInIl2Cpp]
    internal class Preserve : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
