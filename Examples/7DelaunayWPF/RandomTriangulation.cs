﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using MIConvexHull;
using System.Windows.Media;
using Petzold.Media3D;

namespace DelaunayWPF
{
    /// <summary>
    /// This class represents 3D triangulation of random data.
    /// </summary>
    class RandomTriangulation : ModelVisual3D
    {   
        IEnumerable<Tetrahedron> tetrahedrons;

        /// <summary>
        /// The count of the tetrahedrons.
        /// </summary>
        public int Count { get { return tetrahedrons.Count(); } }
        
        /// <summary>
        /// Creates a triangulation of random data.
        /// For larger data sets it might be a good idea to separate the triangulation calculation
        /// from creating the visual so that the triangulation can be computed on a separate threat.
        /// </summary>
        /// <param name="count">Number of vertices to generate</param>
        /// <param name="radius">Radius of the vertices</param>
        /// <returns>Triangulation</returns>
        public static RandomTriangulation Create(int count, double radius)
        {
            Random rnd = new Random();

            // generate some random points
            Func<double> nextRandom = () => 2 * radius * rnd.NextDouble() - radius;
            var vertices = Enumerable.Range(0, count)
                .Select(_ => new Vertex(nextRandom(), nextRandom(), nextRandom()))
                .ToList();

            //var vertices = new List<Vertex>();
            //int d = 4;
            //double cs = 10.0;
            //for (int i = 0; i < d; i++)
            //{
            //    for (int j = 0; j < d; j++)
            //    {
            //        for (int k = 0; k < d; k++)
            //        {
            //            //vertices.Add(new Vertex(10 * i - 20 + rnd.NextDouble(), 10 * j - 20 - rnd.NextDouble(), 10 * k - 20 + rnd.NextDouble()));
            //            vertices.Add(new Vertex(-10 * i, 10 * j, 10 * k));
            //        }
            //    }
            //}

            // calculate the triangulation
            var tetrahedrons = Triangulation.CreateDelaunay<Vertex, Tetrahedron>(vertices).Cells;

            // create a model for each tetrahedron, pick a random color
            Model3DGroup model = new Model3DGroup();
            foreach (var t in tetrahedrons)            
            {
                var color = Color.FromArgb((byte)255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
                model.Children.Add(t.CreateModel(color, radius));
            }

            var redMaterial = new MaterialGroup 
            { 
                Children = new MaterialCollection
                {
                    new DiffuseMaterial(Brushes.Red),
                    // give it some shine
                    new SpecularMaterial(Brushes.LightYellow, 2.0) 
                } 
            };

            var greenMaterial = new MaterialGroup
            {
                Children = new MaterialCollection
                {
                    new DiffuseMaterial(Brushes.Green),
                    // give it some shine
                    new SpecularMaterial(Brushes.LightYellow, 2.0) 
                }
            };

            var blueMaterial = new MaterialGroup
            {
                Children = new MaterialCollection
                {
                    new DiffuseMaterial(Brushes.Blue),
                    // give it some shine
                    new SpecularMaterial(Brushes.LightYellow, 2.0) 
                }
            };

            CylinderMesh c = new CylinderMesh() { Length = 10, Radius = 0.5 };
            model.Children.Add(new GeometryModel3D { Geometry = c.Geometry, Material = greenMaterial });
            model.Children.Add(new GeometryModel3D { Geometry = c.Geometry, Material = redMaterial, Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90)) });
            model.Children.Add(new GeometryModel3D { Geometry = c.Geometry, Material = blueMaterial, Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90)) });

            var triangulation = new RandomTriangulation();
            triangulation.tetrahedrons = tetrahedrons;

            // assign the Visual3DModel property of the ModelVisual3D class
            triangulation.Visual3DModel = model;

            return triangulation;
        }

        /// <summary>
        /// Begins the expand animation.
        /// </summary>
        public void Expand()
        {
            foreach (var t in tetrahedrons) t.Expand();
        }

        /// <summary>
        /// Begins the expand random animation.
        /// </summary>
        public void ExpandRandom()
        {
            foreach (var t in tetrahedrons) t.ExpandRandom();
        }

        /// <summary>
        /// Begins the collapse animation.
        /// </summary>
        public void Collapse()
        {
            foreach (var t in tetrahedrons) t.Collapse();
        }

        /// <summary>
        /// Begins the pulse animation.
        /// </summary>
        public void Pulse()
        {
            foreach (var t in tetrahedrons) t.Pulse();
        }
    }
}
