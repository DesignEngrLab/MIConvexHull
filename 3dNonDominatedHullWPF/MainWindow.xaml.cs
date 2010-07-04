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
        const int NumberOfVertices = 1000000;
        const double size = 50;
        List<IVertexConvHull> vertices, nonDomVertices;
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
            //faces = new List<IFaceConvHull>();
            nonDomVertices = NonDominatedHull.Find3D(vertices);
            TimeSpan interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours.ToString() + ":" + interval.Minutes.ToString()
                + ":" + interval.Seconds.ToString() + "." + interval.TotalMilliseconds.ToString();
            btnDisplay.IsEnabled = true;
            btnDisplay.IsDefault = true;

        }

        private void btnDisplay_Click(object sender, RoutedEventArgs e)
        {
            foreach (var ndv in nonDomVertices)
            {
                var s=new Sphere()
                { Center = new Point3D(ndv.X,ndv.Y,ndv.Z),
                Radius = 0.3,
               BackMaterial = new DiffuseMaterial(Brushes.Red)};
                viewport.Children.Add(s);
            }
        }
    }
}
