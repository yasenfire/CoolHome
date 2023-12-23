using UnityEngine;
using MelonLoader;
using Il2Cpp;
using Il2CppTLD.Gear;

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

            float Heat = 100;

            PlayerManager pm = GameManager.GetPlayerManagerComponent();
            GearItem? gi = pm.m_ItemInHands;
            if (gi is not null)
            {
                if (gi.m_FlareItem is not null)
                {
                    FlareItem fi = gi.m_FlareItem;
                    if (fi.IsBurning()) Heat += 1000;
                } 
                else if (gi.m_KeroseneLampItem is not null)
                {
                    KeroseneLampItem kli = gi.m_KeroseneLampItem;
                    if (kli.IsOn()) Heat += 400;
                }
                else if (gi.m_TorchItem is not null)
                {
                    TorchItem ti = gi.m_TorchItem;
                    if (ti.IsBurning()) Heat += 800;
                }
                else if (gi.m_MatchesItem is not null)
                {
                    MatchesItem mi = gi.m_MatchesItem;
                    if (mi.IsBurning()) Heat += 60;
                }
            }

            ww.Heat(Heat);
        }
    }
}
