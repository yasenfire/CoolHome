﻿using MelonLoader;
using UnityEngine;
using Il2CppInterop;
using Il2CppInterop.Runtime.Injection; 
using System.Collections;
using Il2Cpp;
using MelonLoader.Utils;
using MelonLoader.TinyJSON;
using HarmonyLib;
using UnityEngine.Analytics;

namespace CoolHome
{
	public class CoolHome : MelonMod
	{
        internal static InteriorSpaceManager spaceManager = new InteriorSpaceManager();

        public static readonly string MODS_FOLDER_PATH = Path.Combine(MelonEnvironment.ModsDirectory, "CoolHome");
        internal static SaveManager saveManager = new SaveManager();

        public override void OnInitializeMelon()
		{           
			MelonLogger.Msg("Mod started!");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName.ToLowerInvariant().Contains("menu") || sceneName.ToLowerInvariant().Contains("boot") || sceneName.ToLowerInvariant().Contains("empty"))
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
                return data;
            }
            else
            {
                Melon<CoolHome>.Logger.Msg("No config found for " + spaceName + ". Using default");
                return new InteriorSpaceConfig();
            }
        }

        public static float GetOutsideTemperature()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            Weather w = GameManager.GetWeatherComponent();
            ExperienceModeManager emm = GameManager.GetExperienceModeManagerComponent();

            return w.m_BaseTemperature + emm.GetOutdoorTempDropCelcius(tod.GetDayNumber());
        }
    }
}