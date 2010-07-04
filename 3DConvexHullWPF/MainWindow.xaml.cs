using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Petzold.Media3D;
using MIConvexHull;
using System.Collections.Generic;

namespace ExampleWithGraphics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int NumberOfVertices = 10000;
        const double size = 50;
        List<IVertexConvHull> vertices;
        List<IVertexConvHull> convexHullVertices;
        List<IFaceConvHull> faces;
        public MainWindow()
        {
            InitializeComponent();
            var ax = new Petzold.Media3D.Axes();
            ax.Extent = 60;
            viewport.Children.Add(ax);
            Setup();
        }

        private void Setup()
        {
            vertices = new List<IVertexConvHull>();
            Random r = new Random();
            //Console.WriteLine("Ready? Push Return/Enter to start.");
            //Console.ReadLine();
            //Console.WriteLine("Making " + NumberOfVertices.ToString() + " random vertices.");

            for (int i = 0; i < NumberOfVertices; i++)
            {
                var vi = new vertex(size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2);
                vertices.Add(vi);

               // viewport.Children.Add(vi);
            }




        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Running...");
            DateTime now = DateTime.Now;
            faces = new List<IFaceConvHull>();
            convexHullVertices = ConvexHull.Find3D(vertices, typeof(face), out faces);
            TimeSpan interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours.ToString() + ":" + interval.Minutes.ToString()
                + ":" + interval.Seconds.ToString() + "." + interval.TotalMilliseconds.ToString();
            btnDisplay.IsEnabled = true;
            btnDisplay.IsDefault = true;

        }

        private void btnDisplay_Click(object sender, RoutedEventArgs e)
        {
            Point3DCollection CVPoints = new Point3DCollection();
            foreach (var chV in convexHullVertices)
                CVPoints.Add(new Point3D (chV.X,chV.Y,chV.Z));
           // CVPoints.Add(((vertex)chV).Center);

            Int32Collection faceTris = new Int32Collection();
            foreach (IFaceConvHull f in faces)
            {
                faceTris.Add(convexHullVertices.IndexOf(((face)f).v1));
                faceTris.Add(convexHullVertices.IndexOf(((face)f).v2));
                faceTris.Add(convexHullVertices.IndexOf(((face)f).v3));
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
                    new SpecularMaterial(Brushes.Beige, 250) }
                }
            };
            var modViz = new ModelVisual3D();
            modViz.Content = geoMod;

            viewport.Children.Add(modViz);

        }
    }
}
