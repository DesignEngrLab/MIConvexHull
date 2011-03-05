using System.Collections.Generic;
using System;

namespace MIConvexHull
{
    /// <summary>
    /// This internal class manages the faces of the convex hull. It is a 
    /// separate class from the desired user class.
    /// </summary>
    internal class FaceData : IFaceConvHull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FaceData"/> class.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        internal FaceData(int dimension)
        {
            adjacentFaces = new FaceData[dimension];
            /* the following initiations are commented out because they are redundant with other parts
             * of the code that assign them. */
            //normal = new double[dimension];
            //vertices = new IVertexConvHull[dimension];
            //verticesBeyond = new SortedList<double, IVertexConvHull>(new noEqualSortMaxtoMinDouble());
        }
        /// <summary>
        /// Gets or sets the adjacent face data.
        /// </summary>
        /// <value>The adjacent face data.</value>
        public FaceData[] adjacentFaces { get; private set; }
        /// <summary>
        /// Gets or sets the vertices beyond.
        /// </summary>
        /// <value>The vertices beyond.</value>
        public HashSet<IVertexConvHull> verticesBeyond { get; set; }
        /// <summary>
        /// Gets or sets the "minimum" vertex.
        /// </summary>
        public Tuple<double, IVertexConvHull> minVertexBeyond;
        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        /// <value>The vertex, v1.</value>
        public IVertexConvHull[] vertices { get; set; }
        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        /// <value>The normal.</value>
        public double[] normal { get; set; }
        /// <summary>
        /// Gets or sets the fibonacci heap cell representing this FaceData
        /// </summary>
        public FibonacciHeapCell<double, FaceData> fibCell { get; set; }
    }
}
