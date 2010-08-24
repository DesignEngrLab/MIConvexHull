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
        double[] location { get; }
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


}
