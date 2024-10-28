namespace CoolHome.Utilities
{
    public class ShadowHeater
    {
        public string Type = "FIRE";
        public string PDID = "";
        public float Power = 0;
        public float Seconds = 0;

        public ShadowHeater() { }

        public ShadowHeater(string type, string pdid, float power, float seconds)
        {
            Type = type;
            PDID = pdid;
            Power = power;
            Seconds = seconds;
        }
    }
}
