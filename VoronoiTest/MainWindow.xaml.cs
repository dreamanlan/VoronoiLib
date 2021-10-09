using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using Delaunay.Annotations;
using VoronoiLib;
using VoronoiLib.Structures;

namespace VoronoiTest
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public int PointCount { get; set; } = 2000;
        public double DiagramWidth => (int)Canvas.ActualWidth;
        public double DiagramHeight => (int)Canvas.ActualHeight;

        public ICommand DrawCommand { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            DrawCommand = new Command(param => GenerateAndDraw());
            Canvas.SizeChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(DiagramHeight));
                OnPropertyChanged(nameof(DiagramWidth));
            };

        }

        private List<FortuneSite> GeneratePoints(int amount, double maxX, double maxY)
        {
            // TODO make more beautiful
            var point0 = FortuneSite.New(0, 0);
            var point1 = FortuneSite.New(0, maxY);
            var point2 = FortuneSite.New(maxX, maxY);
            var point3 = FortuneSite.New(maxX, 0);
            var points = new List<FortuneSite>();// { point0, point1, point2, point3 };

            var random = new Random();
            for (int i = 0; i < amount; i++) {
                var pointX = random.NextDouble() * maxX;
                var pointY = random.NextDouble() * maxY;
                points.Add(FortuneSite.New(pointX, pointY));
            }

            return points;
        }

        private void GenerateAndDraw()
        {
            Canvas.Children.Clear();
            Canvas.ClipToBounds = true;
            var points = GeneratePoints(PointCount, DiagramWidth, DiagramHeight);
                        
            var voronoiTimer = Stopwatch.StartNew();
            var vornoiEdges = FortunesAlgorithm.Run(points, 0, 0, DiagramWidth, DiagramHeight);
            voronoiTimer.Stop();
            Console.WriteLine($"voronoi:{voronoiTimer.ElapsedTicks * 1000000 / Stopwatch.Frequency}us {voronoiTimer.ElapsedMilliseconds}ms");
            DrawVoronoi(vornoiEdges);
            DrawPoints(points);
        }

        private void DrawPoints(IEnumerable<FortuneSite> points)
        {
            foreach (var point in points)
            {
                var myEllipse = new Ellipse();
                myEllipse.Fill = System.Windows.Media.Brushes.Red;
                myEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                myEllipse.VerticalAlignment = VerticalAlignment.Top;
                myEllipse.Width = 1;
                myEllipse.Height = 1;
                var ellipseX = point.X - 0.5 * myEllipse.Height;
                var ellipseY = point.Y - 0.5 * myEllipse.Width;
                myEllipse.Margin = new Thickness(ellipseX, ellipseY, 0, 0);

                Canvas.Children.Add(myEllipse);
            }
        }

        private void DrawVoronoi(IEnumerable<VEdge> voronoiEdges)
        {
            foreach (var edge in voronoiEdges)
            {
                var line = new Line();
                line.Stroke = System.Windows.Media.Brushes.DarkViolet;
                line.StrokeThickness = 1;

                line.X1 = edge.Start.X;
                line.X2 = edge.End.X;
                line.Y1 = edge.Start.Y;
                line.Y2 = edge.End.Y;

                Canvas.Children.Add(line);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Command : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public Command(Action<object> execute)
            : this(execute, param => true)
        {
        }

        public Command(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (_canExecute == null) || _canExecute(parameter);
        }
    }
}