namespace VoronoiLib.Structures
{
    public class VEdge
    {
        public VPoint Start { get; internal set; }
        public VPoint End { get; internal set; }
        public FortuneSite Left { get; private set; }
        public FortuneSite Right { get; private set; }
        internal double SlopeRise { get; private set; }
        internal double SlopeRun { get; private set; }
        internal double? Slope { get; private set; }
        internal double? Intercept { get; private set; }

        public VEdge Neighbor { get; internal set; }

        public void Recycle()
        {
            Start = null;
            End = null;
            Neighbor = null;
            Left = null;
            Right = null;

            s_Pool.Recycle(this);
        }

        private void Init(VPoint start, FortuneSite left, FortuneSite right)
        {
            Start = start;
            Left = left;
            Right = right;
            
            //for bounding box edges
            if (left == null || right == null)
                return;

            //from negative reciprocal of slope of line from left to right
            //ala m = (left.y -right.y / left.x - right.x)
            SlopeRise = left.X - right.X;
            SlopeRun = -(left.Y - right.Y);
            Intercept = null;

            if (SlopeRise.ApproxEqual(0) || SlopeRun.ApproxEqual(0)) return;
            Slope = SlopeRise/SlopeRun;
            Intercept = start.Y - Slope*start.X;
        }

        public static VEdge New(VPoint start, FortuneSite left, FortuneSite right)
        {
            var obj = s_Pool.Alloc();
            obj.Init(start, left, right);
            return obj;
        }
        private static SimpleObjectPool<VEdge> s_Pool = new SimpleObjectPool<VEdge>();
    }
}
