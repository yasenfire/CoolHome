using ModData;
using MelonLoader;
using MelonLoader.TinyJSON;

namespace CoolHome
{
    internal class SaveManager
    {
        ModDataManager dm = new ModDataManager("CoolHome", false);

        public void SaveSpaceManager(InteriorSpaceManagerProxy sm)
        {
            string dataString = JSON.Dump(sm);
            dm.Save(dataString);
        }

        public InteriorSpaceManagerProxy LoadSpaceManager()
        {
            string? dataString = dm.Load();
            if (dataString is null) {
                return new InteriorSpaceManagerProxy();
            }

            InteriorSpaceManagerProxy? data = JSON.Load(dataString).Make<InteriorSpaceManagerProxy>();
            return data is not null ? data : new InteriorSpaceManagerProxy();
        }
    }
}
