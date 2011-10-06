namespace MIConvexHull
{
    using System.Collections.Generic;
    using System.Linq;
    using System;

    /// <summary>
    /// Calculation and representation of Delaunay triangulation.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public class DelaunayTriangulation<TVertex, TCell> : ITriangulation<TVertex, TCell>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
    {
        public IEnumerable<TCell> Cells { get; private set; }

        public static DelaunayTriangulation<TVertex, TCell> Create(IEnumerable<TVertex> data)
        {
            if (data.Count() == 0) return new DelaunayTriangulation<TVertex, TCell> { Cells = Enumerable.Empty<TCell>() };

            int dimension = data.First().Position.Length;

            foreach (var p in data)
            {
                double lenSq = StarMath.norm2(p.Position, true);
                var v = p.Position;
                Array.Resize(ref v, dimension + 1);
                p.Position = v;
                p.Position[dimension] = lenSq;
            }

            var delaunayFaces = ConvexHullInternal.GetConvexFacesInternal<TVertex, TCell>(data);

            foreach (var p in data)
            {
                var v = p.Position;
                Array.Resize(ref v, dimension);
                p.Position = v;
            }

            for (var i = delaunayFaces.Count - 1; i >= 0; i--)
            {
                var candidate = delaunayFaces[i];
                if (candidate.Normal[dimension] >= 0)
                {
                    for (int fi = 0; fi < candidate.AdjacentFaces.Length; fi++)
                    {
                        var f = candidate.AdjacentFaces[fi];
                        if (f != null)
                        {
                            for (int j = 0; j < f.AdjacentFaces.Length; j++)
                            {
                                if (object.ReferenceEquals(f.AdjacentFaces[j], candidate))
                                {
                                    f.AdjacentFaces[j] = null;
                                }
                            }
                        }
                    }
                    delaunayFaces.RemoveAt(i);
                }
            }

            #region Create TFace List
            int cellCount = delaunayFaces.Count;
            var cells = new TCell[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var face = delaunayFaces[i];
                var vertices = new TVertex[dimension + 1];
                for (int j = 0; j <= dimension; j++) vertices[j] = (TVertex)face.Vertices[j].Vertex;
                cells[i] = new TCell
                {
                    Vertices = vertices,
                    AdjacentFaces = new TCell[dimension + 1],
                    Normal = face.Normal
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                var face = delaunayFaces[i];
                var cell = cells[i];
                for (int j = 0; j <= dimension; j++)
                {
                    if (face.AdjacentFaces[j] == null) continue;
                    cell.AdjacentFaces[j] = cells[face.AdjacentFaces[j].Tag];
                }
            }
            #endregion

            return new DelaunayTriangulation<TVertex, TCell> { Cells = cells };
        }

        private DelaunayTriangulation()
        {

        }
    }
}
