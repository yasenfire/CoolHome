using CoolHome.Utilities;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CoolHome
{
    [RegisterTypeInIl2Cpp]
    public class AuroraManager : MonoBehaviour
    {
        static int DAY_LENGTH = 24 * 60 * 60;

        Il2Cpp.AuroraManager? GameAuroraManager = null;
        TimeOfDay? Time = null;
        float LastAuroraStartedAt = 0;
        float LastAuroraEndedAt = 0;
        bool IsAuroraActive = false;

        public void LoadData(AuroraManagerProxy dataToLoad)
        {
            LastAuroraStartedAt = dataToLoad.LastAuroraStartedAt;
            LastAuroraEndedAt = dataToLoad.LastAuroraEndedAt;
            IsAuroraActive = dataToLoad.IsAuroraActive;
        }

        public AuroraManagerProxy SaveData()
        {
            SaveManager sm = CoolHome.saveManager;
            AuroraManagerProxy? dataToSave = new AuroraManagerProxy();

            dataToSave.LastAuroraStartedAt = LastAuroraStartedAt;
            dataToSave.LastAuroraEndedAt = LastAuroraEndedAt;
            dataToSave.IsAuroraActive = IsAuroraActive;

            return dataToSave;
        }

        void Awake()
        {
            Il2Cpp.AuroraManager am = GameManager.GetAuroraManager();
            GameAuroraManager = am;

            TimeOfDay t = GameManager.GetTimeOfDayComponent();
            Time = t;
        }

        void Update()
        {
            if (GameAuroraManager is null || Time is null) return;

            bool auroraState = GameAuroraManager.IsFullyActive();

            if (!auroraState && !IsAuroraActive) return;
            if (!auroraState && IsAuroraActive)
            {
                LastAuroraEndedAt = Time.GetDayNumber() + Time.GetNormalizedTime();
                IsAuroraActive = false;
                return;
            }
            if (!IsAuroraActive)
            {
                LastAuroraStartedAt = Time.GetDayNumber() + Time.GetNormalizedTime();
                IsAuroraActive = true;
            }

            if (CoolHome.spaceManager.Instance is null) return;
            WarmingWalls[] walls = CoolHome.spaceManager.Instance.GetComponentsInChildren<WarmingWalls>();
            foreach (WarmingWalls wall in walls)
            {
                wall.AuroraHeat();
                if (!wall.TrackedLastAurora) wall.TrackedLastAurora = true;
            }
        }

        public float GetSecondsSinceLastAuroraStarted()
        {
            if (Time is null || LastAuroraStartedAt == 0) return float.PositiveInfinity;
            return (Time.GetDayNumber() + Time.GetNormalizedTime() - LastAuroraStartedAt) * DAY_LENGTH;
        }

        public float GetLastAuroraLength()
        {
            if (Time is null || LastAuroraStartedAt == 0) return 0;
            if (LastAuroraEndedAt < LastAuroraStartedAt) return (Time.GetDayNumber() + Time.GetNormalizedTime() - LastAuroraStartedAt) * DAY_LENGTH;
            return (LastAuroraEndedAt - LastAuroraStartedAt) * DAY_LENGTH;
        }
    }
}