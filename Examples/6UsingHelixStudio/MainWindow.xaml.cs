#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using HelixToolkit;
using MIConvexHull;
using Microsoft.Win32;
using StudioDemo;

#endregion

namespace ExampleWithGraphics
{
    class Vertex : IVertex
    {
        public double[] Position { get; set; }

        public Vertex(Point3D point)
        {
            Position = new double[3] { point.X, point.Y, point.Z };
        }
    }


    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window  //, INotifyPropertyChanged
    {
        private List<DefaultConvexFace<Vertex>> CVXfaces;
        private List<Vertex> CVXvertices;
        private List<DefaultTriangulationCell<Vertex>> Del_tetras;
        List<DefaultTriangulationCell<Vertex>> Voro_nodes;
        List<VoronoiEdge<Vertex, DefaultTriangulationCell<Vertex>>> Voro_edges;
        public Model3DGroup CurrentModel { get; set; }
        private MeshGeometry3D mesh;

        List<Vertex> vertices;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MIConvexHullMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                var hull = ConvexHull.Create(vertices);
                CVXvertices = hull.Points.ToList(); // convexHull.FindConvexHull(out CVXfaces);
                CVXfaces = hull.Faces.ToList();
                var interval = DateTime.Now - now;
                txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                                   + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
                btnDisplay.IsEnabled = btnDisplay.IsDefault = true;
                Voro_edges = null; Voro_nodes = null; Del_tetras = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FindDelaunayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                Del_tetras = Triangulation.CreateDelaunay(vertices).Cells.ToList();
                var interval = DateTime.Now - now;
                txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                                   + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
                btnDisplay.IsEnabled = btnDisplay.IsDefault = true;
                CVXvertices = null; CVXfaces = null; Voro_edges = null; Voro_nodes = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FindVoronoiClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                var voronoi = VoronoiMesh.Create(vertices);
                Voro_nodes = voronoi.Cells.ToList();
                Voro_edges = voronoi.Edges.ToList();
                var interval = DateTime.Now - now;
                txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                                   + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
                btnDisplay.IsEnabled = btnDisplay.IsDefault = true;
                CVXvertices = null; CVXfaces = null; Del_tetras = null;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
            Random rnd = new Random();
            vertices = verts.Distinct(new samePoint()).Select(p =>new Vertex((Point3D)((Vector3D)p + 0.05 * new Vector3D(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble())))).ToList();
            //vertices = verts.Distinct(new samePoint()).Select(p => new Vertex(p)).ToList();

            txtBlkTimer.Text = "#verts=" + vertices.Count;
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
            verts.AddRange(CVXvertices.Select(p => new Point3D(p.Position[0], p.Position[1], p.Position[2])));
            mesh.Positions = new Point3DCollection(verts);
            var faceTriCollection = new Int32Collection();
            foreach (var f in CVXfaces)
                foreach (var v in f.Vertices)
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
                verts.AddRange(t.Vertices.Select(p => new Point3D(p.Position[0], p.Position[1], p.Position[2])));
                var center = new double[3];
                foreach (var v in t.Vertices)
                    center = StarMath.add(center, v.Position, 3);
                center = StarMath.divide(center, 4, 3);
                for (int i = 0; i < 4; i++)
                {
                    var newface = t.Vertices.ToList(); //new List<IVertexConvHull>((IVertexConvHull[])t.vertices.Clone());
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

        private bool inTheProperOrder(double[] center, List<Vertex> vertices)
        {
            var outDir = new double[3];
            outDir = vertices.Aggregate(outDir, (current, v) => StarMath.add(current, v.Position, 3));
            outDir = StarMath.divide(outDir, 3, 3);
            outDir = StarMath.subtract(outDir, center, 3);
            var normal = StarMath.crossProduct3(StarMath.subtract(vertices[1].Position, vertices[0].Position, 3),
                                                StarMath.subtract(vertices[2].Position, vertices[1].Position, 3));
            return (StarMath.dotProduct(normal, outDir, 3) >= 0);
        }

        private void displayVoronoi()
        {
            //if (viewport.Children.Count > 2) viewport.Children.RemoveAt(2);

            //foreach (var edge in Voro_edges)
            //    viewport.Add(new TubeVisual3D
            //   {
            //       BackMaterial = Materials.LightGray,
            //       Material = Materials.Red,
            //       Path = new Point3DCollection
            //                                       {
            //                                           new Point3D(edge.Source.Position[0],edge.Source.Position[1],edge.Source.Position[2]),
            //                                           new Point3D(edge.Target.Position[0],edge.Target.Position[1],edge.Target.Position[2])
            //                                       }
            //   });
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
            return ((x - y).Length < 0.000000001) ;
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
