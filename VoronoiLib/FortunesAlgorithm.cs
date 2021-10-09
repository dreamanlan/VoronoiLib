using System.Collections.Generic;
using System.Diagnostics;
using VoronoiLib.Structures;

namespace VoronoiLib
{
    public static class FortunesAlgorithm
    {
        public static LinkedList<VEdge> Run(List<FortuneSite> sites, double minX, double minY, double maxX, double maxY)
        {
            var eventQueue = NewFortuneEventMinHeap(5 * sites.Count);
            foreach (var s in sites) {
                eventQueue.Insert(FortuneSiteEvent.New(s));
            }

            //init tree
            var beachLine = BeachLine.New();
            var edges = NewVEdgeLinkedList();
            var deleted = NewFortuneCircleEventHashSet();

            //init edge list
            while (eventQueue.Count != 0) {
                var fEvent = eventQueue.Pop();
                if (fEvent is FortuneSiteEvent)
                    beachLine.AddBeachSection((FortuneSiteEvent)fEvent, eventQueue, deleted, edges);
                else {
                    if (deleted.Contains((FortuneCircleEvent)fEvent)) {
                        deleted.Remove((FortuneCircleEvent)fEvent);
                    }
                    else {
                        beachLine.RemoveBeachSection((FortuneCircleEvent)fEvent, eventQueue, deleted, edges);
                    }
                }
            }

            //clip edges
            var edgeNode = edges.First;
            while (edgeNode != null) {
                var edge = edgeNode.Value;
                var next = edgeNode.Next;

                var valid = ClipEdge(edge, minX, minY, maxX, maxY);
                if (!valid)
                    edges.Remove(edgeNode);
                //advance
                edgeNode = next;
            }

            beachLine.Recycle();
            RecycleFortuneCircleEventHashSet(deleted);
            RecycleFortuneEventMinHeap(eventQueue);
            return edges;
        }
        public static void Recycle(LinkedList<VEdge> graph)
        {
            for (var node = graph.First; null != node; node = node.Next) {
                var edge = node.Value;
                edge.Start?.Recycle();
                edge.End?.Recycle();
                edge.Recycle();
            }
            RecycleVEdgeLinkedList(graph);
        }
        //combination of personal ray clipping alg and cohen sutherland
        private static bool ClipEdge(VEdge edge, double minX, double minY, double maxX, double maxY)
        {
            var accept = false;

            //if its a ray
            if (edge.End == null) {
                accept = ClipRay(edge, minX, minY, maxX, maxY);
            }
            else {
                //Cohen–Sutherland
                var start = ComputeOutCode(edge.Start.X, edge.Start.Y, minX, minY, maxX, maxY);
                var end = ComputeOutCode(edge.End.X, edge.End.Y, minX, minY, maxX, maxY);

                while (true) {
                    if ((start | end) == 0) {
                        accept = true;
                        break;
                    }
                    if ((start & end) != 0) {
                        break;
                    }

                    double x = -1, y = -1;
                    var outcode = start != 0 ? start : end;

                    if ((outcode & 0x8) != 0) // top
                    {
                        x = edge.Start.X + (edge.End.X - edge.Start.X) * (maxY - edge.Start.Y) / (edge.End.Y - edge.Start.Y);
                        y = maxY;
                    }
                    else if ((outcode & 0x4) != 0) // bottom
                    {
                        x = edge.Start.X + (edge.End.X - edge.Start.X) * (minY - edge.Start.Y) / (edge.End.Y - edge.Start.Y);
                        y = minY;
                    }
                    else if ((outcode & 0x2) != 0) //right
                    {
                        y = edge.Start.Y + (edge.End.Y - edge.Start.Y) * (maxX - edge.Start.X) / (edge.End.X - edge.Start.X);
                        x = maxX;
                    }
                    else if ((outcode & 0x1) != 0) //left
                    {
                        y = edge.Start.Y + (edge.End.Y - edge.Start.Y) * (minX - edge.Start.X) / (edge.End.X - edge.Start.X);
                        x = minX;
                    }

                    if (outcode == start) {
                        edge.Start = VPoint.New(x, y);
                        start = ComputeOutCode(x, y, minX, minY, maxX, maxY);
                    }
                    else {
                        edge.End = VPoint.New(x, y);
                        end = ComputeOutCode(x, y, minX, minY, maxX, maxY);
                    }
                }
            }
            //if we have a neighbor
            if (edge.Neighbor != null) {
                //check it
                var valid = ClipEdge(edge.Neighbor, minX, minY, maxX, maxY);
                //both are valid
                if (accept && valid) {
                    edge.Start = edge.Neighbor.End;
                }
                //this edge isn't valid, but the neighbor is
                //flip and set
                if (!accept && valid) {
                    edge.Start = edge.Neighbor.End;
                    edge.End = edge.Neighbor.Start;
                    accept = true;
                }
            }
            return accept;
        }
        private static int ComputeOutCode(double x, double y, double minX, double minY, double maxX, double maxY)
        {
            int code = 0;
            if (x.ApproxEqual(minX) || x.ApproxEqual(maxX)) { }
            else if (x < minX)
                code |= 0x1;
            else if (x > maxX)
                code |= 0x2;

            if (y.ApproxEqual(minY) || x.ApproxEqual(maxY)) { }
            else if (y < minY)
                code |= 0x4;
            else if (y > maxY)
                code |= 0x8;
            return code;
        }
        private static bool ClipRay(VEdge edge, double minX, double minY, double maxX, double maxY)
        {
            var start = edge.Start;
            //horizontal ray
            if (edge.SlopeRise.ApproxEqual(0)) {
                if (!Within(start.Y, minY, maxY))
                    return false;
                if (edge.SlopeRun > 0 && start.X > maxX)
                    return false;
                if (edge.SlopeRun < 0 && start.X < minX)
                    return false;
                if (Within(start.X, minX, maxX)) {
                    if (edge.SlopeRun > 0)
                        edge.End = VPoint.New(maxX, start.Y);
                    else
                        edge.End = VPoint.New(minX, start.Y);
                }
                else {
                    if (edge.SlopeRun > 0) {
                        edge.Start = VPoint.New(minX, start.Y);
                        edge.End = VPoint.New(maxX, start.Y);
                    }
                    else {
                        edge.Start = VPoint.New(maxX, start.Y);
                        edge.End = VPoint.New(minX, start.Y);
                    }
                }
                return true;
            }
            //vertical ray
            if (edge.SlopeRun.ApproxEqual(0)) {
                if (start.X < minX || start.X > maxX)
                    return false;
                if (edge.SlopeRise > 0 && start.Y > maxY)
                    return false;
                if (edge.SlopeRise < 0 && start.Y < minY)
                    return false;
                if (Within(start.Y, minY, maxY)) {
                    if (edge.SlopeRise > 0)
                        edge.End = VPoint.New(start.X, maxY);
                    else
                        edge.End = VPoint.New(start.X, minY);
                }
                else {
                    if (edge.SlopeRise > 0) {
                        edge.Start = VPoint.New(start.X, minY);
                        edge.End = VPoint.New(start.X, maxY);
                    }
                    else {
                        edge.Start = VPoint.New(start.X, maxY);
                        edge.End = VPoint.New(start.X, minY);
                    }
                }
                return true;
            }

            //works for outside
            Debug.Assert(edge.Slope != null, "edge.Slope != null");
            Debug.Assert(edge.Intercept != null, "edge.Intercept != null");
            var topX = VPoint.New(CalcX(edge.Slope.Value, maxY, edge.Intercept.Value), maxY);
            var bottomX = VPoint.New(CalcX(edge.Slope.Value, minY, edge.Intercept.Value), minY);
            var leftY = VPoint.New(minX, CalcY(edge.Slope.Value, minX, edge.Intercept.Value));
            var rightY = VPoint.New(maxX, CalcY(edge.Slope.Value, maxX, edge.Intercept.Value));

            //reject intersections not within bounds
            var candidates = NewVPointList();
            if (Within(topX.X, minX, maxX))
                candidates.Add(topX);
            if (Within(bottomX.X, minX, maxX))
                candidates.Add(bottomX);
            if (Within(leftY.Y, minY, maxY))
                candidates.Add(leftY);
            if (Within(rightY.Y, minY, maxY))
                candidates.Add(rightY);

            //reject candidates which don't align with the slope
            for (var i = candidates.Count - 1; i > -1; i--) {
                var candidate = candidates[i];
                //grab vector representing the edge
                var ax = candidate.X - start.X;
                var ay = candidate.Y - start.Y;
                if (edge.SlopeRun * ax + edge.SlopeRise * ay < 0)
                    candidates.RemoveAt(i);
            }

            //if there are two candidates we are outside the closer one is start
            //the further one is the end
            if (candidates.Count == 2) {
                var ax = candidates[0].X - start.X;
                var ay = candidates[0].Y - start.Y;
                var bx = candidates[1].X - start.X;
                var by = candidates[1].Y - start.Y;
                if (ax * ax + ay * ay > bx * bx + by * by) {
                    edge.Start = candidates[1];
                    edge.End = candidates[0];
                }
                else {
                    edge.Start = candidates[0];
                    edge.End = candidates[1];
                }
            }

            //if there is one candidate we are inside
            if (candidates.Count == 1)
                edge.End = candidates[0];

            RecycleVPointList(candidates);
            //there were no candidates
            return edge.End != null;
        }
        private static bool Within(double x, double a, double b)
        {
            return x.ApproxGreaterThanOrEqualTo(a) && x.ApproxLessThanOrEqualTo(b);
        }
        private static double CalcY(double m, double x, double b)
        {
            return m * x + b;
        }
        private static double CalcX(double m, double y, double b)
        {
            return (y - b) / m;
        }
        private static MinHeap<FortuneEvent> NewFortuneEventMinHeap(int capacity)
        {
            var heap = s_FortuneEventMinHeapPool.Alloc();
            heap.Init(capacity);
            return heap;
        }
        private static void RecycleFortuneEventMinHeap(MinHeap<FortuneEvent> heap)
        {
            foreach (var evt in heap.Items) {
                if (null != evt)
                    evt.Recycle();
            }
            heap.Clear();
            s_FortuneEventMinHeapPool.Recycle(heap);
        }
        private static LinkedList<VEdge> NewVEdgeLinkedList()
        {
            return s_VEdgeLinkedListPool.Alloc();
        }
        private static void RecycleVEdgeLinkedList(LinkedList<VEdge> vedgeLinkedList)
        {
            vedgeLinkedList.Clear();
            s_VEdgeLinkedListPool.Recycle(vedgeLinkedList);
        }
        private static HashSet<FortuneCircleEvent> NewFortuneCircleEventHashSet()
        {
            return s_FortuneCircleEventHashSetPool.Alloc();
        }
        private static void RecycleFortuneCircleEventHashSet(HashSet<FortuneCircleEvent> hash)
        {
            hash.Clear();
            s_FortuneCircleEventHashSetPool.Recycle(hash);
        }
        private static List<VPoint> NewVPointList()
        {
            return s_VPointListPool.Alloc();
        }
        private static void RecycleVPointList(List<VPoint> list)
        {
            list.Clear();
            s_VPointListPool.Recycle(list);
        }

        private static SimpleObjectPool<MinHeap<FortuneEvent>> s_FortuneEventMinHeapPool = new SimpleObjectPool<MinHeap<FortuneEvent>>();
        private static SimpleObjectPool<LinkedList<VEdge>> s_VEdgeLinkedListPool = new SimpleObjectPool<LinkedList<VEdge>>();
        private static SimpleObjectPool<HashSet<FortuneCircleEvent>> s_FortuneCircleEventHashSetPool = new SimpleObjectPool<HashSet<FortuneCircleEvent>>();
        private static SimpleObjectPool<List<VPoint>> s_VPointListPool = new SimpleObjectPool<List<VPoint>>();
    }
}
