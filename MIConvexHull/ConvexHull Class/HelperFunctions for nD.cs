#region

using System;
using System.Collections.Generic;
using System.Linq;
using StarMathLib;

#endregion

namespace MIConvexHull
{
    /// <summary>
    ///   functions called from Find for the 3D case.
    /// </summary>
    public partial class ConvexHull
    {
        private void determineDimension(IList<IVertexConvHull> vertices)
        {
            var r = new Random();
            var VCount = vertices.Count;
            var dimensions = new List<int>();
            for (var i = 0; i < 10; i++)
                dimensions.Add(vertices[r.Next(VCount)].coordinates.GetLength(0));
            dimension = dimensions.Min();
            if (dimensions.Min() != dimensions.Max())
                Console.WriteLine("\n\n\n*******************************************\n" +
                                  "Differing dimensions to vertex locations." +
                                  "\nBased on a small sample, a value of " +
                                  dimension + "  will be used." +
                                  "\n*******************************************\n\n\n");
        }

        #region Ternary Counter functions

        private Boolean incrementTernaryPosition(int[] ternaryPosition, int position = 0)
        {
            if (position == ternaryPosition.GetLength(0)) return false;
            ternaryPosition[position]++;
            if (ternaryPosition[position] == 2)
            {
                ternaryPosition[position] = -1;
                return incrementTernaryPosition(ternaryPosition, ++position);
            }
            return true;
        }

        private int findIndex(IList<int> ternaryPosition, int midPoint)
        {
            var index = midPoint;
            var power = 1;
            for (var i = 0; i < dimension; i++)
            {
                index += power * ternaryPosition[i];
                power *= 3;
            }
            return index;
        }
        #endregion

        #region Make functions

        private SortedList<double, FaceData> initiateFaceDatabase()
        {
            for (var i = 0; i < dimension + 1; i++)
            {
                var vertices = new List<IVertexConvHull>(convexHull);
                vertices.RemoveAt(i);
                var newFace = MakeFace(vertices);
                /* the next line initialization of "verticesBeyond" is just to allow the line of
                 * code in updateFaces ("edge.Item2.verticesBeyond.Values).ToList());")
                 * to not crash when filling out the initial polygon. */
                newFace.verticesBeyond = new SortedList<double, IVertexConvHull>();
                convexFaces.Add(0.0, newFace);
            }
            for (var i = 0; i < dimension; i++)
                for (var j = i + 1; j < dimension + 1; j++)
                {
                    var edge = new List<IVertexConvHull>(convexHull);
                    edge.RemoveAt(j);
                    edge.RemoveAt(i);
                    var betweenFaces = (from f in convexFaces.Values
                                        where f.vertices.Intersect(edge).Count() == edge.Count()
                                        select f).ToList();
                    recordAdjacentFaces(betweenFaces[0], betweenFaces[1], edge);
                }
            return convexFaces;
        }

        private static void recordAdjacentFaces(FaceData face1, FaceData face2, ICollection<IVertexConvHull> edge)
        {
            var vertexIndexNotOnEdge = (from v in face1.vertices
                                        where (!edge.Contains(v))
                                        select Array.IndexOf(face1.vertices, v)).FirstOrDefault();
            face1.adjacentFaces[vertexIndexNotOnEdge] = face2;

            vertexIndexNotOnEdge = (from v in face2.vertices
                                    where (!edge.Contains(v))
                                    select Array.IndexOf(face2.vertices, v)).FirstOrDefault();
            face2.adjacentFaces[vertexIndexNotOnEdge] = face1;
        }


        private FaceData MakeFace(IVertexConvHull currentVertex, IEnumerable<IVertexConvHull> edge)
        {
            var vertices = new List<IVertexConvHull>(edge);
            vertices.Insert(0, currentVertex);
            return MakeFace(vertices);
        }

        private FaceData MakeFace(List<IVertexConvHull> vertices)
        {
            var outDir = new double[dimension];
            outDir = vertices.Aggregate(outDir, (current, v) => StarMath.add(current, v.coordinates));
            outDir = StarMath.divide(outDir, dimension);
            outDir = StarMath.subtract(outDir, center);
            var normal = findNormalVector(vertices);
            if (StarMath.multiplyDot(normal, outDir) < 0)
            {
                normal = StarMath.subtract(StarMath.makeZeroVector(dimension), normal);
                if (dimension == 3) vertices.Reverse();
            }
            var newFace = new FaceData(dimension)
                              {
                                  normal = normal,
                                  vertices = vertices.ToArray()
                              };
            return newFace;
        }

        #endregion

        #region Find, Get and Update functions

        private IEnumerable<FaceData> findFacesBeneathInitialVertices(IVertexConvHull currentVertex)
        {
            var facesUnder = new List<FaceData>();
            foreach (var face in convexFaces.Values)
            {
                double dummy;
                if (isVertexOverFace(currentVertex, face, out dummy))
                    facesUnder.Add(face);
            }
            return facesUnder;
        }

