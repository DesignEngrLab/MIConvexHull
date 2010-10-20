#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using StarMathLib;
#endregion

namespace MIConvexHullPluginNameSpace
{
    /// <summary>
    ///   MIConvexHull for 3D.
    /// </summary>
    public static partial class ConvexHull
    {
        #region Input Vertices
        /// <summary>
        /// Inputs the vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public static int InputVertices(List<IVertexConvHull> vertices)
        {
            origVertices = new List<IVertexConvHull>(vertices);
            convexHullAnalysisComplete = delaunayAnalysisComplete = false;
            return origVertices.Count;
        }


        /// <summary>
        /// Inputs the vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public static int InputVertices(IList vertices)
        {
            origVertices = new List<IVertexConvHull>(vertices.Count);
            if (typeof(Point).IsInstanceOfType(vertices[0]))
                Input2DPoints((IList<Point>)vertices);
            else if (typeof(Point3D).IsInstanceOfType(vertices[0]))
                Input3DPoints((IList<Point3D>)vertices);
            else if (typeof(IVertexConvHull).IsInstanceOfType(vertices[0]))
                foreach (var v in vertices)
                    origVertices.Add((IVertexConvHull)v);
            else if (typeof(double[]).IsInstanceOfType(vertices[0]))
            {
                foreach (var v in vertices)
                    origVertices.Add(new defaultVertex { coordinates = (double[])v });
            }
            else throw new Exception("List must be made up of Point (System.Windows), Point3D(Windows.Media3D),or IVertexConvHull objects, or 1D double arrays.");
            convexHullAnalysisComplete = delaunayAnalysisComplete = false;
            return origVertices.Count;
        }
        static void Input2DPoints(IEnumerable<Point> vertices)
        {
            foreach (Point t in vertices)
                origVertices.Add(new defaultVertex { coordinates = new[] { t.X, t.Y } });
        }

        static void Input3DPoints(IEnumerable<Point3D> vertices)
        {
            foreach (Point3D t in vertices)
                origVertices.Add(new defaultVertex { coordinates = new[] { t.X, t.Y, t.Z } });
        }


        /// <summary>
        /// Inputs the vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public static int InputVertices(object[] vertices)
        {
            origVertices = new List<IVertexConvHull>();
            if (vertices[0] as IVertexConvHull != null)
            {
                for (var i = 0; i < vertices.GetLength(0); i++)
                    origVertices.Add((IVertexConvHull)vertices[i]);
            }
            else if ((vertices[0] as double[] != null) || (vertices[0] as float[] != null))
            {
                for (var i = 0; i < vertices.GetLength(0); i++)
                    origVertices.Add(new defaultVertex { coordinates = (double[])vertices[i] });
            }
            convexHullAnalysisComplete = delaunayAnalysisComplete = false;
            return origVertices.Count;
        }


        /// <summary>
        /// Inputs the vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public static int InputVertices(double[,] vertices)
        {
            origVertices = new List<IVertexConvHull>();
            for (var i = 0; i < vertices.GetLength(0); i++)
                origVertices.Add(new defaultVertex { coordinates = StarMath.GetRow(i, vertices) });

            convexHullAnalysisComplete = delaunayAnalysisComplete = false;
            return origVertices.Count;
        }
        #endregion

        #region Main Find Convex Hull invocations (others below feed into these)
        /// <summary>
        /// Finds the convex hull.
        /// </summary>
        /// <param name="dimensions">The dimensions of the system. It will be automatically determined if not provided.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(int dimensions = -1)
        {
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            Initialize();
            if (dimension < 2) throw new Exception("Dimensions of space must be 2 or greater.");
            if (!convexHullAnalysisComplete)
            {
                if (dimension == 2) Find2D();
                else FindConvexHull();
                convexHullAnalysisComplete = true;
            }
            return convexHull;
        }
        /// <summary>
        /// Returns the convex hull as array of double arrays.
        /// </summary>
        /// <param name="dimensions">The dimensions of the system. It will be automatically determined if not provided.</param>
        /// <returns></returns>
        public static double[][] FindConvexHull_AsDoubleArray(int dimensions = -1)
        {
            var vertices = FindConvexHull(dimensions);
            var result = new double[vertices.Count][];
            for (int i = 0; i < vertices.Count; i++)
                result[i] = vertices[i].coordinates;
            return result;
        }



        /// <summary>
        /// Finds the convex hull.
        /// </summary>
        /// <param name="faces">The faces.</param>
        /// <param name="face_Type">Type of the face.</param>
        /// <param name="dimensions">The dimensions of the system. It will be automatically determined if not provided.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(out List<IFaceConvHull> faces, Type face_Type = null, int dimensions = -1)
        {
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            Initialize();
            if (dimension < 2) throw new Exception("Dimensions of space must be 2 or greater.");
            if (!convexHullAnalysisComplete)
            {
                if (dimension == 2) Find2D();
                else FindConvexHull();
                convexHullAnalysisComplete = true;
            }

            if (face_Type != null)
            {
                faces = new List<IFaceConvHull>(convexFaces.Count);
                foreach (var f in convexFaces)
                {
                    var constructor = face_Type.GetConstructor(new Type[0]);
                    var newFace = (IFaceConvHull)constructor.Invoke(new object[0]);
                    newFace.normal = f.Value.normal;
                    newFace.vertices = f.Value.vertices;
                    faces.Add(newFace);
                }
            }
            else faces = new List<IFaceConvHull>(convexFaces.Values);
            return convexHull;
        }


