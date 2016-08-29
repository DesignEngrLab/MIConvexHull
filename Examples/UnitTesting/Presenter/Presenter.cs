// ***********************************************************************
// Assembly         : TVGL Presenter
// Author           : Matt
// Created          : 05-20-2016
//
// Last Modified By : Matt
// Last Modified On : 05-24-2016
// ***********************************************************************
// <copyright file="Presenter.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************

using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using MIConvexHull;
using System;

namespace UnitTesting
{
    public static class Presenter
    {
        static ModelImporter modelImporter = new ModelImporter();
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
                        Material = MaterialHelper.CreateMaterial(new System.Windows.Media.Color { A = 189, G = 189, B = 189 })
                    }
            });
            window.view1.FitView(window.view1.Camera.LookDirection, window.view1.Camera.UpDirection);
            //window.Show();
            window.ShowDialog();
        }

        internal static Visual3D MakeModelVisual3D(string filename, out List<DefaultVertex> vertices)
        {

            var currentModel = modelImporter.Load(filename);
            var verts = new List<Point3D>();
            MeshGeometry3D mesh = null;
            foreach (var model in currentModel.Children)
            {
                if (typeof(GeometryModel3D).IsInstanceOfType(model))
                    if (typeof(MeshGeometry3D).IsInstanceOfType(((GeometryModel3D)model).Geometry))
                    {
                        mesh = (MeshGeometry3D)((GeometryModel3D)model).Geometry;
                        verts.AddRange(mesh.Positions);
                    }
            }
            vertices = verts.Distinct(new Point3DComparer()).Select(p =>
            new MIConvexHull.DefaultVertex { Position = new[] { p.X, p.Y, p.Z } }).ToList();


            return new ModelVisual3D { Content = currentModel.Children[0] };

        }



        internal class Point3DComparer : IEqualityComparer<Point3D>
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
                return ((x - y).Length < 0.00000000001);
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
}
