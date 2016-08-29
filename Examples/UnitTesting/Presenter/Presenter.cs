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

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace TVGL
{
    /// <summary>
    ///     The Class HelixPresenter is the only class within the TVGL Helix Presenter
    ///     project (TVGL_Presenter.dll). It is a simple static class with one main
    ///     function, "Show".
    /// </summary>
    public static class Presenter
    {
        public static void ShowWithConvexHull(TessellatedSolid ts)
        {
            var window = new Window3DPlot();
            window.view1.Children.Add(MakeModelVisual3D(ts));
            var positions =
         ts.ConvexHull.Faces.SelectMany(
             f => f.Vertices.Select(v => new Point3D(v.Position[0], v.Position[1], v.Position[2])));
            var normals =
                ts.ConvexHull.Faces.SelectMany(f => f.Vertices.Select(v => new Vector3D(f.Normal[0], f.Normal[1], f.Normal[2])));
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


        /// <summary>
        ///     Makes the model visual3 d.
        /// </summary>
        /// <param name="ts">The ts.</param>
        /// <returns>Visual3D.</returns>
        private static Visual3D MakeModelVisual3D(string filename)
        {
            var model = ModelImporter.Load(fileName);
            return new Visual3D(model.Children[0]);
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
            vertices = verts.Distinct(new Point3DComparer()).Select(p => new Vertex(p)).ToList();


            var defaultMaterial = MaterialHelper.CreateMaterial(
                new System.Windows.Media.Color
                {
                    A = ts.SolidColor.A,
                    B = ts.SolidColor.B,
                    G = ts.SolidColor.G,
                    R = ts.SolidColor.R
                });
            if (ts.HasUniformColor)
            {
                var positions =
                    ts.Faces.SelectMany(
                        f => f.Vertices.Select(v => new Point3D(v.Position[0], v.Position[1], v.Position[2])));
                var normals =
                    ts.Faces.SelectMany(f => f.Vertices.Select(v => new Vector3D(f.Normal[0], f.Normal[1], f.Normal[2])));
                return new ModelVisual3D
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
                            Material = defaultMaterial
                        }
                };
            }
            var result = new ModelVisual3D();
            foreach (var f in ts.Faces)
            {
                var vOrder = new Point3DCollection();
                for (var i = 0; i < 3; i++)
                    vOrder.Add(new Point3D(f.Vertices[i].X, f.Vertices[i].Y, f.Vertices[i].Z));

                var c = f.Color == null
                    ? defaultMaterial
                    : MaterialHelper.CreateMaterial(new System.Windows.Media.Color
                    {
                        A = f.Color.A,
                        B = f.Color.B,
                        G = f.Color.G,
                        R = f.Color.R
                    });
                result.Children.Add(new ModelVisual3D
                {
                    Content =
                        new GeometryModel3D
                        {
                            Geometry = new MeshGeometry3D { Positions = vOrder },
                            Material = c
                        }
                });
            }
            return result;
        }
            }
}