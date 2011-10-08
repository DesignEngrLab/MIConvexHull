namespace MIConvexHull
{
    using System.Collections.Generic;

    /// <summary>
    /// Wraps each IVertex to allow marking of nodes.
    /// </summary>
    class VertexWrap
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
    class ConvexFaceInternal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvexFaceInternal"/> class.
        /// </summary>
        internal ConvexFaceInternal(int dimension, List<VertexWrap> beyondList)
        {
            AdjacentFaces = new ConvexFaceInternal[dimension];
            VerticesBeyond = beyondList;
            Normal = new double[dimension];
            Vertices = new VertexWrap[dimension];
            ListNode = new LinkedListNode<ConvexFaceInternal>(this);
        }

        /// <summary>
        /// Gets or sets the adjacent face data.
        /// </summary>
        public ConvexFaceInternal[] AdjacentFaces;

        /// <summary>
        /// Gets or sets the vertices beyond.
        /// </summary>
        public List<VertexWrap> VerticesBeyond;

        /// <summary>
        /// The furthest vertex.
        /// </summary>
        public VertexWrap FurthestVertex;

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
        /// Linked list node storing this face.
        /// </summary>
        public LinkedListNode<ConvexFaceInternal> ListNode;
    }
}
