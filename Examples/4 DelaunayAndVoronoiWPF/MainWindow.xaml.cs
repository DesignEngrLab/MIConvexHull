#region

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using MIConvexHull;
using System.Linq;

#endregion

namespace ExampleWithGraphics
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NumberOfVertices = 100;
        private double size;
        private List<face> faces;
        private List<vertex> vertices;
        private List<VoronoiEdge<vertex, face>> edges;

        public MainWindow()
        {
            InitializeComponent();
            // btnMakePoints_Click(null, null);
        }


        private void btnMakePoints_Click(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Children.Clear();
            size = Math.Min(drawingCanvas.Height, drawingCanvas.Width);
            vertices = new List<vertex>();
            var r = new Random();

            /****** Random Vertices ******/
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var vi = new vertex(size * r.NextDouble(), size * r.NextDouble());
                vertices.Add(vi);
                drawingCanvas.Children.Add(vi);
            }

            //int ls = 10;
            //int offset = 50;
            //for (var i = 0; i < ls; i++)
            //{
            //    for (int j = 0; j < ls; j++)
            //    {
            //        var vi = new vertex(i * size / (ls + 1) + offset, j * size / (ls + 1) + offset);
            //        vertices.Add(vi);
            //        drawingCanvas.Children.Add(vi);
            //    }
            //}

            //var ver = new vertex(0, 0);
            //vertices.Add(ver);
            //drawingCanvas.Children.Add(ver);

            //var tt = size;
            //ver = new vertex(tt, 0);
            //vertices.Add(ver);
            //drawingCanvas.Children.Add(ver);

            //ver = new vertex(0, tt);
            //vertices.Add(ver);
            //drawingCanvas.Children.Add(ver);

            //ver = new vertex(tt, tt);
            //vertices.Add(ver);
            //drawingCanvas.Children.Add(ver);

            btnDisplayDelaunay.IsDefault = true;
            btnDisplayDelaunay.IsEnabled = false;
            txtBlkTimer.Text = "00:00:00.000";
        }

        private void btnFindDelaunay_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Running...");
            var now = DateTime.Now;
            faces = Triangulation.CreateDelaunay<vertex, face>(vertices).Cells.ToList();
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = faces.Count.ToString() + " | " + interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplayDelaunay.IsEnabled = true;
            btnDisplayDelaunay.IsDefault = true;
        }

        private void btnDisplayDelaunay_Click(object sender, RoutedEventArgs e)
        {
            foreach (var f in faces)
                drawingCanvas.Children.Add((UIElement)f.Visual);
        }

        private void btnFindVoronoi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("sorry was too lazy to code this property.");
            //Console.WriteLine("Running...");
            //var now = DateTime.Now;
            //List<IVertexConvHull> nodes;
            //convexHull.FindVoronoiGraph(out nodes, out edges);
            //var interval = DateTime.Now - now;
            //txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
            //                   + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            //btnDisplayVoronoi.IsEnabled = true;
            //btnDisplayVoronoi.IsDefault = true;

        }

        private void btnDisplayVoronoi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("sorry was too lazy to code this property.");
            //foreach (var edge in edges)
            //    drawingCanvas.Children.Add(
            //        new Line
            //            {
            //                X1 = edge.Item1.coordinates[0],
            //                Y1 = edge.Item1.coordinates[1],
            //                X2 = edge.Item2.coordinates[0],
            //                Y2 = edge.Item2.coordinates[1],
            //                Stroke = Brushes.Red
            //            });
        }
    }
}