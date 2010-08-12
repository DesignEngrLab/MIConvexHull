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
        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>The X.</value>
        double X { get; set; }
        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>The Y.</value>
        double Y { get; set; }
        /// <summary>
        /// Gets or sets the Z.
        /// </summary>
        /// <value>The Z.</value>
        double Z { get; set; }

    }

    /// <summary>
    /// The interface for faces that must be used in your program so that MIConvexHull can access you methods.
    /// </summary>
    public interface IFaceConvHull
    {
        /// <summary>
        /// Gets or sets the vertex, v1.
        /// </summary>
        /// <value>The vertex, v1.</value>
        IVertexConvHull v1 { get; set; }
        /// <summary>
        /// Gets or sets the vertex, v2.
        /// </summary>
        /// <value>The vertex, v2.</value>
        IVertexConvHull v2 { get; set; }
        /// <summary>
        /// Gets or sets the vertex, v3.
        /// </summary>
        /// <value>The vertex, v3.</value>
        IVertexConvHull v3 { get; set; }
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
        /// Gets or sets the vertex, v1.
        /// </summary>
        /// <value>The vertex, v1.</value>
        public IVertexConvHull v1 { get;  set; }
        /// <summary>
        /// Gets or sets the vertex, v2.
        /// </summary>
        /// <value>The vertex, v2.</value>
        public IVertexConvHull v2 { get;  set; }
        /// <summary>
        /// Gets or sets the vertex, v3.
        /// </summary>
        /// <value>The vertex, v3.</value>
        public IVertexConvHull v3 { get;  set; }
        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        /// <value>The normal.</value>
        public double[] normal { get;  set; }
        /// <summary>
        /// Gets or sets the center vector.
        /// </summary>
        /// <value>The center.</value>
        public double[] center { get;  set; }

    }
    internal class defVertexClass : IVertexConvHull
    {
        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>The X position.</value>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>The Y position.</value>
        public double Y { get; set; }


        /// <summary>
        /// Gets or sets the Z. Not used by MIConvexHull2D.
        /// </summary>
        /// <value>The Z position.</value>
        public double Z { get; set; }


        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public double[] location
        {
            get { return new double[] { X, Y, Z }; }
            set
            {
                X = value[0];
                Y = value[1];
                Z = value[2];
            }

        }
    }

}
