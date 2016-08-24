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
    using System;

    /// <summary>
    /// Calculation and representation of Delaunay triangulation.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public class DelaunayTriangulation<TVertex, TCell> : ITriangulation<TVertex, TCell>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
    {
        /// <summary>
        /// Cells of the triangulation.
        /// </summary>
        public IEnumerable<TCell> Cells { get; private set; }
        
        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig is used.</param>
        /// <returns></returns>
        public static DelaunayTriangulation<TVertex, TCell> Create(IList<TVertex> data, TriangulationComputationConfig config)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Count == 0) return new DelaunayTriangulation<TVertex, TCell> { Cells = new TCell[0] };

            config = config ?? new TriangulationComputationConfig();       
            var cells = ConvexHullInternal.GetDelaunayTriangulation<TVertex, TCell>(data, config);

            return new DelaunayTriangulation<TVertex, TCell> { Cells = cells };
        }

        /// <summary>
        /// Can only be created using a factory method.
        /// </summary>
        private DelaunayTriangulation()
        {

        }
    }
}
