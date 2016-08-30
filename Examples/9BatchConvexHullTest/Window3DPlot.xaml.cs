// ***********************************************************************
// Assembly         : TVGL Presenter
// Author           : Matt
// Created          : 05-20-2016
//
// Last Modified By : Matt
// Last Modified On : 05-18-2016
// ***********************************************************************
// <copyright file="Window3DPlot.xaml.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using MIConvexHull;


namespace BatchConvexHullTest
{
    /// <summary>
    ///     Class Window3DPlot.
    /// </summary>
    public partial class Window3DPlot : Window
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Window3DPlot" /> class.
        /// </summary>
        public Window3DPlot()
        {
            InitializeComponent();
        }


        internal static void ShowWithConvexHull(Visual3D v3D, ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>> convexHull)
        {
            var window = new Window3DPlot();
            window.view1.Children.Add(v3D);
            var positions =
                convexHull.Faces.SelectMany(
             f => f.Vertices.Select(v => new Point3D(v.Position[0], v.Position[1], v.Position[2])));
            var normals =
                convexHull.Faces.SelectMany(f => f.Vertices.Select(v => new Vector3D(f.Normal[0], f.Normal[1], f.Normal[2])));
            window.view1.Children.Add(
            new ModelVisual3D
            {
                Content =
                    new GeometryModel3D
                    {
                        Geometry = new MeshGeometry3D
                        {
                            Positions = new Point3DCollection(positions),
                            // TriangleIndices = new Int32Collection(triIndices),
                            Normals = new Vector3DCollection(normals)
                        },
                        Material = MaterialHelper.CreateMaterial(new System.Windows.Media.Color { A = 130, G = 189, R = 189 })
                    }
            });
            window.view1.FitView(window.view1.Camera.LookDirection, window.view1.Camera.UpDirection);
            //window.Show();
            window.ShowDialog();
        }

    }
}