        #endregion


        #region Find Delaunay Triangulation
        /// <summary>
        /// Finds the delaunay triangulation.
        /// </summary>
        /// <param name="face_Type">Type of the face_.</param>
        /// <param name="dimensions">The dimensions.</param>
        /// <returns></returns>
        public static List<IFaceConvHull> FindDelaunayTriangulation(Type face_Type = null, int dimensions = -1)
        {
            if (!delaunayAnalysisComplete)
            {
                if ((origVertices == null) || (origVertices.Count == 0))
                    throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
                if (dimensions == -1) determineDimension(origVertices);
                else dimension = dimensions;
                foreach (var v in origVertices)
                {
                    var size = StarMath.norm2(v.coordinates, true);
                    var coord = v.coordinates;
                    Array.Resize(ref coord, dimension + 1);
                    coord[dimension] = size;
                    v.coordinates = coord;
                }
                dimension++;
                Initialize();
                FindConvexHull();
                delaunayAnalysisComplete = true;
                dimension--;
            }
            foreach (var v in convexHull)
            {
                var coord = v.coordinates;
                Array.Resize(ref coord, dimension);
                v.coordinates = coord;
            }
            delaunayFaces = new List<FaceData>(convexFaces.Values);
            for (var i = delaunayFaces.Count - 1; i >= 0; i--)
                if (delaunayFaces[i].normal[dimension] >= 0)
                    delaunayFaces.RemoveAt(i);

            List<IFaceConvHull> userDelaunayFaces;
            if (face_Type != null)
            {
                userDelaunayFaces = new List<IFaceConvHull>(delaunayFaces.Count);
                foreach (var f in delaunayFaces)
                {
                    var constructor = face_Type.GetConstructor(new Type[0]);
                    var newFace = (IFaceConvHull)constructor.Invoke(new object[0]);
                    newFace.vertices = f.vertices;
                    userDelaunayFaces.Add(newFace);
                }
            }
            else userDelaunayFaces = new List<IFaceConvHull>(delaunayFaces);
            return userDelaunayFaces;
        }


        /// <summary>
        /// Finds the voronoi graph.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="edges">The edges.</param>
        /// <param name="node_Type">Type of the node_.</param>
        /// <param name="dimensions">The dimensions.</param>
        public static void FindVoronoiGraph(out List<IVertexConvHull> nodes, out List<Tuple<IVertexConvHull, IVertexConvHull>> edges,
            Type node_Type = null, int dimensions = -1)
        {
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            FindDelaunayTriangulation(null, dimension);
            voronoiNodes = new List<IVertexConvHull>(delaunayFaces.Count);
            foreach (var f in delaunayFaces)
            {
                var avg = new double[dimension];
                avg = f.vertices.Aggregate(avg, (current, v) => StarMath.add(current, v.coordinates));
                avg = StarMath.divide(avg, dimension + 1);
                voronoiNodes.Add(makeNewVoronoiEdge(avg, node_Type));
            }
            voronoiEdges = new List<Tuple<IVertexConvHull, IVertexConvHull>>(delaunayFaces.Count);
            for (var i = 0; i < delaunayFaces.Count - 1; i++)
                for (var j = i + 1; j < delaunayFaces.Count; j++)
                    if (delaunayFaces[i].adjacentFaces.Contains(delaunayFaces[j]))
                        voronoiEdges.Add(Tuple.Create(voronoiNodes[i], voronoiNodes[j]));

            for (var i = 0; i < delaunayFaces.Count; i++)
                for (var j = 0; j < delaunayFaces[i].adjacentFaces.GetLength(0); j++)
                    if (!delaunayFaces.Contains(delaunayFaces[i].adjacentFaces[j]))
                    {
                        var edgeNodes = new List<IVertexConvHull>(delaunayFaces[i].vertices);
                        edgeNodes.RemoveAt(j);
                        var avg = new double[dimension];
                        avg = edgeNodes.Aggregate(avg, (current, v) => StarMath.add(current, v.coordinates));
                        avg = StarMath.divide(avg, dimension);
                        voronoiNodes.Add(makeNewVoronoiEdge(avg, node_Type));
                        voronoiEdges.Add(Tuple.Create(voronoiNodes[i], voronoiNodes[voronoiNodes.Count - 1]));
                    }
            nodes = voronoiNodes;
            edges = voronoiEdges;
        }

        private static IVertexConvHull makeNewVoronoiEdge(double[] avg, Type node_Type)
        {
            if (node_Type == null)
                return new defaultVertex { coordinates = avg };
            var constructor = node_Type.GetConstructor(new Type[0]);
            var newNode = (IVertexConvHull)constructor.Invoke(new object[0]);
            newNode.coordinates = avg;
            return newNode;
        }

        #endregion

    }
}