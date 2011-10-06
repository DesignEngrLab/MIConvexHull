namespace MIConvexHull
{
    /// <summary>
    /// A convex face representation containing adjacency information.
    /// </summary>
    public abstract class ConvexFace<TVertex, TFace> 
        where TVertex : IVertex
        where TFace : ConvexFace<TVertex, TFace>
    {
        /// <summary>
        /// If the face is "at infinity", AdjacentFaces[i] = null.
        /// </summary>
        public TFace[] AdjacentFaces { get; set; }

        /// <summary>
        /// Face vertices.
        /// </summary>
        public TVertex[] Vertices { get; set; }

        /// <summary>
        /// Normal.
        /// </summary>
        public double[] Normal { get; set; }
    }

    public class DefaultConvexFace<TVertex> : ConvexFace<TVertex, DefaultConvexFace<TVertex>>
        where TVertex : IVertex
    {
    }
}
