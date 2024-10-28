using MelonLoader;
using HarmonyLib;
using Candlelight;
using CoolHome;
using Il2Cpp;
using UnityEngine;

namespace CoolHomeCandlelight
{
    public class CoolHomeCandlelight : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("CoolHomeCandlelight loaded!");
        }

        [HarmonyPatch(typeof(CandleItem), nameof(CandleItem.turnOn))]
        internal class CandleTurnOnPatch
        {
            static void Postfix(CandleItem __instance)
            {
                CoolHome.CoolHome.spaceManager.RegisterGearItemHeater(__instance.candleGearItem);
            }
        }

        [HarmonyPatch(typeof(CandleItem), nameof(CandleItem.Update))]
        internal class CandleUpdatePatch
        {
            static void Prefix(CandleItem __instance)
            {
                if (!__instance.isLit) return;

                WarmingWalls? ww = CoolHome.CoolHome.spaceManager.GetSpaceAssociatedWithGearItem(__instance.candleGearItem);
                if (ww is null) return;

                ww.Heat(90);
            }
        }

        [HarmonyPatch(typeof(CandleItem), nameof(CandleItem.turnOff))]
        internal class CandleTurnOffPatch
        {
            static void Postfix(CandleItem __instance)
            {
                string id = CoolHome.InteriorSpaceManager.GetGearItemId(__instance.candleGearItem);
                CoolHome.CoolHome.spaceManager.UnregisterHeater(id);
            }
        }

        [HarmonyPatch(typeof(InteriorSpaceManager), nameof(InteriorSpaceManager.LeaveIndoorScene))]
        internal class LeaveIndoorSceneWithCandlesPatch
        {
            static void Prefix(InteriorSpaceManager __instance)
            {
                WarmingWalls? ww = CoolHome.CoolHome.spaceManager.GetCurrentSpace();
                if (ww is null) return;

                CandleItem[] allCandles = GameObject.FindObjectsOfType<CandleItem>();
                foreach (CandleItem candle in allCandles)
                {
                    if (!candle.isLit) continue;
                    string id = CoolHome.InteriorSpaceManager.GetGearItemId(candle.candleGearItem);
                    float remainingTime = Candlelight_Main.isEndless ? float.PositiveInfinity : Candlelight_Main.currentBurntimeSetting * 3600 - candle.burnTime * 3600;
                    ww.AddShadowHeater("CANDLE", id, 90, remainingTime);
                }
            }
        }

        [HarmonyPatch(typeof(InteriorSpaceManager), nameof(InteriorSpaceManager.LeaveOutdoorScene))]
        internal class LeaveOutdoorSceneWithCandlesPatch
        {
            static void Prefix(InteriorSpaceManager __instance)
            {
                IndoorSpaceTrigger[] triggers = GameObject.FindObjectsOfType<IndoorSpaceTrigger>();
                List<WarmingWalls> wallComponents = new List<WarmingWalls>();

                foreach (IndoorSpaceTrigger ist in triggers)
                {
                    string name = __instance.GetIndoorSpaceName(ist);
                    WarmingWalls? ww = __instance.TrackedSpaces.ContainsKey(name) && __instance.TrackedSpaces[name] is not null ? __instance.TrackedSpaces[name].GetComponent<WarmingWalls>() : null;
                    if (ww is not null) wallComponents.Add(ww);
                }

                Dictionary<string, CandleItem> candlesPresent = new Dictionary<string, CandleItem>();
                CandleItem[] allCandles = GameObject.FindObjectsOfType<CandleItem>();
                foreach (CandleItem candle in allCandles)
                {
                    candlesPresent[InteriorSpaceManager.GetGearItemId(candle.candleGearItem)] = candle;
                }

                foreach (KeyValuePair<string, WarmingWalls> entry in __instance.RegisteredHeaters)
                {
                    if (!wallComponents.Contains(entry.Value) || !candlesPresent.ContainsKey(entry.Key)) continue;

                    CandleItem candle = candlesPresent[entry.Key];
                    string id = CoolHome.InteriorSpaceManager.GetGearItemId(candle.candleGearItem);
                    float remainingTime = Candlelight_Main.isEndless ? float.PositiveInfinity : Candlelight_Main.currentBurntimeSetting * 3600 - candle.burnTime * 3600;
                    entry.Value.AddShadowHeater("CANDLE", id, 90, remainingTime);
                }
            }
        }
    }
}