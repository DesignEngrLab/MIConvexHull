#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit;
using MIConvexHullPluginNameSpace;
using Microsoft.Win32;
using StarMathLib;
using StudioDemo;

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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MIConvexHullMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            CVXvertices = ConvexHull.FindConvexHull(out CVXfaces);
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplay.IsEnabled = btnDisplay.IsDefault = true;
            Voro_edges = null; Voro_nodes = null; Del_tetras = null;
        }

        private void FindDelaunayClick(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            Del_tetras = ConvexHull.FindDelaunayTriangulation();
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;
            btnDisplay.IsEnabled = btnDisplay.IsDefault = true;
            CVXvertices = null; CVXfaces = null; Voro_edges = null; Voro_nodes = null;
        }

        private void FindVoronoiClick(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            ConvexHull.FindVoronoiGraph(out Voro_nodes, out Voro_edges);
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

            ConvexHull.InputVertices(verts);
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
                    center = StarMath.add(center, v.coordinates);
                center = StarMath.divide(center, 4);
                for (int i=0; i<4;i++)
                {
                    var newface = new List<IVertexConvHull>((IVertexConvHull[])t.vertices.Clone());
                    newface.RemoveAt(i);
                    var indices = Enumerable.Range(0, 4).Where(p => p != i).Select(p => p + offset);
                    if (!inTheProperOrder(center,newface))
                        indices.Reverse();
                    faceTriCollection.AddRange(indices);
                }
            }
            mesh.Positions = new Point3DCollection(verts);
            mesh.TriangleIndices =new Int32Collection( faceTriCollection);
        }

        private bool inTheProperOrder(double[] center, List<IVertexConvHull> vertices)
        {
            var outDir = new double[3];
            outDir = vertices.Aggregate(outDir, (current, v) => StarMath.add(current, v.coordinates));
            outDir = StarMath.divide(outDir, 3);
            outDir = StarMath.subtract(outDir, center);
            var normal = StarMath.multiplyCross(StarMath.subtract(vertices[1].coordinates, vertices[0].coordinates),
                                                StarMath.subtract(vertices[2].coordinates, vertices[1].coordinates));
            return (StarMath.multiplyDot(normal, outDir) >= 0);
        }

        private void displayVoronoi()
        {
            throw new NotImplementedException();
        }

    }
}
