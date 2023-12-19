namespace CoolHome.Utilities
{
    public class ShadowHeater
    {
        public string Type;
        public string PDID;
        public float Power;
        public float Seconds;

        public ShadowHeater(string type, string pdid, float power, float seconds)
        {
            Type = type;
            PDID = pdid;
            Power = power;
            Seconds = seconds;
        }
    }
}
