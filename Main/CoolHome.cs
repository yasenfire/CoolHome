using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using Il2Cpp;
using MelonLoader.Utils;
using MelonLoader.TinyJSON;
using HarmonyLib;
using ModSettings;
using CoolHome.Utilities;
using UnityEngine.Events;
using Il2CppTLD.Interactions;

namespace CoolHome
{
	public class CoolHome : MelonMod
	{
        public static InteriorSpaceManager spaceManager = new InteriorSpaceManager();

        public static readonly string MODS_FOLDER_PATH = Path.Combine(MelonEnvironment.ModsDirectory, "CoolHome");
        internal static SaveManager saveManager = new SaveManager();
        internal static Settings settings = new Settings();

        public override void OnInitializeMelon()
		{
            settings.AddToModSettings("CoolHome");
			MelonLogger.Msg("CoolHome loaded!");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            string sceneNameLower = sceneName.ToLowerInvariant();

            if (sceneNameLower.Contains("empty")) return;

            if (sceneNameLower.Contains("menu") || sceneNameLower.Contains("boot"))
            {
                if (spaceManager.IsInitialized()) spaceManager.Deinit();
                return;
            }

            if (!spaceManager.IsInitialized()) spaceManager.Init();

            if (!GameManager.GetWeatherComponent().IsIndoorScene())
            {
                CoolHome.spaceManager.EnterOutdoorScene();
                return;
            }

            if (!sceneName.Contains("_SANDBOX") && !sceneName.Contains("_DLC") && !sceneName.Contains("_WILDLIFE"))
            {
                spaceManager.EnterIndoorScene(sceneName);
            }
        }

        [HarmonyPatch(typeof(LoadScene), nameof(LoadScene.PerformSceneLoad))]
        internal class LoadScenePerformPatch
        {
            static void Prefix()
            {
                if (GameManager.GetWeatherComponent().IsIndoorScene())
                {
                    CoolHome.spaceManager.LeaveIndoorScene();
                    return;
                }
                CoolHome.spaceManager.LeaveOutdoorScene();
            }
        }

        public static InteriorSpaceConfig LoadSceneConfig(string spaceName)
        {
            string spaceFile = "scene_" + spaceName + ".json";
            InteriorSpaceConfig data;

            if (File.Exists(Path.Combine(CoolHome.MODS_FOLDER_PATH, spaceFile)))
            {
                data = JSON.Load(File.ReadAllText(Path.Combine(CoolHome.MODS_FOLDER_PATH, spaceFile))).Make<InteriorSpaceConfig>();
                data.UpdateByTags();
                data.TransferProperties();
                return data;
            }
            else
            {
                Melon<CoolHome>.Logger.Msg("No config found for " + spaceName + ". Using default");
                InteriorSpaceConfig defaultConfig = new InteriorSpaceConfig();
                defaultConfig.TransferProperties();
                return defaultConfig;
            }
        }

        public static float GetInsideTemperature()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            Weather w = GameManager.GetWeatherComponent();
            ExperienceModeManager emm = GameManager.GetExperienceModeManagerComponent();

            WarmingWalls? ww = spaceManager.GetCurrentSpace();
            float DeltaTemperature = ww is not null && ww.Profile is not null ? ww.Profile.DeltaTemperature : 0;

            return w.m_BaseTemperature + emm.GetOutdoorTempDropCelcius(tod.GetDayNumber()) * -1 + DeltaTemperature;
        }

        public static float GetOutsideTemperature()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            Weather w = GameManager.GetWeatherComponent();
            ExperienceModeManager emm = GameManager.GetExperienceModeManagerComponent();

            return w.m_BaseTemperature + emm.GetOutdoorTempDropCelcius(tod.GetDayNumber()) * -1 + w.m_CurrentBlizzardDegreesDrop + w.m_CurrentWindChill;
        }
    }
}