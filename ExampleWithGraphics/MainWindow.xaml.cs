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
        public MainWindow()
        {
            InitializeComponent();
            int NumberOfVertices = 100;
            double size = 50;

            Random r = new Random();
            //Console.WriteLine("Ready? Push Return/Enter to start.");
            //Console.ReadLine();
            //Console.WriteLine("Making " + NumberOfVertices.ToString() + " random vertices.");
            var vertices = new List<vertex>();
            for (int i = 0; i < NumberOfVertices; i++)
            {
                var vi = new vertex(size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2);
                vertices.Add(vi);

                viewport.Children.Add(vi);
            }


            Console.WriteLine("Running...");
            DateTime now = DateTime.Now;
            var faces = new List<IFaceConvHull>();
            var convexHullVertices = ConvexHull.Find3D(vertices, typeof(face), faces);
            TimeSpan interval = DateTime.Now - now;
            Console.WriteLine("Out of the " + NumberOfVertices.ToString() + " vertices, there are " +
                convexHullVertices.Count.ToString() + " in the convex hull.");
            Console.WriteLine("time = " + interval);
            Console.ReadLine();

            //Point3DCollection CVPoints = new Point3DCollection();
            //foreach (var chV in convexHullVertices)
            //    CVPoints.Add(((vertex)chV).Center);

            //Int32Collection faceTris = new Int32Collection();
            //foreach (IFaceConvHull f in faces)
            //{
            //    faceTris.Add(convexHullVertices.IndexOf(((face)f).v1));
            //    faceTris.Add(convexHullVertices.IndexOf(((face)f).v2));
            //    faceTris.Add(convexHullVertices.IndexOf(((face)f).v3));
            //}
            //var mg3d = new MeshGeometry3D();
            //mg3d.Positions = CVPoints;
            //mg3d.TriangleIndices = faceTris;
            //viewport.Children.Add(mg3d);
        }
    }
}
