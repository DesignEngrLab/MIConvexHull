#region

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;
using MIConvexHullPluginNameSpace;

#endregion

namespace ExampleWithGraphics
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NumberOfVertices = 500;
        private const double size = 50;
        private List<IVertexConvHull> convexHullVertices;
        private List<IFaceConvHull> faces;
        private ModelVisual3D modViz;
        private List<IVertexConvHull> vertices;

        public MainWindow()
        {
            InitializeComponent();
            btnMakePoints_Click(null, null);
        }


        private void btnMakePoints_Click(object sender, RoutedEventArgs e)
        {
            vertices = new List<IVertexConvHull>();
            var r = new Random();

            /****** Random Vertices ******/
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var vi = new vertex(size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2);
                vertices.Add(vi);

                drawingCanvas.Children.Add(vi);
            }
            btnMakePoints.IsDefault = true;
            btnDisplayDelaunay.IsEnabled = false;
            txtBlkTimer.Text = "00:00:00.000";
            var now = DateTime.Now;
            faces = new List<IFaceConvHull>();
            ConvexHull.InputVertices(vertices);
            convexHullVertices = ConvexHull.FindConvexHull(out faces, typeof(face), 3);
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplayDelaunay.IsEnabled = true;
            btnDisplayDelaunay.IsDefault = true;

        }

        private void btnFindDelaunay_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Running...");
            var now = DateTime.Now;
            faces = new List<IFaceConvHull>();
            ConvexHull.InputVertices(vertices);
            convexHullVertices = ConvexHull.FindConvexHull(out faces, typeof(face), 3);
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplayDelaunay.IsEnabled = true;
            btnDisplayDelaunay.IsDefault = true;

        }

        private void btnDisplayDelaunay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFindVoronoi_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDisplayVoronoi_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}