#region
using System;
using System.Collections;
using System.Collections.Generic;
using StarMathLib;
using System.Linq;
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
            convexHullAnalysisComplete = false;
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
            if (vertices[0] as IVertexConvHull != null)
                foreach (var v in vertices)
                    origVertices.Add((IVertexConvHull)v);
            else if (vertices[0] as double[] != null)
            {
                foreach (var v in vertices)
                    origVertices.Add(new defaultVertex { coordinates = (double[])v });
            }
            else throw new Exception("List must be made up of IVertexConvHull objects or 1D double arrays.");
            convexHullAnalysisComplete = false;
            return origVertices.Count;
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
            convexHullAnalysisComplete = false;
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

            convexHullAnalysisComplete = false;
            return origVertices.Count;
        }
        #endregion

        #region Main Find Convex Hull invocations (others below feed into these)
        /// <summary>
        /// Finds the convex hull.
        /// </summary>
        /// <param name="dimensions">The dimensions.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(int dimensions = -1)
        {
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            Initialize();
            if (dimension < 2) throw new Exception("Dimensions of space must be 2 or greater.");
            if (dimension == 2) Find2D();
            else FindConvexHull();
            convexHullAnalysisComplete = true;
            return convexHull;
        }



        /// <summary>
        /// Finds the convex hull.
        /// </summary>
        /// <param name="faces">The faces.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <param name="dimensions">The dimensions.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(out List<IFaceConvHull> faces, Type face_Type, int dimensions = -1)
        {
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            Initialize();
            if (dimension < 2) throw new Exception("Dimensions of space must be 2 or greater.");
            if (dimension == 2) Find2D();
            else FindConvexHull();

            faces = new List<IFaceConvHull>(convexFaces.Count);
            foreach (var f in convexFaces)
            {
                var constructor = face_Type.GetConstructor(new Type[0]);
                var newFace = (IFaceConvHull)constructor.Invoke(new object[0]);
                newFace.normal = f.Value.normal;
                newFace.vertices = f.Value.vertices;
                faces.Add(newFace);
            }
            convexHullAnalysisComplete = true;
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
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            if (!convexHullAnalysisComplete)
            {
                foreach (var v in origVertices)
                {
                    var size = 0.0;
                    for (var i = 0; i < dimension; i++)
                        size += (v.coordinates[i] * v.coordinates[i]);
                    var coord = v.coordinates;
                    Array.Resize(ref coord, dimension + 1);
                    v.coordinates[dimension] = size;
                }
                FindConvexHull();
            }
            delaunayFaces = new List<FaceData>(convexFaces.Values);
            for (var i = delaunayFaces.Count - 1; i >= 0; i--)
                if (delaunayFaces[i].normal[dimension] <= 0)
                    delaunayFaces.RemoveAt(i);
            var userDelaunayFaces = new List<IFaceConvHull>();
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
            if (!convexHullAnalysisComplete)
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
                        avg = StarMath.divide(avg, dimension + 1);
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