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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Simple interface to unify different types of triangulations in the future.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public interface ITriangulation<TVertex, TCell>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
    {
        /// <summary>
        /// Triangulation simplexes. For 2D - triangles, 3D - tetrahedrons, etc ...
        /// </summary>
        IEnumerable<TCell> Cells { get; }
    }

    /// <summary>
    /// Factory class for creating triangulations.
    /// </summary>
    public static class Triangulation
    {
        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static ITriangulation<TVertex, DefaultTriangulationCell<TVertex>> CreateDelaunay<TVertex>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TVertex : IVertex
        {
            return DelaunayTriangulation<TVertex, DefaultTriangulationCell<TVertex>>.Create(data, config);
        }

        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static ITriangulation<DefaultVertex, DefaultTriangulationCell<DefaultVertex>> CreateDelaunay(IList<double[]> data, TriangulationComputationConfig config = null)
        {
            var points = data.Select(p => new DefaultVertex { Position = p.ToArray() }).ToList();
            return DelaunayTriangulation<DefaultVertex, DefaultTriangulationCell<DefaultVertex>>.Create(points, config);
        }

        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static ITriangulation<TVertex, TFace> CreateDelaunay<TVertex, TFace>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TVertex : IVertex
            where TFace : TriangulationCell<TVertex, TFace>, new()
        {
            return DelaunayTriangulation<TVertex, TFace>.Create(data, config);
        }


        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TCell"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<TVertex, TCell, TEdge> CreateVoronoi<TVertex, TCell, TEdge>(IList<TVertex> data, TriangulationComputationConfig config = null)
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
        public static VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<TVertex, DefaultTriangulationCell<TVertex>>> CreateVoronoi<TVertex>(IList<TVertex> data, TriangulationComputationConfig config = null)
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
            CreateVoronoi(IList<double[]> data, TriangulationComputationConfig config = null)
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
        public static VoronoiMesh<TVertex, TCell, VoronoiEdge<TVertex, TCell>> CreateVoronoi<TVertex, TCell>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TVertex : IVertex
            where TCell : TriangulationCell<TVertex, TCell>, new()
        {
            return VoronoiMesh<TVertex, TCell, VoronoiEdge<TVertex, TCell>>.Create(data, config);
        }
    }
}
