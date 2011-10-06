#region

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MIConvexHull;
using Petzold.Media3D;
using System.Linq;

#endregion

namespace ExampleWithGraphics
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NumberOfVertices = 1000;
        private const double size = 50;
        private List<vertex> convexHullVertices;
        private List<face> faces;
        private ModelVisual3D modViz;
        private List<vertex> vertices;

        public MainWindow()
        {
            InitializeComponent();
            btnMakeSquarePoints_Click(null, null);
        }

        private void ClearAndDrawAxes()
        {
            var init = viewport.Children[0];
            viewport.Children.Clear();
            viewport.Children.Add(init);
            var ax = new Axes { Extent = 60 };
            viewport.Children.Add(ax);
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Running...");
            var now = DateTime.Now;
            var convexHull = ConvexHull.Create<vertex, face>(vertices);
            convexHullVertices = convexHull.Hull.ToList();
            faces = convexHull.Faces.ToList();
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplay.IsEnabled = true;
            btnDisplay.IsDefault = true;
        }

        private void btnDisplay_Click(object sender, RoutedEventArgs e)
        {
            viewport.Children.Remove(modViz);

            var CVPoints = new Point3DCollection();
            foreach (var chV in convexHullVertices)
            {
                CVPoints.Add(((vertex)chV).Center);
                viewport.Children.Add(new Sphere
                                          {
                                              Center = ((vertex)chV).Center,
                                              BackMaterial = new DiffuseMaterial(Brushes.Orange),
                                              Radius = .5
                                          });
            }


            var faceTris = new Int32Collection();
            foreach (var f in faces)
            {
                var orderImpliedNormal = StarMath.crossProduct3(
                    StarMath.subtract(f.Vertices[1].Position, f.Vertices[0].Position, 3),
                    StarMath.subtract(f.Vertices[2].Position, f.Vertices[1].Position, 3)
                    );
                if (StarMath.dotProduct(f.Normal, orderImpliedNormal, 3) < 0)
                    Array.Reverse(f.Vertices);
                faceTris.Add(convexHullVertices.IndexOf(f.Vertices[0]));
                faceTris.Add(convexHullVertices.IndexOf(f.Vertices[1]));
                faceTris.Add(convexHullVertices.IndexOf(f.Vertices[2]));
            }
            var mg3d = new MeshGeometry3D
                           {
                               Positions = CVPoints,
                               TriangleIndices = faceTris
                           };

            var material = new MaterialGroup
                            {
                                Children = new MaterialCollection
                                            {
                                                new DiffuseMaterial(Brushes.Red),
                                                new SpecularMaterial(Brushes.Beige, 2.0)
                                            }
                            };


            var geoMod = new GeometryModel3D
                             {
                                 Geometry = mg3d,
                                 Material = material
                             };

            modViz = new ModelVisual3D { Content = geoMod };
            viewport.Children.Add(modViz);
        }

        private void btnMakeSquarePoints_Click(object sender, RoutedEventArgs e)
        {
            ClearAndDrawAxes();
            vertices = new List<vertex>();
            var r = new Random();

            /****** Random Vertices ******/
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var vi = new vertex(size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2,
                                    size * r.NextDouble() - size / 2);
                vertices.Add(vi);

                viewport.Children.Add(vi);
            }

            //vertices.Add(new vertex(0, 0, 0));
            //vertices.Add(new vertex(10, 0, 0));
            //vertices.Add(new vertex(0, 10, 0));
            //vertices.Add(new vertex(0, 0, 10));
            //vertices.Add(new vertex(10, 10, 0));
            //vertices.Add(new vertex(10, 0, 10));
            //vertices.Add(new vertex(0, 10, 10));
            //vertices.Add(new vertex(10, 10, 10));

            //vertices.Add(new vertex(10, 10, 20));
            //vertices.Add(new vertex(0, 10, 20));
            //vertices.Add(new vertex(10, 0, 20));
            //vertices.Add(new vertex(0, 0, 20));

            //int d = 5;
            //for (int i = 0; i < d; i++)
            //{
            //    for (int j = 0; j < d; j++)
            //    {
            //        for (int k = 0; k < d; k++)
            //        {
            //            //                        vertices.Add(new vertex(5 * i, 0.6 * (i * i + j * j), 5 * j));
            //            vertices.Add(new vertex(5 * i, 5 * j, 5 * k));
            //        }
            //    }
            //}

            //foreach (var item in vertices)
            //{
            //   // viewport.Children.Add((vertex)item);
            //}

            btnRun.IsDefault = true;
            btnDisplay.IsEnabled = false;
            txtBlkTimer.Text = "00:00:00.000";
        }

        private void btnMakeCirclePoints_Click(object sender, RoutedEventArgs e)
        {
            ClearAndDrawAxes();
            vertices = new List<vertex>();
            var r = new Random();

            /****** Random Vertices ******/
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var radius = size + r.NextDouble();
                // if (i < NumberOfVertices / 2) radius /= 2;
                var theta = 2 * Math.PI * r.NextDouble();
                var azimuth = Math.PI * r.NextDouble();
                var x = radius * Math.Cos(theta) * Math.Sin(azimuth);
                var y = radius * Math.Sin(theta) * Math.Sin(azimuth);
                var z = radius * Math.Cos(azimuth);
                var vi = new vertex(x, y, z);
                vertices.Add(vi);
                /*
                 *          do {
                 x1 = 2.0 * ranf() - 1.0;
                 x2 = 2.0 * ranf() - 1.0;
                 w = x1 * x1 + x2 * x2;
         } while ( w >= 1.0 );

         w = sqrt( (-2.0 * ln( w ) ) / w );
         y1 = x1 * w;
         y2 = x2 * w;

                 */

                viewport.Children.Add(vi);
            }
            btnRun.IsDefault = true;
            btnDisplay.IsEnabled = false;
            txtBlkTimer.Text = "00:00:00.000";
        }
    }
}