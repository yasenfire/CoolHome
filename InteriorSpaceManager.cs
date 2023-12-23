using CoolHome.Utilities;
using Il2Cpp;
using MelonLoader;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using Il2CppTLD.Gear;

namespace CoolHome
{
    internal class InteriorSpaceManager
    {
        static string ScriptName = "SCRIPT_Heating";

        string? CurrentSpace;
        WarmingWalls? CurrentWalls;
        GameObject? Instance;

        Dictionary<string, GameObject> TrackedSpaces = new Dictionary<string, GameObject>();
        Dictionary<string, WarmingWalls> RegisteredHeaters = new Dictionary<string, WarmingWalls>();

        public void LoadData()
        {
            if (Instance is null) return;
            TrackedSpaces.Clear();
            RegisteredHeaters.Clear();

            SaveManager saveManager = CoolHome.saveManager;
            InteriorSpaceManagerProxy dataToLoad = saveManager.LoadSpaceManager();

            CurrentSpace = dataToLoad.CurrentSpace;

            foreach (KeyValuePair<string, WarmingWallsProxy> trackedSpace in dataToLoad.TrackedSpaces)
            {
                GameObject go = new GameObject(trackedSpace.Key);
                go.transform.parent = Instance.transform;

                WarmingWalls ww = go.AddComponent<WarmingWalls>();
                ww.LoadData(trackedSpace.Value);
                TrackedSpaces.Add(trackedSpace.Key, go);

                if (trackedSpace.Key == CurrentSpace) CurrentWalls = ww;
            }

            foreach (KeyValuePair<string, string> registeredFire in dataToLoad.RegisteredFires)
            {
                if (TrackedSpaces[registeredFire.Value]) RegisteredHeaters.Add(
                    registeredFire.Key,
                    TrackedSpaces[registeredFire.Value].GetComponent<WarmingWalls>()
                );
            }
        }

        public void SaveData()
        {
            if (Instance is null) return;

            SaveManager saveManager = CoolHome.saveManager;
            InteriorSpaceManagerProxy dataToSave = new InteriorSpaceManagerProxy();

            dataToSave.CurrentSpace = CurrentSpace;
            foreach (KeyValuePair<string, WarmingWalls> registeredFire in RegisteredHeaters)
            {
                string name = registeredFire.Value.gameObject.name;
                dataToSave.RegisteredFires.Add(registeredFire.Key, name);
            }

            foreach (KeyValuePair<string, GameObject> trackedSpace in TrackedSpaces)
            {
                WarmingWallsProxy ww = trackedSpace.Value.GetComponent<WarmingWalls>().SaveData();
                dataToSave.TrackedSpaces.Add(trackedSpace.Key, ww);
            }

            saveManager.SaveSpaceManager(dataToSave);
        }

        [HarmonyPatch(typeof(Weather), nameof(Weather.Serialize))]
        internal class WeatherSerializePatch
        {
            static void Prefix()
            {
                CoolHome.spaceManager.SaveData();
            }
        }

        public void Init()
        {
            GameObject instance = GameObject.Find(ScriptName);
            if (instance is null)
            {
                instance = new GameObject(ScriptName);
                instance.AddComponent<Preserve>();
                instance.AddComponent<PlayerHeater>();
            }
            Instance = instance;
            LoadData();
        }

        public void Deinit()
        {
            if (Instance is null) return;
            Preserve p = Instance.GetComponent<Preserve>();
            p.Destroy();
            Instance = null;
        }

        public bool IsInitialized()
        {
            return Instance is not null;
        }

        public string? GetCurrentSpaceName()
        {
            return CurrentSpace;
        }

        public WarmingWalls? GetCurrentSpace()
        {
            if (CurrentWalls is not null) return CurrentWalls;
            if (CurrentSpace is not null && TrackedSpaces.ContainsKey(CurrentSpace))
            {
                GameObject go = TrackedSpaces[CurrentSpace];
                CurrentWalls = go.GetComponent<WarmingWalls>();
                if (CurrentWalls is not null) return CurrentWalls;
            }
            return null;
        }

        public void EnterIndoorScene(string sceneName)
        {
            CurrentSpace = sceneName;
            if (TrackedSpaces.ContainsKey(sceneName)) TrackedSpaces[sceneName].GetComponent<WarmingWalls>().RemoveShadowHeaters();
        }

