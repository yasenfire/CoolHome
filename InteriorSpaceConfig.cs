namespace CoolHome
{
    public class InteriorSpaceConfig
    {
        public class Substance
        {
            public float Conductivity;
            public float Density;
            public float HeatCapacity;

            public Substance(float conductivity, float density, float heatCapacity)
            {
                Conductivity = conductivity;
                Density = density;
                HeatCapacity = heatCapacity;
            }
        }

        public class WallSize
        {
            public float Square;
            public float Thickness;
            public float Volume;
            public float AirVolume;
            public float AirChangesPerHour;

            public WallSize(float square, float thickness, float airVolume, float airChangesPerHour)
            {
                Square = square;
                Thickness = thickness;
                Volume = Square * Thickness;
            }
        }

        public static WallSize SMALL  = new WallSize(40f, 0.1f, 40f, 1f);
        public static WallSize MEDIUM = new WallSize(100f, 0.2f, 180f, 0.5f);
        public static WallSize LARGE  = new WallSize(400f, 0.4f, 1620f, 0.25f);
        public static WallSize HUGE   = new WallSize(2500f, 1f, 42800f, 0.05f);

        public static WallSize TRUCK  = new WallSize(2f, 0.05f, 1f, 1f);
        public static WallSize PLANE  = new WallSize(3f, 0.01f, 2f, 2f);

        public static Substance AIR       = new Substance(0.022f, 0.001293f, 1.007f);
        public static Substance WOOD      = new Substance(0.15f,  0.52f,     1.70f);
        public static Substance CONCRETE  = new Substance(1.51f,  2.00f,     0.88f);
        public static Substance STEEL     = new Substance(15f,    7.80f,     0.47f);
        public static Substance ALUMINIUM = new Substance(200f,   2.69f,     0.89f); 
        public static Substance GRANITE   = new Substance(2.4f,   2.60f,     0.79f);
        public static Substance BRICK     = new Substance(0.4f,   1.35f,     0.80f);

        public static float WINDOW_LOSS_NIGHT = 3f;
        public static float WINDOW_LOSS_DAY = 1.5f;

        public string Name = "default";
        public Substance Material = WOOD;
        public string MaterialTag = "WOOD";
        public WallSize Size = MEDIUM;
        public string SizeTag = "MEDIUM";

        public float Conductivity = 0;
        public float Density = 0;
        public float HeatCapacity = 0;
        public float Square = 0;
        public float Thickness = 0;
        public float Volume = 0;
        public float AirVolume = 0;
        public float AirChangesPerHour = 0;

        public float InsulationRValue = 0;

        public float WindowSquare = 6.8f;

        public float DeltaTemperature = 0f;

        public float GetMass()
        {
            return Density * 1000 * Volume;
        }

        public float GetRValue()
        {
            return Thickness / Conductivity + InsulationRValue;
        }

        public float GetUValue()
        {
            return 1 / GetRValue();
        }

        public void UpdateByTags()
        {
            if (MaterialTag == "WOOD") Material = WOOD;
            else if (MaterialTag == "CONCRETE") Material = CONCRETE;
            else if (MaterialTag == "STEEL") Material = STEEL;
            else if (MaterialTag == "ALUMINIUM") Material = ALUMINIUM;
            else if (MaterialTag == "GRANITE") Material = GRANITE;
            else if (MaterialTag == "BRICK") Material = BRICK;

            if (SizeTag == "SMALL") Size = SMALL;
            else if (SizeTag == "MEDIUM") Size = MEDIUM;
            else if (SizeTag == "LARGE") Size = LARGE;
            else if (SizeTag == "HUGE") Size = HUGE;
            else if (SizeTag == "TRUCK") Size = TRUCK;
            else if (SizeTag == "PLANE") Size = PLANE;
        }

        public void TransferProperties()
        {
            if (Conductivity == 0) Conductivity = Material.Conductivity;
            if (Density == 0) Density = Material.Density;
            if (HeatCapacity == 0) HeatCapacity = Material.HeatCapacity;

            if (Square == 0) Square = Size.Square;
            if (Thickness == 0) Thickness = Size.Thickness;
            if (Volume == 0) Volume = Size.Volume;
            if (AirVolume == 0) AirVolume = Size.AirVolume;
            if (AirChangesPerHour == 0) AirChangesPerHour = Size.AirChangesPerHour;
        }
    }
}
