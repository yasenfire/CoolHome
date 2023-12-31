﻿using Il2Cpp;
using MelonLoader;
using UnityEngine;
using CoolHome.Utilities;

namespace CoolHome
{
    [RegisterTypeInIl2Cpp]
    class WarmingWalls : MonoBehaviour
    {
        static float PRESSURE_PER_K = 37.24f;
        static float HEIGHT_TEMP_PER_M = 0.115f;

        public string Name = "";
        public InteriorSpaceConfig? Profile;
        public double StoredHeat;

        private int FramesToCull = 60;
        private List<ShadowHeater> ShadowHeaters = new List<ShadowHeater>();

        public void LoadData(WarmingWallsProxy dataToLoad)
        {
            Name = dataToLoad.Name;
            StoredHeat = dataToLoad.storedHeat;
            ShadowHeaters = dataToLoad.shadowHeaters;
            Profile = CoolHome.LoadSceneConfig(dataToLoad.ProfileName);
        }
        public WarmingWallsProxy SaveData()
        {
            SaveManager sm = CoolHome.saveManager;
            WarmingWallsProxy? dataToSave = new WarmingWallsProxy();

            dataToSave.Name = Name;
            dataToSave.storedHeat = StoredHeat;
            dataToSave.shadowHeaters = ShadowHeaters;
            dataToSave.ProfileName = Profile is null ? "default" : Profile.Name;

            return dataToSave;
        }

        public void Heat(float rawHeat)
        {
            float heat = rawHeat * CoolHome.settings.HeatGainCoefficient;
            if (Profile is null) return;
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            float numSecondsDelta = tod.GetTODSeconds(Time.deltaTime);
            StoredHeat += heat * numSecondsDelta;
        }

        public void HeatLoss()
        {
            if (Profile is null) return;
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();

            float deltaTemperature = this.GetDeltaTemperature();
            float insideTemperature = CoolHome.GetInsideTemperature() - Profile.DeltaTemperature;
            float outsideTemperature = CoolHome.GetOutsideTemperature();
            deltaTemperature += insideTemperature - outsideTemperature;

            float numSecondsDelta = tod.GetTODSeconds(Time.deltaTime);
            
            FabricHeatLoss(numSecondsDelta, deltaTemperature);
            AirHeatLoss(numSecondsDelta, deltaTemperature);
        }

        public void FabricHeatLoss(float numSecondsDelta, float deltaTemperature)
        {
            if (Profile is null) return;
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            StoredHeat -= 10 * numSecondsDelta;
            StoredHeat -= Profile.GetUValue() * Profile.Square * deltaTemperature * numSecondsDelta;

            float windowLoss = tod.IsDay() ? InteriorSpaceConfig.WINDOW_LOSS_DAY : InteriorSpaceConfig.WINDOW_LOSS_NIGHT;
            StoredHeat -= windowLoss * Profile.WindowSquare * deltaTemperature * numSecondsDelta * CoolHome.settings.HeatLossCoefficient;
        }

        public void AirHeatLoss(float numSecondsDelta, float deltaTemperature)
        {
            if (Profile is null) return;

            float hoursPassed = numSecondsDelta / 3600;

            double airToLeave = Profile.AirChangesPerHour * Profile.AirVolume * InteriorSpaceConfig.AIR.Density * hoursPassed;
            StoredHeat -= airToLeave * InteriorSpaceConfig.AIR.HeatCapacity * deltaTemperature * CoolHome.settings.HeatLossCoefficient;
        }

        public void MaybeCull()
        {
            if (CoolHome.spaceManager.GetCurrentSpaceName() == Name || CoolHome.spaceManager.HasRegisteredHeaters(this) || StoredHeat > 1000)
            {
                FramesToCull = 60;
                return;
            }

            if (FramesToCull > 0)
            {
                FramesToCull -= 1;
                return;
            }
            CoolHome.spaceManager.RemoveIrrelevantSpace(this);
            Destroy(this.gameObject);
        }

        public void Update()
        {
            float totalHeatPower = 0;
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            float numSecondsDelta = tod.GetTODSeconds(Time.deltaTime);
            foreach (ShadowHeater sh in ShadowHeaters)
            {
                totalHeatPower += sh.Power;
                sh.Seconds -= numSecondsDelta;
                if (sh.Seconds < 0)
                {
                    CoolHome.spaceManager.UnregisterHeater(sh.PDID);
                    ShadowHeaters.Remove(sh);
                }
            }
            if (totalHeatPower > 0) Heat(totalHeatPower);

            HeatLoss();
            MaybeCull();
        }

        public float GetDeltaTemperature()
        {
            if (Profile is null) return 0;
            if (StoredHeat < 0) return 0;
            return (float)(StoredHeat / (Profile.GetMass() * Profile.HeatCapacity * 1000));
        }

        public void AddShadowHeater(string type, string pdid, float power, float seconds)
        {
            ShadowHeaters.Add(new ShadowHeater(type, pdid, power, seconds));
        }

        public void RemoveShadowHeaters()
        {
            ShadowHeaters.Clear();
        }

        public List<ShadowHeater> GetShadowHeaters()
        {
            return ShadowHeaters;
        }

        public InteriorSpaceConfig GetProfile()
        {
            if (Profile is null) Profile = new InteriorSpaceConfig();
            return Profile;
        }
    }
}
