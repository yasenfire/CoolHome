using CoolHome.Utilities;

namespace CoolHome
{
    internal class WarmingWallsProxy
    {
        public double storedHeat = 0;
        public List<ShadowHeater> shadowHeaters = new List<ShadowHeater>();
        public string ProfileName = "default";
    }
}
