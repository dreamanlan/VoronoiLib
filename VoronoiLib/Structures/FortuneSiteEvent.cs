namespace VoronoiLib.Structures
{
    internal class FortuneSiteEvent : FortuneEvent
    {
        public double X => Site.X;
        public double Y => Site.Y;
        internal FortuneSite Site { get; private set; }

        public void Recycle()
        {
            Site = null;
            s_Pool.Recycle(this);
        }
        private void Init(FortuneSite site)
        {
            Site = site;
        }

        public int CompareTo(FortuneEvent other)
        {
            var c = Y.CompareTo(other.Y);
            return c == 0 ? X.CompareTo(other.X) : c;
        }

        public static FortuneSiteEvent New(FortuneSite site)
        {
            var evt = s_Pool.Alloc();
            evt.Init(site);
            return evt;
        }
        private static SimpleObjectPool<FortuneSiteEvent> s_Pool = new SimpleObjectPool<FortuneSiteEvent>();
    }
}