        public void LeaveIndoorScene()
        {
            string? sceneName = CurrentSpace;
            WarmingWalls? ww = CurrentWalls;

            if (sceneName is null || ww is null) return;

            GearItem? itemInHands = GameManager.GetPlayerManagerComponent().m_ItemInHands;

            Fire[] allFires = GameObject.FindObjectsOfType<Fire>();
            foreach (Fire f in allFires)
            {
                if (f.GetRemainingLifeTimeSeconds() < 1) continue;
                string pdid = f.GetComponent<ObjectGuid>().PDID;
                ww.AddShadowHeater("FIRE", pdid, 3000, f.GetRemainingLifeTimeSeconds());
            }

            FlareItem[] allFlares = GameObject.FindObjectsOfType<FlareItem>();
            foreach (FlareItem flare in allFlares)
            {
                if (!flare.IsBurning()) continue;
                if (itemInHands is not null && itemInHands == flare.m_GearItem) continue;
                string id = GetGearItemId(flare.m_GearItem);
                ww.AddShadowHeater("FLARE", id, 1000, flare.GetNormalizedBurnTimeLeft() * flare.GetModifiedBurnLifetimeMinutes() * 60);
            }

            TorchItem[] allTorches = GameObject.FindObjectsOfType<TorchItem>();
            foreach (TorchItem torch in allTorches)
            {
                if (!torch.IsBurning()) continue;
                if (itemInHands is not null && itemInHands == torch.m_GearItem) continue;
                string id = GetGearItemId(torch.m_GearItem);
                ww.AddShadowHeater("TORCH", id, 800, (1 - torch.GetBurnProgress()) * torch.GetModifiedBurnLifetimeMinutes() * 60);
            }

            KeroseneLampItem[] allLamps = GameObject.FindObjectsOfType<KeroseneLampItem>();
            foreach (KeroseneLampItem lamp in allLamps)
            {
                if (!lamp.IsOn()) continue;
                if (itemInHands is not null && itemInHands == lamp.m_GearItem) continue;
                string id = GetGearItemId(lamp.m_GearItem);
                ww.AddShadowHeater("LAMP", id, 400, (lamp.m_CurrentFuelLiters / lamp.GetModifiedFuelBurnLitersPerHour()) * 3600);
            }

            CurrentSpace = null;
            CurrentWalls = null;
        }

        public void EnterOutdoorScene()
        {
            IndoorSpaceTrigger[] triggers = GameObject.FindObjectsOfType<IndoorSpaceTrigger>();
            foreach (IndoorSpaceTrigger ist in triggers)
            {
                string name = GetIndoorSpaceName(ist);
                if (TrackedSpaces.ContainsKey(name)) TrackedSpaces[name].GetComponent<WarmingWalls>().RemoveShadowHeaters();
            }
        }

