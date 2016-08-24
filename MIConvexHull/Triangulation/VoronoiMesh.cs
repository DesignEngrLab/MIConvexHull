/******************************************************************************
 *
 * The MIT License (MIT)
 *
 * MIConvexHull, Copyright (c) 2015 David Sehnal, Matthew Campbell
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *  
 *****************************************************************************/

namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A factory class for creating a Voronoi mesh.
    /// </summary>
    public static class VoronoiMesh
    {
        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TCell"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<TVertex, TCell, TEdge> Create<TVertex, TCell, TEdge>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TCell : TriangulationCell<TVertex, TCell>, new()
            where TVertex : IVertex
            where TEdge : VoronoiEdge<TVertex, TCell>, new()
        {
            return VoronoiMesh<TVertex, TCell, TEdge>.Create(data, config);
        }

        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<TVertex, DefaultTriangulationCell<TVertex>>> Create<TVertex>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TVertex : IVertex
        {
            return VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<TVertex, DefaultTriangulationCell<TVertex>>>.Create(data, config);
        }

        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<DefaultVertex, DefaultTriangulationCell<DefaultVertex>, VoronoiEdge<DefaultVertex, DefaultTriangulationCell<DefaultVertex>>>
            Create(IList<double[]> data, TriangulationComputationConfig config = null)
        {
            var points = data.Select(p => new DefaultVertex { Position = p.ToArray() }).ToList();
            return VoronoiMesh<DefaultVertex, DefaultTriangulationCell<DefaultVertex>, VoronoiEdge<DefaultVertex, DefaultTriangulationCell<DefaultVertex>>>.Create(points, config);
        }

        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TCell"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<TVertex, TCell, VoronoiEdge<TVertex, TCell>> Create<TVertex, TCell>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TVertex : IVertex
            where TCell : TriangulationCell<TVertex, TCell>, new()
        {
            return VoronoiMesh<TVertex, TCell, VoronoiEdge<TVertex, TCell>>.Create(data, config);
        }
    }

    /// <summary>
    /// A representation of a voronoi mesh.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    public class VoronoiMesh<TVertex, TCell, TEdge>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
        where TEdge : VoronoiEdge<TVertex, TCell>, new()
    {
        /// <summary>
        /// This is probably not needed, but might make things a tiny bit faster.
        /// </summary>
        class EdgeComparer : IEqualityComparer<TEdge>
        {
            public bool Equals(TEdge x, TEdge y)
            {
                return (x.Source == y.Source && x.Target == y.Target) || (x.Source == y.Target && x.Target == y.Source);
            }

            public int GetHashCode(TEdge obj)
            {
                return obj.Source.GetHashCode() ^ obj.Target.GetHashCode();
            }
        }

        /// <summary>
        /// Vertices of the diagram.
        /// </summary>
        public IEnumerable<TCell> Vertices { get; private set; }

        /// <summary>
        /// Edges connecting the cells. 
        /// The same information can be retrieved Cells' Adjacency.
        /// </summary>
        public IEnumerable<TEdge> Edges { get; private set; }

        /// <summary>
        /// Create a Voronoi diagram of the input data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        public static VoronoiMesh<TVertex, TCell, TEdge> Create(IList<TVertex> data, TriangulationComputationConfig config)
        {
            if (data == null) throw new ArgumentNullException("data");
            
            var t = DelaunayTriangulation<TVertex, TCell>.Create(data, config);
            var vertices = t.Cells.ToList();
            var edges = new HashSet<TEdge>(new EdgeComparer());

            foreach (var f in vertices)
            {
                for (int i = 0; i < f.Adjacency.Length; i++)
                {
                    var af = f.Adjacency[i];
                    if (af != null) edges.Add(new TEdge { Source = f, Target = af });
                }
            }

            return new VoronoiMesh<TVertex, TCell, TEdge>
            {
                Vertices = vertices,
                Edges = edges.ToList()
            };
        }

        /// <summary>
        /// Can only be created using a factory method.
        /// </summary>
        private VoronoiMesh()
        {

        }
    }
}
