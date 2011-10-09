namespace MIConvexHull
{
    /// <summary>
    /// Wraps each IVertex to allow marking of nodes.
    /// </summary>
    sealed class VertexWrap
    {
        /// <summary>
        /// Ref. to the original vertex.
        /// </summary>
        public IVertex Vertex;

        /// <summary>
        /// Direct reference to PositionData makes IsVertexOverFace faster.
        /// </summary>
        public double[] PositionData;

        /// <summary>
        /// Used mostly to enumerate unique vertices.
        /// </summary>
        public bool Marked;
    }

    /// <summary>
    /// This internal class manages the faces of the convex hull. It is a 
    /// separate class from the desired user class.
    /// </summary>
    sealed class ConvexFaceInternal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvexFaceInternal"/> class.
        /// </summary>
        public ConvexFaceInternal(int dimension, VertexBuffer beyondList)
        {
            AdjacentFaces = new ConvexFaceInternal[dimension];
            VerticesBeyond = beyondList;
            Normal = new double[dimension];
            Vertices = new VertexWrap[dimension];
        }

        /// <summary>
        /// Gets or sets the adjacent face data.
        /// </summary>
        public ConvexFaceInternal[] AdjacentFaces;

        /// <summary>
        /// Gets or sets the vertices beyond.
        /// </summary>
        public VertexBuffer VerticesBeyond;

        /// <summary>
        /// The furthest vertex.
        /// </summary>
        public VertexWrap FurthestVertex;

        /// <summary>
        /// Distance to the furthest vertex.
        /// </summary>
        public double FurthestDistance;

        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        public VertexWrap[] Vertices;

        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        public double[] Normal;

        /// <summary>
        /// Face plane constant element.
        /// </summary>
        public double Offset;

        /// <summary>
        /// Used to traverse affected faces and create the Delaunay representation.
        /// </summary>
        public int Tag;

        /// <summary>
        /// Prev node in the list.
        /// </summary>
        public ConvexFaceInternal Previous;

        /// <summary>
        /// Next node in the list.
        /// </summary>
        public ConvexFaceInternal Next;

        /// <summary>
        /// Is it present in the list.
        /// </summary>
        public bool InList;
    }
}
