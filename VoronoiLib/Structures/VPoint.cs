namespace VoronoiLib.Structures
{
    public class VPoint
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public void Recycle()
        {
            s_Pool.Recycle(this);
        }

        public static VPoint New(double x, double y)
        {
            var obj = s_Pool.Alloc();
            obj.X = x;
            obj.Y = y;
            return obj;
        }
        private static SimpleObjectPool<VPoint> s_Pool = new SimpleObjectPool<VPoint>();
    }
}