        public void LeaveOutdoorScene()
        {
            IndoorSpaceTrigger[] triggers = GameObject.FindObjectsOfType<IndoorSpaceTrigger>();
            List<WarmingWalls> wallComponents = new List<WarmingWalls>();

            GearItem? itemInHands = GameManager.GetPlayerManagerComponent().m_ItemInHands;

            Dictionary<string, Fire> firesPresent = new Dictionary<string, Fire>();
            Fire[] allFires = GameObject.FindObjectsOfType<Fire>();
            foreach (Fire f in allFires)
            {
                firesPresent[GetFireId(f.gameObject)] = f;
            }

            Dictionary<string, FlareItem> flaresPresent = new Dictionary<string, FlareItem>();
            FlareItem[] allFlares = GameObject.FindObjectsOfType<FlareItem>();
            foreach (FlareItem flare in allFlares)
            {
                flaresPresent[GetGearItemId(flare.m_GearItem)] = flare;
            }

            Dictionary<string, TorchItem> torchesPresent = new Dictionary<string, TorchItem>();
            TorchItem[] allTorches = GameObject.FindObjectsOfType<TorchItem>();
            foreach (TorchItem torch in allTorches)
            {
                torchesPresent[GetGearItemId(torch.m_GearItem)] = torch;
            }

            Dictionary<string, KeroseneLampItem> lampsPresent = new Dictionary<string, KeroseneLampItem>();
            KeroseneLampItem[] allLamps = GameObject.FindObjectsOfType<KeroseneLampItem>();
            foreach (KeroseneLampItem lamp in allLamps)
            {
                lampsPresent[GetGearItemId(lamp.m_GearItem)] = lamp;
            }

            foreach (IndoorSpaceTrigger ist in triggers)
            {
                string name = GetIndoorSpaceName(ist);
                WarmingWalls? ww = TrackedSpaces.ContainsKey(name) ? TrackedSpaces[name].GetComponent<WarmingWalls>() : null;
                if (ww is not null) wallComponents.Add(ww);
            }

            foreach (KeyValuePair<string, WarmingWalls> entry in RegisteredHeaters)
            {
                if (!wallComponents.Contains(entry.Value)) continue;
                if (firesPresent.ContainsKey(entry.Key))
                {
                    Fire f = firesPresent[entry.Key];
                    if (f.GetRemainingLifeTimeSeconds() < 1) continue;
                    string pdid = f.GetComponent<ObjectGuid>().PDID;
                    entry.Value.AddShadowHeater("FIRE", pdid, 3000, f.GetRemainingLifeTimeSeconds());
                    continue;
                }
                if (flaresPresent.ContainsKey(entry.Key))
                {
                    FlareItem flare = flaresPresent[entry.Key];
                    if (itemInHands is not null && itemInHands == flare.m_GearItem) continue;
                    entry.Value.AddShadowHeater("FLARE", GetGearItemId(flare.m_GearItem), 1000, flare.GetNormalizedBurnTimeLeft() * flare.GetModifiedBurnLifetimeMinutes() * 60);
                    continue;
                }
                if (torchesPresent.ContainsKey(entry.Key))
                {
                    TorchItem torch = torchesPresent[entry.Key];
                    if (itemInHands is not null && itemInHands == torch.m_GearItem) continue;
                    entry.Value.AddShadowHeater("TORCH", GetGearItemId(torch.m_GearItem), 800, (1 - torch.GetBurnProgress()) * torch.GetModifiedBurnLifetimeMinutes() * 60);
                    continue;
                }
                if (lampsPresent.ContainsKey(entry.Key))
                {
                    KeroseneLampItem lamp = lampsPresent[entry.Key];
                    if (itemInHands is not null && itemInHands == lamp.m_GearItem) continue;
                    entry.Value.AddShadowHeater("LAMP", GetGearItemId(lamp.m_GearItem), 400, (lamp.m_CurrentFuelLiters / lamp.GetModifiedFuelBurnLitersPerHour()) * 3600);
                }
            }
        }

        public string GetIndoorSpaceName(IndoorSpaceTrigger ist)
        {
            GameObject go = ist.gameObject;
            ObjectGuid id = go.GetComponent<ObjectGuid>();
            return id.PDID;
        }

        public void EnterIndoorSpace(IndoorSpaceTrigger ist)
        {
            string name = GetIndoorSpaceName(ist);
            CurrentSpace = name;
            Melon<CoolHome>.Logger.Msg("Entering space named " + name);
        }

        [HarmonyPatch(typeof(IndoorSpaceTrigger), nameof(IndoorSpaceTrigger.OnTriggerEnter))]
        internal class IndoorSpaceTriggerOnEnterPatch
        {
            static void Postfix(IndoorSpaceTrigger __instance)
            {
                __instance.m_UseOutdoorTemperature = false;
                CoolHome.spaceManager.EnterIndoorSpace(__instance);
            }
        }

        public void Leave()
        {
            CurrentSpace = null;
            CurrentWalls = null;
        }

        [HarmonyPatch(typeof(IndoorSpaceTrigger), nameof(IndoorSpaceTrigger.OnTriggerExit))]
        internal class IndoorSpaceTriggerOnExitPatch
        {
            static void Postfix(IndoorSpaceTrigger __instance)
            {
                CoolHome.spaceManager.Leave();
            }
        }

