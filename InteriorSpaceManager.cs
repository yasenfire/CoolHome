using CoolHome.Utilities;
using Il2Cpp;
using MelonLoader;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace CoolHome
{
    internal class InteriorSpaceManager
    {
        static string ScriptName = "SCRIPT_Heating";

        string? CurrentSpace;
        WarmingWalls? CurrentWalls;
        GameObject? Instance;

        Dictionary<string, GameObject> TrackedSpaces = new Dictionary<string, GameObject>();
        Dictionary<string, WarmingWalls> RegisteredFires = new Dictionary<string, WarmingWalls>();

        public void LoadData()
        {
            if (Instance is null) return;
            TrackedSpaces.Clear();
            RegisteredFires.Clear();

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
                if (TrackedSpaces[registeredFire.Value]) RegisteredFires.Add(
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
            foreach (KeyValuePair<string, WarmingWalls> registeredFire in RegisteredFires)
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

            Fire[] allFires = GameObject.FindObjectsOfType<Fire>();
            foreach (Fire f in allFires)
            {
                if (f.GetRemainingLifeTimeSeconds() < 1) continue;
                ww.AddShadowHeater(3000, f.GetRemainingLifeTimeSeconds());
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

            Dictionary<string, Fire> firesPresent = new Dictionary<string, Fire>();
            Fire[] allFires = GameObject.FindObjectsOfType<Fire>();
            foreach (Fire f in allFires)
            {
                firesPresent[GetFireId(f.gameObject)] = f;
            }

            foreach (IndoorSpaceTrigger ist in triggers)
            {
                string name = GetIndoorSpaceName(ist);
                WarmingWalls? ww = TrackedSpaces.ContainsKey(name) ? TrackedSpaces[name].GetComponent<WarmingWalls>() : null;
                if (ww is not null) wallComponents.Add(ww);
            }

            foreach (KeyValuePair<string, WarmingWalls> entry in RegisteredFires)
            {
                if (!wallComponents.Contains(entry.Value)) continue;
                if (!firesPresent.ContainsKey(entry.Key)) continue;
                Fire f = firesPresent[entry.Key];
                if (f.GetRemainingLifeTimeSeconds() < 1) continue;
                entry.Value.AddShadowHeater(3000, f.GetRemainingLifeTimeSeconds());
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
                __instance.m_UseOutdoorTemperature = true;
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

        void CreateNewSpace()
        {
            if (CurrentSpace is null) return;

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
        }

        public bool HasRegisteredFires(WarmingWalls ww)
        {
            return RegisteredFires.ContainsValue(ww);
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

        void RegisterFire(Fire fire)
        {
            WarmingWalls? ww = GetCurrentSpace();
            if (ww is null)
            {
                CreateNewSpace();
                ww = GetCurrentSpace();
                Melon<CoolHome>.Logger.Msg("Created new space for " + CurrentSpace);
            }
            if (ww is null) return;
            string id = GetFireId(fire.gameObject);
            if (!RegisteredFires.ContainsKey(id)) RegisteredFires[id] = ww;
        }

        void UnregisterFire(Fire fire)
        {
            string id = GetFireId(fire.gameObject);
            RegisteredFires.Remove(id);
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
                CoolHome.spaceManager.UnregisterFire(__instance);
            }
        }

        WarmingWalls? GetSpaceAssociatedWithFire(GameObject fire)
        {
            string id = GetFireId(fire);
            if (RegisteredFires.ContainsKey(id)) return RegisteredFires[id];
            return null;
        }

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.Update))]
        internal class HeatSourceUpdatePatch
        {
            static void Prefix(HeatSource __instance)
            {
                if (__instance.m_TempIncrease < 1) return;

                WarmingWalls? ww = CoolHome.spaceManager.GetSpaceAssociatedWithFire(__instance.gameObject);
                if (ww is null) return;

                float powerCoefficient = __instance.m_TempIncrease / __instance.m_MaxTempIncrease;
                float power = 3000;

                ww.Heat(power * powerCoefficient);
            }
        }
    }
}
