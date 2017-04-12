/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the MIT License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     MIT License for more details.
 *  
 *     You should have received a copy of the MIT License
 *     along with MIConvexHull.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at https://designengrlab.github.io/MIConvexHull/
 *************************************************************************/

using System;
using MIConvexHull;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace BatchConvexHullTest
{
    using Path = List<Point>;
    using Paths = List<List<Point>>;

    internal class Program
    {
        static readonly ModelImporter modelImporter = new ModelImporter();

        [STAThread]
        private static void Main(string[] args)
        {
            var dir = new DirectoryInfo("../../../../TestFiles");
            var fileNames = dir.GetFiles();
            string filename = "";
            for (var i = 0; i < fileNames.Count(); i++)
            {
                try
                {
                    filename = fileNames[i].FullName;
                    Console.WriteLine("Attempting: " + filename);
                    List<DefaultVertex> vertices;
                    var v3D = MakeModelVisual3D(filename, out vertices);
                    var now = DateTime.Now;
                    var convexHull = ConvexHull.Create(vertices);
                    var interval = DateTime.Now - now;
                    Window3DPlot.ShowWithConvexHull(v3D, convexHull);
                    Console.WriteLine("time = " + interval);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed on " + filename + ": " + e.Message);
                }
            }
            Console.ReadLine();
        }

        static Visual3D MakeModelVisual3D(string filename, out List<DefaultVertex> vertices)
        {

            var currentModel = modelImporter.Load(filename);
            var verts = new List<Point3D>();
            foreach (var model in currentModel.Children)
            {
                if (model is GeometryModel3D)
                    if (((GeometryModel3D)model).Geometry is MeshGeometry3D)
                    {
                        var mesh = (MeshGeometry3D)((GeometryModel3D)model).Geometry;
                        verts.AddRange(mesh.Positions);
                    }
            }
            vertices = verts.Distinct(new Point3DComparer()).Select(p =>
                new MIConvexHull.DefaultVertex { Position = new[] { p.X, p.Y, p.Z } }).ToList();
            return new ModelVisual3D { Content = currentModel }; //.Children[0] };

        }
    }


    class Point3DComparer : IEqualityComparer<Point3D>
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
            return ((x - y).Length < 1e-15);
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
