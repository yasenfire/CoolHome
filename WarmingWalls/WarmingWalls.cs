using Il2Cpp;
using MelonLoader;
using UnityEngine;
using CoolHome.Utilities;

namespace CoolHome
{
    [RegisterTypeInIl2Cpp]
    class WarmingWalls : MonoBehaviour
    {
        public InteriorSpaceConfig? Profile;
        public double StoredHeat;

        private List<ShadowHeater> ShadowHeaters = new List<ShadowHeater>();

        public void LoadData(WarmingWallsProxy dataToLoad)
        {
            StoredHeat = dataToLoad.storedHeat;
            ShadowHeaters = dataToLoad.shadowHeaters;
            Profile = CoolHome.LoadSceneConfig(dataToLoad.ProfileName);
        }
        public WarmingWallsProxy SaveData()
        {
            SaveManager sm = CoolHome.saveManager;
            WarmingWallsProxy? dataToSave = new WarmingWallsProxy();

            dataToSave.storedHeat = StoredHeat;
            dataToSave.shadowHeaters = ShadowHeaters;
            dataToSave.ProfileName = Profile is null ? "default" : Profile.Name;

            return dataToSave;
        }

        public void Heat(float heat)
        {
            if (Profile is null) return;
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            float numSecondsDelta = tod.GetTODSeconds(Time.deltaTime);
            StoredHeat += heat * numSecondsDelta;
        }

        public void HeatLoss()
        {
            if (StoredHeat == 0 || Profile is null) return;
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();
            float deltaTemperature = this.GetDeltaTemperature();
            float numSecondsDelta = tod.GetTODSeconds(Time.deltaTime);
            StoredHeat -= Profile.Material.Conductivity * Profile.Size.Square * deltaTemperature * numSecondsDelta;

            float windowLoss = tod.IsDay() ? InteriorSpaceConfig.WINDOW_LOSS_DAY : InteriorSpaceConfig.WINDOW_LOSS_NIGHT;
            StoredHeat -= windowLoss * Profile.WindowSquare * deltaTemperature * numSecondsDelta;

            StoredHeat = Math.Max(StoredHeat, 0);
            if (StoredHeat > 0 || CoolHome.spaceManager.HasRegisteredHeaters(this)) return;
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
                    if (sh.Type == "FIRE") CoolHome.spaceManager.UnregisterFire(sh.PDID);
                    ShadowHeaters.Remove(sh);
                }
            }
            if (totalHeatPower > 0) Heat(totalHeatPower);

            this.HeatLoss();
        }

        public float GetDeltaTemperature()
        {
            if (Profile is null) return 0;
            return (float)(StoredHeat / (Profile.GetMass() * Profile.Material.HeatCapacity * 1000));
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
