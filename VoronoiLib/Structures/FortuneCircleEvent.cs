namespace VoronoiLib.Structures
{
    internal class FortuneCircleEvent : FortuneEvent
    {
        internal VPoint Lowest { get; private set; }
        internal double YCenter { get; private set; }
        internal RBTreeNode<BeachSection> ToDelete { get; private set; }

        public void Recycle()
        {
            Lowest = null;
            ToDelete = null;
            s_Pool.Recycle(this);
        }
        private void Init(VPoint lowest, double yCenter, RBTreeNode<BeachSection> toDelete)
        {
            Lowest = lowest;
            YCenter = yCenter;
            ToDelete = toDelete;
        }

        public int CompareTo(FortuneEvent other)
        {
            var c = Y.CompareTo(other.Y);
            return c == 0 ? X.CompareTo(other.X) : c;
        }

        public double X => Lowest.X;
        public double Y => Lowest.Y;

        public static FortuneCircleEvent New(VPoint lowest, double yCenter, RBTreeNode<BeachSection> toDelete)
        {
            var evt = s_Pool.Alloc();
            evt.Init(lowest, yCenter, toDelete);
            return evt;
        }
        private static SimpleObjectPool<FortuneCircleEvent> s_Pool = new SimpleObjectPool<FortuneCircleEvent>();
    }
}
