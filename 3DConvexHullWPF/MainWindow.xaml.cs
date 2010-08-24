using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Petzold.Media3D;
using MIConvexHullPluginNameSpace;
using System.Collections.Generic;
using StarMathLib;

namespace ExampleWithGraphics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int NumberOfVertices = 500;
        const double size = 50;
        List<IVertexConvHull> vertices;
        List<IVertexConvHull> convexHullVertices;
        List<IFaceConvHull> faces;
        public MainWindow()
        {
            InitializeComponent();
            btnMakeSquarePoints_Click(null, null);
        }

        void ClearAndDrawAxes()
        {
            var init = viewport.Children[0];
            viewport.Children.Clear();
            viewport.Children.Add(init);
            var ax = new Petzold.Media3D.Axes();
            ax.Extent = 60;
            viewport.Children.Add(ax);
        }
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Running...");
            DateTime now = DateTime.Now;
            faces = new List<IFaceConvHull>();
            convexHullVertices = ConvexHull.FindConvexHull(vertices, out faces, typeof(face),3);
            TimeSpan interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours.ToString() + ":" + interval.Minutes.ToString()
                + ":" + interval.Seconds.ToString() + "." + interval.TotalMilliseconds.ToString();
            btnDisplay.IsEnabled = true;
            btnDisplay.IsDefault = true;

        }
        ModelVisual3D modViz;
        private void btnDisplay_Click(object sender, RoutedEventArgs e)
        {
            viewport.Children.Remove(modViz);

            Point3DCollection CVPoints = new Point3DCollection();
            foreach (var chV in convexHullVertices)
            {
                CVPoints.Add(((vertex)chV).Center);
                viewport.Children.Add(new Sphere()
                {
                    Center = ((vertex)chV).Center,
                    BackMaterial = new DiffuseMaterial(Brushes.Orange),
                    Radius = .5
                });
            }


            Int32Collection faceTris = new Int32Collection();
            foreach (IFaceConvHull f in faces)
            {
                var orderImpliedNormal = StarMath.multiplyCross(
                    StarMath.subtract(f.vertices[1].location,f.vertices[0].location),
                    StarMath.subtract(f.vertices[2].location,f.vertices[1].location)
                    );
                if (StarMath.multiplyDot(f.normal, orderImpliedNormal) < 0)
                    Array.Reverse(f.vertices);
                faceTris.Add(convexHullVertices.IndexOf(f.vertices[0]));
                faceTris.Add(convexHullVertices.IndexOf(f.vertices[1]));
                faceTris.Add(convexHullVertices.IndexOf(f.vertices[2]));
            }
            var mg3d = new MeshGeometry3D();
            mg3d.Positions = CVPoints;
            mg3d.TriangleIndices = faceTris;


            var geoMod = new GeometryModel3D()
            {
                Geometry = mg3d,
                BackMaterial = new MaterialGroup()
                {
                    Children = new MaterialCollection()
                    { new DiffuseMaterial(Brushes.Red),
                    new SpecularMaterial(Brushes.Beige, 2.0) }
                }
            };
            modViz = new ModelVisual3D();
            modViz.Content = geoMod;

            viewport.Children.Add(modViz);

        }

        private void btnMakeSquarePoints_Click(object sender, RoutedEventArgs e)
        {
            ClearAndDrawAxes();
            vertices = new List<IVertexConvHull>();
            Random r = new Random();

            /****** Random Vertices ******/
            for (int i = 0; i < NumberOfVertices; i++)
            {
                var vi = new vertex(size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2);
                vertices.Add(vi);

                //viewport.Children.Add(vi);
            }
            btnRun.IsDefault = true;
            btnDisplay.IsEnabled = false;
            txtBlkTimer.Text = "00:00:00.000";
        }

        private void btnMakeCirclePoints_Click(object sender, RoutedEventArgs e)
        {
            ClearAndDrawAxes();
            vertices = new List<IVertexConvHull>();
            Random r = new Random();

            /****** Random Vertices ******/
            for (int i = 0; i < NumberOfVertices; i++)
            {
                var radius = size +r.NextDouble();
               // if (i < NumberOfVertices / 2) radius /= 2;
                var theta = 2 * Math.PI * r.NextDouble();
                var azimuth = Math.PI * r.NextDouble();
                var x = radius * Math.Cos(theta) * Math.Sin(azimuth);
                var y = radius * Math.Sin(theta) * Math.Sin(azimuth);
                var z = radius * Math.Cos(azimuth);
                var vi = new vertex(x, y, z);
                vertices.Add(vi);

                //viewport.Children.Add(vi);
            }
            btnRun.IsDefault = true;
            btnDisplay.IsEnabled = false;
            txtBlkTimer.Text = "00:00:00.000";
        }
    }
}
