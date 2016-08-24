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
    /// <summary>
    /// A class representing an (undirected) edge of the Voronoi graph.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public class VoronoiEdge<TVertex, TCell>
        where TVertex : IVertex
        where TCell : TriangulationCell<TVertex, TCell>
    {
        /// <summary>
        /// Source of the edge.
        /// </summary>
        public TCell Source
        {
            get;
            internal set;
        }

        /// <summary>
        /// Target of the edge.
        /// </summary>
        public TCell Target
        {
            get;
            internal set;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as VoronoiEdge<TVertex, TCell>;
            if (other == null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return (Source == other.Source && Target == other.Target)
                || (Source == other.Target && Target == other.Source);
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 31 + Source.GetHashCode();
            return hash * 31 + Target.GetHashCode();
        }

        /// <summary>
        /// Create an instance of the edge.
        /// </summary>
        public VoronoiEdge()
        {

        }

        /// <summary>
        /// Create an instance of the edge.
        /// </summary>
        public VoronoiEdge(TCell source, TCell target)
        {
            this.Source = source;
            this.Target = target;
        }
    }
}
