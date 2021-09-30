﻿using System.Collections.Generic;

namespace VoronoiLib.Structures
{
    public class FortuneSite
    {
        public double X { get; set; }
        public double Y { get; set; }
        public List<VEdge> Cell { get; private set; }
        public List<FortuneSite> Neighbors { get; private set; }

        public long Tag { get; set; }
        public object CustomData { get; set; }

        public FortuneSite()
        {
            Cell = new List<VEdge>();
            Neighbors = new List<FortuneSite>();
        }
        public FortuneSite(double x, double y):this()
        {
            X = x;
            Y = y;
        }
        public void Reset()
        {
            Cell.Clear();
            Neighbors.Clear();
        }
    }
}
