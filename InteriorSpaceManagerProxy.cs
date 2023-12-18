namespace CoolHome
{
    internal class InteriorSpaceManagerProxy
    {
        public string? CurrentSpace;
        public Dictionary<string, WarmingWallsProxy> TrackedSpaces = new Dictionary<string, WarmingWallsProxy>();
        public Dictionary<string, string> RegisteredFires = new Dictionary<string, string>();
    }
}
