namespace MIConvexHullPluginNameSpace
{
    /// <summary>
    /// A default vertex class in cases where the user provides 
    /// only a vector of coordinates.
    /// </summary>
    internal class defaultVertex : IVertexConvHull
    {
        public double[] coordinates { get; set; }

    }
}
