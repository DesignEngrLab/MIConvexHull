namespace MIConvexHullPluginNameSpace
{
    /// <summary>
    /// The interface for vertices that must be used in your program so that MIConvexHull can access you methods.
    /// </summary>
    public interface IVertexConvHull
    {
        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        double[] coordinates { get; set; }
    }

    /// <summary>
    /// The interface for faces that must be used in your program so that MIConvexHull can access you methods.
    /// </summary>
    public interface IFaceConvHull
    {
        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        /// <value>The vertices.</value>
        IVertexConvHull[] vertices { get; set; }
        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        /// <value>The normal.</value>
        double[] normal { get; set; }
    }


}
