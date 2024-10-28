using ModSettings;

namespace CoolHome.Utilities
{
    internal class Settings : JsonModSettings
    {
        [Name("Heat Gain Coefficient")]
        [Description("The higher it is the more effective heat sources will be")]
        [Slider(0.1f, 10f)]
        public float HeatGainCoefficient = 1;

        [Name("Heat Loss Coefficient")]
        [Description("The higher it is the faster shelters will cool down")]
        [Slider(0.1f, 10f)]
        public float HeatLossCoefficient = 1;

        [Name("Use temperature-based fire heat gain")]
        [Description("If enabled, fire power will be calculated based on its current temperature. Don't use it with vanilla fires")]
        public bool UseTemperatureBasedFires = false;

        [Name("Aurora powers electric heaters")]
        [Description("If enabled, some large industrial buildings will have their internal heating systems turned on during auroras")]
        public bool UseAuroraHeaters = false;
    }
}
