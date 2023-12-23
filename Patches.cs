using Il2Cpp;
using HarmonyLib;
using UnityEngine;

namespace CoolHome
{
    internal class Patches
    {
        [HarmonyPatch(typeof(Weather), nameof(Weather.CalculateCurrentTemperature))]
        internal class WeatherCalculateCurrentTemperaturePatch
        {
            static void Postfix(Weather __instance)
            {
                WarmingWalls? ww = CoolHome.spaceManager.GetCurrentSpace();
                float deltaTemperature = ww is null ? 0 : ww.GetDeltaTemperature();
                __instance.m_IndoorTemperatureCelsius = CoolHome.GetInsideTemperature() + deltaTemperature;
            }
        }
    }
}
