namespace MIConvexHull
{
    /// <summary>
    /// To declare your own face type, use class Face : DelaunayFace(of Vertex, of Face)
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public abstract class TriangulationCell<TVertex, TCell> : ConvexFace<TVertex, TCell> 
        where TVertex : IVertex
        where TCell : ConvexFace<TVertex, TCell> 
    {

    }

    public class DefaultTriangulationCell<TVertex> : TriangulationCell<TVertex, DefaultTriangulationCell<TVertex>>
        where TVertex : IVertex
    {
    }
}
