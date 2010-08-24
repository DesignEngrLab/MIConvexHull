using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIConvexHullPluginNameSpace
{
    internal class FaceData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="defFaceClass"/> class.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        public FaceData(int dimension)
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
        public FaceData[] adjacentFaces { get; set; }
        /// <summary>
        /// Gets or sets the vertices beyond.
        /// </summary>
        /// <value>The vertices beyond.</value>
        public SortedList<double, IVertexConvHull> verticesBeyond { get; set; }
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
    }
}
