namespace CoolHome
{
    public class InteriorSpaceConfig
    {
        public class WallMaterial
        {
            public float Conductivity;
            public float Density;
            public float HeatCapacity;

            public WallMaterial(float conductivity, float density, float heatCapacity)
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

            public WallSize(float square, float thickness)
            {
                Square = square;
                Thickness = thickness;
                Volume = Square * Thickness;
            }
        }

        public static WallSize SMALL  = new WallSize(40f, 0.04f);
        public static WallSize MEDIUM = new WallSize(100f, 0.04f);
        public static WallSize LARGE  = new WallSize(400f, 0.04f);
        public static WallSize HUGE   = new WallSize(2500f, 1f);

        public static WallMaterial WOOD     = new WallMaterial(0.15f, 0.52f, 1.70f);
        public static WallMaterial CONCRETE = new WallMaterial(1.51f, 2.00f, 0.88f);
        public static WallMaterial STEEL    = new WallMaterial(47f,   7.80f, 0.47f);
        public static WallMaterial GRANITE  = new WallMaterial(2.4f,  2.60f, 0.79f);

        public string Name = "default";
        public WallMaterial Material = WOOD;
        public WallSize Size = MEDIUM;

        public float GetMass()
        {
            return Material.Density * 1000 * Size.Volume;
        }
    }
}
