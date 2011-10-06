namespace MIConvexHull
{
    public class VoronoiEdge<TVertex, TCell>
        where TVertex : IVertex
        where TCell : TriangulationCell<TVertex, TCell>
    {

        public TCell Source
        {
            get;
            internal set;
        }

        public TCell Target
        {
            get;
            internal set;
        }

        public VoronoiEdge()
        {

        }
    }
}
