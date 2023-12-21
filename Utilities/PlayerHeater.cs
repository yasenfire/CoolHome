using UnityEngine;
using MelonLoader;

namespace CoolHome.Utilities
{
    [RegisterTypeInIl2Cpp]
    internal class PlayerHeater : MonoBehaviour
    {
        void Update()
        {
            string? CurrentSpaceName = CoolHome.spaceManager.GetCurrentSpaceName();
            if (CurrentSpaceName is null) return;
            WarmingWalls? ww = CoolHome.spaceManager.GetCurrentSpace();
            if (ww is null) ww = CoolHome.spaceManager.CreateNewSpace();
            ww.Heat(100);
        }
    }
}
