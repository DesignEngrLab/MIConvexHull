#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit;
using MIConvexHull;
using Microsoft.Win32;
#endregion

namespace ExampleWithGraphics
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window  //, INotifyPropertyChanged
    {
        private List<IFaceConvHull> CVXfaces;
        private List<IVertexConvHull> CVXvertices;
        private List<IFaceConvHull> Del_tetras;
        List<IVertexConvHull> Voro_nodes;
        List<Tuple<IVertexConvHull, IVertexConvHull>> Voro_edges;
        public Model3DGroup CurrentModel { get; set; }
        private MeshGeometry3D mesh;
        private ConvexHull convexHull;

        public MainWindow()
        {
            InitializeComponent();
            statusTimer.Tick += statusTimerTimer_Tick;
        }

        private void MIConvexHullMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            CVXvertices = convexHull.FindConvexHull(out CVXfaces);
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplay.IsEnabled = btnDisplay.IsDefault = true;
            Voro_edges = null; Voro_nodes = null; Del_tetras = null;
        }

        private void FindDelaunayClick(object sender, RoutedEventArgs e)
        {
            statusTimer.Start();
            Dispatcher.BeginInvoke((ThreadStart)RunDelaunay);
        }
        void RunDelaunay()
        {
            var now = DateTime.Now;
            Del_tetras = convexHull.FindDelaunayTriangulation();
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplay.IsEnabled = btnDisplay.IsDefault = true;
            CVXvertices = null; CVXfaces = null; Voro_edges = null; Voro_nodes = null;

        }

        private void FindVoronoiClick(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            convexHull.FindVoronoiGraph(out Voro_nodes, out Voro_edges);
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplay.IsEnabled = btnDisplay.IsDefault = true;
            CVXvertices = null; CVXfaces = null; Del_tetras = null;

        }




        private const string OpenFileFilter = "3D model files (*.3ds;*.obj;*.lwo;*.stl)|*.3ds;*.obj;*.objz;*.lwo;*.stl";


        //  public event PropertyChangedEventHandler PropertyChanged = new PropertyChangedEventHandler();

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog();
            d.InitialDirectory = "models";
            d.FileName = null;
            d.Filter = OpenFileFilter;
            d.DefaultExt = ".3ds";
            if (!d.ShowDialog().Value)
                return;
            CurrentModel = ModelImporter.Load(d.FileName);
            if (viewport.Children.Count > 2) viewport.Children.RemoveAt(2);
            viewport.Add(new ModelVisual3D
                             {
                                 Content = CurrentModel
                             });
            //var handler = PropertyChanged;
            //handler(this, new PropertyChangedEventArgs(CurrentModel.ToString()));
            //   PropertyChanged(this, new PropertyChangedEventArgs(CurrentModel.ToString()));
            viewport.ZoomToFit(100);

            var verts = new List<Point3D>();
            mesh = null;
            foreach (var model in CurrentModel.Children)
            {
                if (typeof(GeometryModel3D).IsInstanceOfType(model))
                    if (typeof(MeshGeometry3D).IsInstanceOfType(((GeometryModel3D)model).Geometry))
                    {
                        mesh = (MeshGeometry3D)((GeometryModel3D)model).Geometry;
                        verts.AddRange(mesh.Positions);
                    }
            }
            verts = verts.Distinct(new samePoint()).ToList();

            convexHull = new ConvexHull(verts);
            txtBlkTimer.Text = "#verts=" + verts.Count;
            CVXvertices = null;
            CVXfaces = null;
            btnDisplay.IsEnabled = false;
        }

        private void btnDisplay_Click(object sender, RoutedEventArgs e)
        {
            if ((CVXfaces != null) && (CVXvertices != null))
                displayConvexHull();
            else if (Del_tetras != null) displayDelaunayTetras();
            else if (Voro_nodes != null) displayVoronoi();
        }

        private void displayConvexHull()
        {
            var verts = new List<Point3D>();
            verts.AddRange(CVXvertices.Select(p => new Point3D(p.coordinates[0], p.coordinates[1], p.coordinates[2])));
            mesh.Positions = new Point3DCollection(verts);
            var faceTriCollection = new Int32Collection();
            foreach (var f in CVXfaces)
                foreach (var v in f.vertices)
                    faceTriCollection.Add(CVXvertices.IndexOf(v));
            mesh.TriangleIndices = faceTriCollection;

        }
        private void displayDelaunayTetras()
        {
            var verts = new List<Point3D>();
            var faceTriCollection = new List<int>();
            foreach (var t in Del_tetras)
            {
                var offset = verts.Count;
                verts.AddRange(t.vertices.Select(p => new Point3D(p.coordinates[0], p.coordinates[1], p.coordinates[2])));
                var center = new double[3];
                foreach (var v in t.vertices)
                    center = StarMath.add(center, v.coordinates, 3);
                center = StarMath.divide(center, 4, 3);
                for (int i = 0; i < 4; i++)
                {
                    var newface = new List<IVertexConvHull>((IVertexConvHull[])t.vertices.Clone());
                    newface.RemoveAt(i);
                    var indices = Enumerable.Range(0, 4).Where(p => p != i).Select(p => p + offset);
                    if (!inTheProperOrder(center, newface))
                        indices.Reverse();
                    faceTriCollection.AddRange(indices);
                }
            }
            mesh.Positions = new Point3DCollection(verts);
            mesh.TriangleIndices = new Int32Collection(faceTriCollection);
        }

        private bool inTheProperOrder(double[] center, List<IVertexConvHull> vertices)
        {
            var outDir = new double[3];
            outDir = vertices.Aggregate(outDir, (current, v) => StarMath.add(current, v.coordinates, 3));
            outDir = StarMath.divide(outDir, 3, 3);
            outDir = StarMath.subtract(outDir, center, 3);
            var normal = StarMath.crossProduct3(StarMath.subtract(vertices[1].coordinates, vertices[0].coordinates, 3),
                                                StarMath.subtract(vertices[2].coordinates, vertices[1].coordinates, 3));
            return (StarMath.dotProduct(normal, outDir, 3) >= 0);
        }

        private void displayVoronoi()
        {
            if (viewport.Children.Count > 2) viewport.Children.RemoveAt(2);

            foreach (var edge in Voro_edges)
                viewport.Add(new TubeVisual3D
               {
                   BackMaterial = Materials.LightGray,
                   Material = Materials.Red,
                   Path = new Point3DCollection
                                                   {
                                                       new Point3D(edge.Item1.coordinates[0],edge.Item1.coordinates[1],edge.Item1.coordinates[2]),
                                                       new Point3D(edge.Item2.coordinates[0],edge.Item2.coordinates[1],edge.Item2.coordinates[2])
                                                   }
               });
        }


        readonly DispatcherTimer statusTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(50000000),
            IsEnabled = true
        };

        void statusTimerTimer_Tick(object sender, EventArgs e)
        {
            if (convexHull != null)
            {
                outputTextBox.Text += convexHull.Status;
                outputTextBox.Text += "\n\n";
            }
        }
    }

    internal class samePoint : IEqualityComparer<Point3D>
    {
        #region Implementation of IEqualityComparer<in Point3D>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
        bool IEqualityComparer<Point3D>.Equals(Point3D x, Point3D y)
        {
            return ((x - y).Length < 0.000000001);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        int IEqualityComparer<Point3D>.GetHashCode(Point3D obj)
        {
            var d = obj.ToVector3D().LengthSquared;
            return d.GetHashCode();
        }

        #endregion
    }
}
