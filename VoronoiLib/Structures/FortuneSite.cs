using System.Collections.Generic;

namespace VoronoiLib.Structures
{
    public class FortuneSite
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public List<VEdge> Cell { get; private set; }
        public List<FortuneSite> Neighbors { get; private set; }

        public long Tag { get; set; }
        public object CustomData { get; set; }

        public void Recycle()
        {
            Cell.Clear();
            Neighbors.Clear();
            s_Pool.Recycle(this);
        }
        public FortuneSite()
        {
            Cell = new List<VEdge>();
            Neighbors = new List<FortuneSite>();
        }
        private void Init(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static FortuneSite New(double x, double y)
        {
            var obj = s_Pool.Alloc();
            obj.Init(x, y);
            return obj;
        }
        private static SimpleObjectPool<FortuneSite> s_Pool = new SimpleObjectPool<FortuneSite>();
    }
}
