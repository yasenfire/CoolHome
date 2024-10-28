using CoolHome.Utilities;

namespace CoolHome
{
    public class WarmingWallsProxy
    {
        public string Name = "";
        public double storedHeat = 0;
        public List<ShadowHeater> shadowHeaters = new List<ShadowHeater>();
        public string ProfileName = "default";
        public bool TrackedLastAurora = false;
    }
}