        public WarmingWalls? CreateNewSpace()
        {
            if (CurrentSpace is null) return null;

            GameObject go = new GameObject();
            go.name = CurrentSpace;
            go.AddComponent<WarmingWalls>();
            if (Instance is null) CoolHome.spaceManager.Init();
            if (Instance is not null) go.transform.parent = Instance.transform;
            WarmingWalls ww = go.GetComponent<WarmingWalls>();
            if (ww is not null)
            {
                CurrentWalls = ww;
                TrackedSpaces[CurrentSpace] = go;
                ww.Profile = CoolHome.LoadSceneConfig(CurrentSpace);
            }
            return ww;
        }

        public bool HasRegisteredHeaters(WarmingWalls ww)
        {
            return RegisteredHeaters.ContainsValue(ww);
        }

        public void RemoveIrrelevantSpace(WarmingWalls ww)
        {
            if (CurrentWalls == ww)
            {
                CurrentSpace = null;
                CurrentWalls = null;
            }

            foreach (string id in TrackedSpaces.Keys)
            {
                if (TrackedSpaces[id] == ww) TrackedSpaces.Remove(id);
            }
        }

        static string GetFireId(GameObject fire)
        {
            ObjectGuid og = fire.GetComponent<ObjectGuid>();
            return og.PDID;
        }

        static string GetGearItemId(GearItem gi)
        {
            return gi.m_InstanceID.ToString();
        }

        public void RegisterFire(Fire fire)
        {
            if (GetCurrentSpaceName() is null) return;
            WarmingWalls? ww = GetCurrentSpace();
            if (ww is null)
            {
                ww = CreateNewSpace();
            }
            string id = GetFireId(fire.gameObject);
            if (!RegisteredHeaters.ContainsKey(id)) RegisteredHeaters[id] = ww;
        }

        public void RegisterGearItemHeater(GearItem heater)
        {
            if (GetCurrentSpaceName() is null) return;
            WarmingWalls? ww = GetCurrentSpace();
            if (ww is null)
            {
                ww = CreateNewSpace();
            }
            string id = GetGearItemId(heater);
            if (!RegisteredHeaters.ContainsKey(id)) RegisteredHeaters[id] = ww;
        }

        public void UnregisterHeater(string id)
        {
            if (RegisteredHeaters.ContainsKey(id)) RegisteredHeaters.Remove(id);
        }



