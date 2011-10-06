namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class VoronoiMesh
    {
        public static VoronoiMesh<TVertex, TCell, TEdge> Create<TVertex, TCell, TEdge>(IEnumerable<TVertex> data)
            where TCell : TriangulationCell<TVertex, TCell>, new()
            where TVertex : IVertex
            where TEdge : VoronoiEdge<TVertex, TCell>, new()
        {
            return VoronoiMesh<TVertex, TCell, TEdge>.Create(data);
        }

        public static VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<TVertex, DefaultTriangulationCell<TVertex>>> Create<TVertex>(IEnumerable<TVertex> data)
            where TVertex : IVertex
        {
            return VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<TVertex, DefaultTriangulationCell<TVertex>>>.Create(data);
        }

        public static VoronoiMesh<TVertex, TCell, VoronoiEdge<TVertex, TCell>> Create<TVertex, TCell>(IEnumerable<TVertex> data)
            where TVertex : IVertex
            where TCell : TriangulationCell<TVertex, TCell>, new()
        {
            return VoronoiMesh<TVertex, TCell, VoronoiEdge<TVertex, TCell>>.Create(data);
        }
    }

    public class VoronoiMesh<TVertex, TCell, TEdge>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
        where TEdge : VoronoiEdge<TVertex, TCell>, new()
    {
        class EdgeComparer : IEqualityComparer<TEdge>
        {
            public bool Equals(TEdge x, TEdge y)
            {
                return (x.Source == y.Source && x.Target == y.Target) || (x.Source == y.Target && x.Target == y.Source);
            }

            public int GetHashCode(TEdge obj)
            {
                return obj.Source.GetHashCode() ^ obj.Target.GetHashCode();
            }
        }

        public IEnumerable<TCell> Cells { get; private set; }
        public IEnumerable<TEdge> Edges { get; private set; }
        
        /// <summary>
        /// This omits the "infinite faces"
        /// </summary>
        /// <param name="data"></param>
        /// <param name="vertices"></param>
        /// <param name="edges"></param>
        public static VoronoiMesh<TVertex, TCell, TEdge> Create(IEnumerable<TVertex> data)
        {
            if (data == null) throw new ArgumentNullException("data can't be null");

            var t = DelaunayTriangulation<TVertex, TCell>.Create(data); 
            var vertices = t.Cells;
            var edges = new HashSet<TEdge>(new EdgeComparer());
            
            foreach (var f in vertices)
            {
                for (int i = 0; i < f.AdjacentFaces.Length; i++)
                {
                    var af = f.AdjacentFaces[i];
                    if (af != null) edges.Add(new TEdge { Source = f, Target = af });
                }
            }

            ////for (var i = 0; i < delaunayFaces.Count; i++)
            ////    for (var j = 0; j < delaunayFaces[i].adjacentFaces.GetLength(0); j++)
            ////        if (!delaunayFaces.Contains(delaunayFaces[i].adjacentFaces[j]))
            ////        {
            ////            var edgeNodes = new List<IVertexConvHull>(delaunayFaces[i].vertices);
            ////            edgeNodes.RemoveAt(j);
            ////            var avg = new double[dimension];
            ////            avg = edgeNodes.Aggregate(avg, (current, v) => StarMath.add(current, v.coordinates));
            ////            avg = StarMath.divide(avg, dimension);
            ////            voronoiNodes.Add(makeNewVoronoiEdge(avg, node_Type));
            ////            voronoiEdges.Add(Tuple.Create(voronoiNodes[i], voronoiNodes[voronoiNodes.Count - 1]));
            ////        }

            return new VoronoiMesh<TVertex, TCell, TEdge> 
            { 
                Cells = vertices, 
                Edges = edges.ToList() 
            };
        }

        private VoronoiMesh()
        {

        }
    }
}
