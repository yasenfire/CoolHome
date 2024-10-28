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
                float newTemperature = CoolHome.GetInsideTemperature() + deltaTemperature;
                __instance.m_IndoorTemperatureCelsius = newTemperature;
                GameManager.GetPlayerInVehicle().m_TempIncrease = deltaTemperature;
            }
        }

        [HarmonyPatch(typeof(IncreaseTemperatureTrigger), nameof(IncreaseTemperatureTrigger.OnTriggerEnter))]
        internal class IncreaseTemperatureTriggetDisablePatch
        {
            static void Postfix(IncreaseTemperatureTrigger __instance)
            {
                __instance.m_TempIncrease = 0;
            }
        }
    }
}
