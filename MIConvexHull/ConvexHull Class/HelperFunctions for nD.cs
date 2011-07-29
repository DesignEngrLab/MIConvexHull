#region

using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// must be of length dim + 1
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        private bool IsZeroVolume(IList<IVertexConvHull> vertices)
        {
            double[,] m = new double[dimension, dimension];

            var v0 = vertices[0].coordinates;
            for (int i = 1; i <= dimension; i++)
            {
                StarMath.SetColumn(i - 1, m, StarMath.subtract(vertices[i].coordinates, v0, dimension));
            }
            double det;
            if (dimension > 3) det = StarMath.determinantBig(m, dimension);
            else det = StarMath.determinant(m);

            return Math.Abs(det) < 1e-8;
        }

        private FibonacciHeap<double, FaceData> initiateFaceDatabase()
        {
            bool coplanar = IsZeroVolume(convexHull);

            for (var i = 0; i < dimension + 1; i++)
            {
                var vertices = new List<IVertexConvHull>(convexHull);
                vertices.RemoveAt(i);
                var newFace = coplanar ? MakeInitFaceCoplanar(vertices, i) : MakeFace(vertices);
                //var newFace = MakeFace(vertices);
                /* the next line initialization of "verticesBeyond" is just to allow the line of
                 * code in updateFaces ("edge.Item2.verticesBeyond.Values).ToList());")
                 * to not crash when filling out the initial polygon. */
                newFace.verticesBeyond = new HashSet<IVertexConvHull>();
                newFace.fibCell = convexFaces.Enqueue(0.0, newFace);
            }
            for (var i = 0; i < dimension; i++)
                for (var j = i + 1; j < dimension + 1; j++)
                {
                    var edge = new List<IVertexConvHull>(convexHull);
                    edge.RemoveAt(j);
                    edge.RemoveAt(i);
                    var betweenFaces = (from f in convexFaces.Select(tt => tt.Value)
                                        where f.vertices.Intersect(edge).Count() == edge.Count()
                                        select f).Take(2).ToArray();
                    recordAdjacentFaces(betweenFaces[0], betweenFaces[1], edge);
                }
            return convexFaces;
        }

        static bool containsVert(IList<IVertexConvHull> vs, IVertexConvHull vert, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (vs[i] == vert) return true;
            }

            return false;
        }

        private void recordAdjacentFaces(FaceData face1, FaceData face2, IList<IVertexConvHull> edge)
        {
            int ec = edge.Count;

            for (int i = 0; i < dimension; i++)
            {
                var v = face1.vertices[i];
                if (!containsVert(edge, v, ec))
                {
                    face1.adjacentFaces[i] = face2;
                    break;
                }
            }

            for (int i = 0; i < dimension; i++)
            {
                var v = face2.vertices[i];
                if (!containsVert(edge, v, ec))
                {
                    face2.adjacentFaces[i] = face1;
                    break;
                }
            }
        }

        private FaceData MakeFace(IVertexConvHull currentVertex, IEnumerable<IVertexConvHull> edge)
        {
            var vertices = new List<IVertexConvHull>(edge);
            vertices.Insert(0, currentVertex);
            return MakeFace(vertices);
        }

        private FaceData MakeInitFaceCoplanar(List<IVertexConvHull> vertices, int i)
        {
            var normal = findNormalVector(vertices);
            if (i % (dimension - 1) == 1)
            {
                normal = StarMath.subtract(StarMath.makeZeroVector(dimension), normal,dimension);
                if (dimension == 3) vertices.Reverse();
            }
            var newFace = new FaceData(dimension)
            {
                normal = normal,
                vertices = vertices.ToArray()
            };
            return newFace;
        }

        //private FaceData MakeInitFace(List<IVertexConvHull> vertices, IVertexConvHull last)
        //{ 
        //    var normal = findNormalVector(vertices);

        //    var dot = MathUtils.subtractAndDot(normal, last.coordinates, vertices[0].coordinates, dimension);
        //    if (dot > 0)
        //    {
        //        normal = StarMath.subtract(StarMath.makeZeroVector(dimension), normal);
        //        if (dimension == 3) vertices.Reverse();
        //    }
        //    var newFace = new FaceData(dimension)
        //    {
        //        normal = normal,
        //        vertices = vertices.ToArray()
        //    };
        //    return newFace;
        //}

        private FaceData MakeFace(List<IVertexConvHull> vertices)
        {
            var outDir = new double[dimension];

            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    outDir[j] += vertices[i].coordinates[j];
                }
            }

            for (int j = 0; j < dimension; j++)
            {
                outDir[j] /= dimension;
            }

            for (int j = 0; j < dimension; j++)
            {
                outDir[j] -= center[j];
            }

            var normal = findNormalVector(vertices);
            var dot = StarMath.dotProduct(normal, outDir, dimension);

            if (dot < 0)
            {
                normal = StarMath.subtract(StarMath.makeZeroVector(dimension), normal,dimension);
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

        private FaceData[] findFacesBeneathInitialVertices(IVertexConvHull currentVertex)
        {
            var facesUnder = new List<FaceData>();

            foreach (var fc in convexFaces)
            {
                var face = fc.Value;
                double dummy;
                if (isVertexOverFace(currentVertex, face, out dummy, dimension))
                    facesUnder.Add(face);
            }
            return facesUnder.ToArray();
        }


        private static Boolean isVertexOverFace(IVertexConvHull v, IFaceConvHull f, out double dotP, int n)
        {
            //dotP = StarMath.multiplyDot(f.normal, StarMath.subtract(v.coordinates, f.vertices[0].coordinates));
            //this function is called very often and calling dot and sub separately is a lot of overhead
            dotP = StarMath.subtractAndDot(f.normal, v.coordinates, f.vertices[0].coordinates, n);
            return (dotP >= 0.0);
        }

        private static FaceData[] findAffectedFaces(FaceData currentFaceData, IVertexConvHull currentVertex,
                                                        HashSet<FaceData> primaryFaces = null)
        {
            if (primaryFaces == null)
                return findAffectedFaces(currentFaceData, currentVertex, new HashSet<FaceData> { currentFaceData });

            for (int i = 0; i < currentFaceData.adjacentFaces.Length; i++)
            {
                var adjFace = currentFaceData.adjacentFaces[i];

                if (adjFace != null && !primaryFaces.Contains(adjFace) /*!contains(primaryFaces, adjFace)*/ && adjFace.verticesBeyond.Contains(currentVertex))
                {
                    primaryFaces.Add(adjFace);
                    findAffectedFaces(adjFace, currentVertex, primaryFaces);
                }
            }
            return primaryFaces.ToArray();
        }

        // this is a somewhat crude optimization of 
        // var edge = newFaces[i].vertices.Intersect(newFaces[j].vertices).ToList();
        // in updateFaces
        int EdgeList(IVertexConvHull[] l, IVertexConvHull[] r, IVertexConvHull[] edgeBuffer)
        {
            int c = 0;
            int miss = 0;

            for (int i = 0; i < dimension; i++)
            {
                bool contains = false;
                for (int j = 0; j < dimension; j++)
                {
                    if (l[i] == r[j])
                    {
                        contains = true;
                        break;
                    }
                }
                if (contains)
                {
                    if (c < dimension - 1) edgeBuffer[c] = l[i];
                    c++;
                }
                else
                {
                    miss++;

                    if (miss > 1) return 0;
                }
            }

            return c;
        }

        // IEnumerable.Contains is called very often and this seems to be faster
        bool containsNull(FaceData[] fd)
        {
            for (int i = 0; i < fd.Length; i++)
            {
                if (fd[i] == null) return true;
            }

            return false;
        }

        // IEnumerable.Contains is called very often and this seems to be faster
        static bool contains(FaceData[] fd, FaceData face)
        {
            for (int i = 0; i < fd.Length; i++)
            {
                if (fd[i] == face) return true;
            }

            return false;
        }

        // IEnumerable.Contains is called very often and this seems to be faster
        static bool contains(List<FaceData> fd, FaceData face)
        {
            for (int i = 0; i < fd.Count; i++)
            {
                if (fd[i] == face) return true;
            }

            return false;
        }

        private void updateFaces(FaceData[] oldFaces, IVertexConvHull currentVertex, bool noRemove = false)
        {
            var newFaces = new List<FaceData>();
            var affectedVertices = new HashSet<IVertexConvHull>();
            foreach (var of in oldFaces) affectedVertices.UnionWith(of.verticesBeyond);
            affectedVertices.Remove(currentVertex);

            foreach (var oldFace in oldFaces)
            {
                if (oldFace.fibCell != null)
                {
                    convexFaces.Delete(oldFace.fibCell);
                    oldFace.fibCell = null;
                }

                for (var i = 0; i < oldFace.adjacentFaces.Length; i++)
                {
                    if (oldFace.adjacentFaces[i] != null && !contains(oldFaces, oldFace.adjacentFaces[i]))
                    {
                        var edge = new List<IVertexConvHull>(oldFace.vertices);
                        edge.RemoveAt(i);
                        var newFace = MakeFace(currentVertex, edge);
                        recordAdjacentFaces(newFace, oldFace.adjacentFaces[i], edge);

                        HashSet<IVertexConvHull> t;

                        if (affectedVertices.Count > oldFace.adjacentFaces[i].verticesBeyond.Count)
                        {
                            t = new HashSet<IVertexConvHull>(affectedVertices);
                            t.UnionWith(oldFace.adjacentFaces[i].verticesBeyond);
                        }
                        else
                        {
                            t = new HashSet<IVertexConvHull>(oldFace.adjacentFaces[i].verticesBeyond);
                            t.UnionWith(affectedVertices);
                        }

                        findBeyondVertices(newFace, t, dimension);

                        newFaces.Add(newFace);
                    }
                }
            }

            if (newFaces.Count == 0) return;

            IVertexConvHull[] edgeBuffer = new IVertexConvHull[newFaces[0].vertices.Length - 1];

            for (var i = 0; i < newFaces.Count - 1; i++)
            {
                for (var j = i + 1; j < newFaces.Count; j++)
                {
                    int count = EdgeList(newFaces[i].vertices, newFaces[j].vertices, edgeBuffer);
                    if (count == dimension - 1)
                    {
                        recordAdjacentFaces(newFaces[i], newFaces[j], edgeBuffer);
                    }
                    if (!containsNull(newFaces[i].adjacentFaces)) break;
                }
            }
            foreach (var newFace in newFaces)
                if (newFace.verticesBeyond.Count == 0)
                {
                    if (Status.TaskNumber == 4) Status.SubTaskNumber++;
                    newFace.fibCell = convexFaces.Enqueue(-1.0, newFace);
                }
                else
                {
                    var key = newFace.minVertexBeyond.Item1;
                    newFace.fibCell = convexFaces.Enqueue(key, newFace);
                }
        }


        double[] findNormalVector4D(IList<IVertexConvHull> vertices)
        {
            var x = StarMath.subtract(vertices[1].coordinates, vertices[0].coordinates, dimension);
            var y = StarMath.subtract(vertices[2].coordinates, vertices[0].coordinates, dimension);
            var z = StarMath.subtract(vertices[3].coordinates, vertices[0].coordinates, dimension);

            var nx = x[3] * y[2] * z[1] - x[2] * y[3] * z[1] - x[3] * y[1] * z[2] + x[1] * y[3] * z[2] + x[2] * y[1] * z[3] - x[1] * y[2] * z[3];
            var ny = -(x[3] * y[2] * z[0]) + x[2] * y[3] * z[0] + x[3] * y[0] * z[2] - x[0] * y[3] * z[2] - x[2] * y[0] * z[3] + x[0] * y[2] * z[3];
            var nz = x[3] * y[1] * z[0] - x[1] * y[3] * z[0] - x[3] * y[0] * z[1] + x[0] * y[3] * z[1] + x[1] * y[0] * z[3] - x[0] * y[1] * z[3];
            var nw = -(x[2] * y[1] * z[0]) + x[1] * y[2] * z[0] + x[2] * y[0] * z[1] - x[0] * y[2] * z[1] - x[1] * y[0] * z[2] + x[0] * y[1] * z[2];

            double norm = Math.Sqrt(nx * nx + ny * ny + nz * nz + nw * nw);

            return new double[] { nx / norm, ny / norm, nz / norm, nw / norm };
        }

        private double[] findNormalVector(IList<IVertexConvHull> vertices)
        {
            double[] normal;
            if (dimension == 3)
            {
                normal = StarMath.crossProduct3(StarMath.subtract(vertices[1].coordinates, vertices[0].coordinates, dimension),
                                                StarMath.subtract(vertices[2].coordinates, vertices[1].coordinates, dimension));
                return StarMath.normalize(normal, dimension);
            }
            else if (dimension == 4)
            {
                normal = findNormalVector4D(vertices);
                return normal;
            }
            else
            {
                var b = new double[dimension];
                for (var i = 0; i < dimension; i++) b[i] = 1.0;
                var A = new double[dimension, dimension];
                for (var i = 0; i < dimension; i++)
                    StarMath.SetRow(i, A, vertices[i].coordinates);

                var s = StarMath.MakePrintString(A);
                Console.WriteLine(s);

                normal = StarMath.solve(A, b);
                return StarMath.normalize(normal, dimension);
            }
        }


        private static void findBeyondVertices(FaceData face, IEnumerable<IVertexConvHull> vertices, int dim)
        {
            HashSet<IVertexConvHull> beyondVertices = new HashSet<IVertexConvHull>();

            double min = double.NegativeInfinity;
            IVertexConvHull minV = null;

            foreach (var v in vertices)
            {
                double dotP;
                if (isVertexOverFace(v, face, out dotP, dim))
                {
                    if (dotP > min)
                    {
                        min = dotP;
                        minV = v;
                    }
                    beyondVertices.Add(v);
                }
            }
            face.verticesBeyond = beyondVertices;
            face.minVertexBeyond = Tuple.Create(min, minV);
        }


        private void updateCenter(IVertexConvHull currentVertex)
        {
            center = StarMath.divide(StarMath.add(
                StarMath.multiply(convexHull.Count - 1, center,dimension),
                currentVertex.coordinates,dimension), convexHull.Count,dimension);
        }

        #endregion
    }
}