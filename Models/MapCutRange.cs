namespace Windsock
{
    public class MapCutRange
    {
        public int minX {get; set;} = 0;
        public int minY {get; set;} = 0;
        public int maxX {get; set;} = 0;
        public int maxY {get; set;} = 0;

        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
    }
}
