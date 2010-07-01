using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIConvexHull
{
    public interface IVertexConvHull
    {
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }

    }

    public interface IFaceConvHull
    {
        IVertexConvHull v1 { get; set; }
        IVertexConvHull v2 { get; set; }
        IVertexConvHull v3 { get; set; }
        double[] normal { get; set; }
        double[] center { get; set; }
    }


    /// <summary>
    /// A IVertexConvHull is a simple class that stores the postion of a point, node or IVertexConvHull.
    /// </summary>
    public class defFaceClass : IFaceConvHull
    {
        public IVertexConvHull v1 { get;  set; }
        public IVertexConvHull v2 { get;  set; }
        public IVertexConvHull v3 { get;  set; }
        public double[] normal { get;  set; }
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
    }

}