        private static Boolean isVertexOverFace(IVertexConvHull v, IFaceConvHull f, out double dotP)
        {
            dotP = StarMath.multiplyDot(f.normal, StarMath.subtract(v.coordinates, f.vertices[0].coordinates));
            return (dotP >= 0);
        }

        private static List<FaceData> findAffectedFaces(FaceData currentFaceData, IVertexConvHull currentVertex,
                                                        List<FaceData> primaryFaces = null)
        {
            if (primaryFaces == null)
                return findAffectedFaces(currentFaceData, currentVertex, new List<FaceData> { currentFaceData });
            foreach (var adjFace in
                currentFaceData.adjacentFaces.Where(anyFace => !primaryFaces.Contains(anyFace)
                    && (anyFace.verticesBeyond.Values.Contains(currentVertex))))
            {
                primaryFaces.Add(adjFace);
                findAffectedFaces(adjFace, currentVertex, primaryFaces);
            }
            return primaryFaces;
        }

        private void updateFaces(IEnumerable<FaceData> oldFaces, IVertexConvHull currentVertex)
        {
            var newFaces = new List<FaceData>();
            var affectedVertices = new List<IVertexConvHull>();
            affectedVertices = oldFaces.Aggregate(affectedVertices, (current, oldFace)
                 => current.Union(oldFace.verticesBeyond.Values).ToList());
            affectedVertices.Remove(currentVertex);
            /******************************************************
             ********* This appears to be the bottleneck for higher dimensions. 
             **** I believe it can be improved. */
            foreach (var oldFace in oldFaces)
            {
                convexFaces.RemoveAt(convexFaces.IndexOfValue(oldFace));
                for (var i = 0; i < oldFace.adjacentFaces.GetLength(0); i++)
                    if (!oldFaces.Contains(oldFace.adjacentFaces[i]))
                    {
                        var edge = new List<IVertexConvHull>(oldFace.vertices);
                        edge.RemoveAt(i);
                        var newFace = MakeFace(currentVertex, edge);
                        recordAdjacentFaces(newFace, oldFace.adjacentFaces[i], edge);
                        newFace.verticesBeyond = findBeyondVertices(newFace,
                                                                    affectedVertices.Union(
                                                                        oldFace.adjacentFaces[i].verticesBeyond.Values).
                                                                        ToList());
                        newFaces.Add(newFace);
                    }
            }
            /**************************************************************************/
            for (var i = 0; i < newFaces.Count - 1; i++)
            {
                for (var j = i + 1; j < newFaces.Count; j++)
                {
                    var edge = newFaces[i].vertices.Intersect(newFaces[j].vertices).ToList();
                    if (edge.Count == dimension - 1)
                        recordAdjacentFaces(newFaces[i], newFaces[j], edge);
                    if (!newFaces[i].adjacentFaces.Contains(null)) break;
                }
            }
            foreach (var newFace in newFaces)
                if (newFace.verticesBeyond.Count == 0)
                {
                    if (Status.TaskNumber == 4) Status.SubTaskNumber++;
                    convexFaces.Add(-1.0, newFace);
                }
                else convexFaces.Add(newFace.verticesBeyond.Keys[0], newFace);
        }

        private double[] findNormalVector(IList<IVertexConvHull> vertices)
        {
            double[] normal;
            if (dimension == 3)
                normal = StarMath.multiplyCross(StarMath.subtract(vertices[1].coordinates, vertices[0].coordinates),
                                                StarMath.subtract(vertices[2].coordinates, vertices[1].coordinates));
            else
            {
                var b = new double[dimension];
                for (var i = 0; i < dimension; i++) b[i] = 1.0;
                var A = new double[dimension, dimension];
                for (var i = 0; i < dimension; i++)
                    StarMath.SetRow(i, A, vertices[i].coordinates);
                normal = StarMath.solve(A, b);
            }
            return StarMath.normalize(normal);
        }


        private static SortedList<double, IVertexConvHull> findBeyondVertices(IFaceConvHull face,
                                                                              IEnumerable<IVertexConvHull> vertices)
        {
            var beyondVertices = new SortedList<double, IVertexConvHull>(new noEqualSortMaxtoMinDouble());
            foreach (var v in vertices)
            {
                double dotP;
                if (isVertexOverFace(v, face, out dotP)) beyondVertices.Add(dotP, v);
            }
            return beyondVertices;
        }


        private void updateCenter(IVertexConvHull currentVertex)
        {
            center = StarMath.divide(StarMath.add(
                StarMath.multiply(convexHull.Count - 1, center),
                currentVertex.coordinates),
                                     convexHull.Count);
        }

        #endregion
    }
}