        [HarmonyPatch(typeof(Fire), nameof(Fire.TurnOn))]
        internal class FireRegisterPatch
        {
            static void Postfix(Fire __instance)
            {
                CoolHome.spaceManager.RegisterFire(__instance);
            }
        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.TurnOff))]
        internal class FireUnregisterPatch
        {
            static void Postfix(Fire __instance)
            {
                string id = GetFireId(__instance.gameObject);
                CoolHome.spaceManager.UnregisterHeater(id);
            }
        }

        static void TryRegisterGearItem(GearItem gi)
        {
            if (gi.m_FlareItem is not null && gi.m_FlareItem.IsBurning())
            {
                CoolHome.spaceManager.RegisterGearItemHeater(gi);
                return;
            }

            if (gi.m_TorchItem is not null && gi.m_TorchItem.IsBurning())
            {
                CoolHome.spaceManager.RegisterGearItemHeater(gi);
                return;
            }

            if (gi.m_KeroseneLampItem is not null && gi.m_KeroseneLampItem.IsOn())
            {
                CoolHome.spaceManager.RegisterGearItemHeater(gi);
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Throw))]
        internal class ThrownGearItemRegisterPatch
        {
            static void Postfix(GearItem gi)
            {
                InteriorSpaceManager.TryRegisterGearItem(gi);
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Drop))]
        internal class DropGearItemRegisterPatch
        {
            static void Postfix(GameObject go)
            {
                GearItem gi = go.GetComponent<GearItem>();
                InteriorSpaceManager.TryRegisterGearItem(gi);
            }
        }

        [HarmonyPatch(typeof(FlareItem), nameof(FlareItem.BurnOut))]
        internal class FlareBurningOutPatch
        {
            static void Postfix(FlareItem __instance)
            {
                string id = InteriorSpaceManager.GetGearItemId(__instance.m_GearItem);
                CoolHome.spaceManager.UnregisterHeater(id);
            }
        }

        [HarmonyPatch(typeof(TorchItem), nameof(TorchItem.Extinguish))]
        internal class TorchItemBurningOutPatch
        {
            static void Postfix(TorchItem __instance)
            {
                string id = InteriorSpaceManager.GetGearItemId((__instance.m_GearItem));
                CoolHome.spaceManager.UnregisterHeater(id);
            }
        }

        public GameObject? ObjectToPlace;

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.StartPlaceMesh), new Type[] { typeof(GameObject), typeof(float), typeof(PlaceMeshFlags) })]
        internal class RememberObjectToPlacePatch
        {
            static void Postfix(GameObject objectToPlace)
            {
                CoolHome.spaceManager.ObjectToPlace = objectToPlace;
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.ExitMeshPlacement))]
        internal class ForgetObjectToPlacePatch
        {
            static void Postfix()
            {
                if (CoolHome.spaceManager.ObjectToPlace is not null) CoolHome.spaceManager.ObjectToPlace = null;
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.PlaceMeshInWorld))]
        internal class PlaceObjectToPlacePatch
        {
            static void Postfix()
            {
                if (CoolHome.spaceManager.ObjectToPlace is not null)
                {
                    GearItem? gi = CoolHome.spaceManager.ObjectToPlace.GetComponent<GearItem>();
                    if (gi is null) return;
                    InteriorSpaceManager.TryRegisterGearItem(gi);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.ProcessPickupItemInteraction))]
        internal class UnregisterObjectPickedUpPatch
        {
            static void Postfix(GearItem item)
            {
                if (item.m_FlareItem is null && item.m_TorchItem is null && item.m_KeroseneLampItem is null) return;
                string id = InteriorSpaceManager.GetGearItemId(item);
                CoolHome.spaceManager.UnregisterHeater(id);
            }
        }

        WarmingWalls? GetSpaceAssociatedWithFire(GameObject fire)
        {
            string id = GetFireId(fire);
            if (RegisteredHeaters.ContainsKey(id)) return RegisteredHeaters[id];
            return null;
        }

        WarmingWalls? GetSpaceAssociatedWithGearItem(GearItem gi)
        {
            string id = GetGearItemId(gi);
            if (RegisteredHeaters.ContainsKey(id)) return RegisteredHeaters[id];
            return null;
        }

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.Update))]
        internal class HeatSourceUpdatePatch
        {
            static void Prefix(HeatSource __instance)
            {
                Fire? fire = __instance.gameObject.GetComponent<Fire>();
                if (fire is null) return;
                if (__instance.m_TempIncrease < 1) return;

                WarmingWalls? ww = CoolHome.spaceManager.GetSpaceAssociatedWithFire(__instance.gameObject);
                if (ww is null) return;

                if (CoolHome.settings.UseTemperatureBasedFires)
                {
                    float power = 1000 * __instance.m_TempIncrease / 3;
                    ww.Heat(power);
                } else
                {
                    float powerCoefficient = __instance.m_TempIncrease / __instance.m_MaxTempIncrease;
                    float power = 3000;
                    ww.Heat(power * powerCoefficient);
                }
            }
        }

        [HarmonyPatch(typeof(FlareItem), nameof(FlareItem.Update))]
        internal class FlareItemUpdatePatch
        {
            static void Prefix(FlareItem __instance)
            {
                if (!__instance.IsBurning()) return;

                WarmingWalls? ww = CoolHome.spaceManager.GetSpaceAssociatedWithGearItem(__instance.m_GearItem);
                if (ww is null) return;

                ww.Heat(1000);
            }
        }

        [HarmonyPatch(typeof(TorchItem), nameof(TorchItem.Update))]
        internal class TorchItemUpdatePatch
        {
            static void Prefix(TorchItem __instance)
            {
                if (!__instance.IsBurning()) return;

                WarmingWalls? ww = CoolHome.spaceManager.GetSpaceAssociatedWithGearItem(__instance.m_GearItem);
                if (ww is null) return;

                ww.Heat(800);
            }
        }

        [HarmonyPatch(typeof(KeroseneLampItem), nameof(KeroseneLampItem.Update))]
        internal class KeroseneLampItemUpdatePatch
        {
            static void Prefix(KeroseneLampItem __instance)
            {
                if (!__instance.IsOn()) return;

                WarmingWalls? ww = CoolHome.spaceManager.GetSpaceAssociatedWithGearItem(__instance.m_GearItem);
                if (ww is null) return;

                ww.Heat(400);
            }
        }
    }
}
