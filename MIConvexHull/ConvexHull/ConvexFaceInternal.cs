namespace MIConvexHull
{
    using System.Collections.Generic;

    /// <summary>
    /// Wraps each IVertex to allow marking and indexing of nodes.
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
        /// <param name="dimension">The dimension.</param>
        internal ConvexFaceInternal(int dimension)
        {
            AdjacentFaces = new ConvexFaceInternal[dimension];
            VerticesBeyond = new List<VertexWrap>();
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
        public List<VertexWrap> VerticesBeyond;

        /// <summary>
        /// Buffer the "minimum" vertex
        /// </summary>
        public double MinVertexKey;
        public VertexWrap MinVertex;

        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        public VertexWrap[] Vertices;

        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        public double[] Normal;

        /// <summary>
        /// Used to traverse affected faces and create the Delaunay representation.
        /// </summary>
        public int Tag;

        /// <summary>
        /// FibonacciHeap cell storing this face.
        /// </summary>
        public FibonacciHeapCell<double, ConvexFaceInternal> FibCell;
    }
}
