using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIConvexHullPluginNameSpace
{
    /// <summary>
    /// The interface for vertices that must be used in your program so that MIConvexHull can access you methods.
    /// </summary>
    public interface IVertexConvHull
    {
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        double[] location { get; set; }
    }

    /// <summary>
    /// The interface for faces that must be used in your program so that MIConvexHull can access you methods.
    /// </summary>
    public interface IFaceConvHull
    {
        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        /// <value>The vertex, v1.</value>
        IVertexConvHull[] vertices { get; set; }
        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        /// <value>The normal.</value>
        double[] normal { get; set; }
    }


    /// <summary>
    /// A IVertexConvHull is a simple class that stores the postion of a point, node or IVertexConvHull.
    /// </summary>
    public class defFaceClass : IFaceConvHull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="defFaceClass"/> class.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        public defFaceClass(int dimension)
        {
            vertices = new IVertexConvHull[dimension];
            normal = new double[dimension];
        }
        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        /// <value>The vertex, v1.</value>
        public IVertexConvHull[] vertices { get;  set; }
        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        /// <value>The normal.</value>
        public double[] normal { get;  set; }
    }
}
