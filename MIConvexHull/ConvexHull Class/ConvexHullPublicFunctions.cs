#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using StarMathLib;
#endregion

namespace MIConvexHull
{
    /// <summary>
    ///   MIConvexHull for 3D.
    /// </summary>
    public partial class ConvexHull
    {
        #region Constructors: Used to Input Vertices
        /// <summary>
        /// Inputs the vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public ConvexHull(List<IVertexConvHull> vertices)
        {
            origVertices = new List<IVertexConvHull>(vertices);
        }


        /// <summary>
        /// Inputs the vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public ConvexHull(IList vertices)
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
        }
        void Input2DPoints(IEnumerable<Point> vertices)
        {
            foreach (var t in vertices)
                origVertices.Add(new defaultVertex { coordinates = new[] { t.X, t.Y } });
        }

        void Input3DPoints(IEnumerable<Point3D> vertices)
        {
            foreach (var t in vertices)
                origVertices.Add(new defaultVertex { coordinates = new[] { t.X, t.Y, t.Z } });
        }


        /// <summary>
        /// Inputs the vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public ConvexHull(double[,] vertices)
        {
            origVertices = new List<IVertexConvHull>();
            for (var i = 0; i < vertices.GetLength(0); i++)
                origVertices.Add(new defaultVertex { coordinates = StarMath.GetRow(i, vertices) });

        }
        #endregion

        #region Main Find Convex Hull invocations (others below feed into these)
        /// <summary>
        /// Finds the convex hull.
        /// </summary>
        /// <param name="dimensions">The dimensions of the system. It will be automatically determined if not provided.</param>
        /// <returns></returns>
        public List<IVertexConvHull> FindConvexHull(int dimensions = -1)
        {
            Status.TotalTaskCount = 4;
            Status.TaskNumber = 0;
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            center = new double[dimension];
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
        public double[][] FindConvexHull_AsDoubleArray(int dimensions = -1)
        {
            Status.TotalTaskCount = 4;
            Status.TaskNumber = 0;
            var vertices = FindConvexHull(dimensions);
            var result = new double[vertices.Count][];
            for (var i = 0; i < vertices.Count; i++)
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
        public List<IVertexConvHull> FindConvexHull(out List<IFaceConvHull> faces, Type face_Type = null, int dimensions = -1)
        {
            Status.TotalTaskCount = 5;
            Status.TaskNumber = 0;
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            center = new double[dimension];
            if (dimension < 2) throw new Exception("Dimensions of space must be 2 or greater.");
            if (!convexHullAnalysisComplete)
            {
                if (dimension == 2) Find2D();
                else FindConvexHull();
                convexHullAnalysisComplete = true;
            }
            Status.TaskNumber = 5;
            Status.TotalSubTaskCount = convexFaces.Count;
            Status.SubTaskNumber = 0;

            if (face_Type != null)
            {
                faces = new List<IFaceConvHull>(convexFaces.Count);
                foreach (var f in convexFaces)
                {
                    Status.SubTaskNumber++;
                    var constructor = face_Type.GetConstructor(new Type[0]);
                    var newFace = (IFaceConvHull)constructor.Invoke(new object[0]);
                    newFace.normal = f.Value.normal;
                    newFace.vertices = f.Value.vertices;
                    faces.Add(newFace);
                }
            }
            else faces = new List<IFaceConvHull>(convexFaces.Select(f => f.Value));
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
        public List<IFaceConvHull> FindDelaunayTriangulation(Type face_Type = null, int dimensions = -1)
        {
            Status.TotalTaskCount = 5;
            Status.TaskNumber = 0;
            if (!delaunayAnalysisComplete)
            {
                var rnd = new Random();

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
                center = new double[dimension];
                FindConvexHull();
                delaunayAnalysisComplete = true;
                dimension--;
            }
            Status.TaskNumber = 5;
            Status.TotalSubTaskCount = convexHull.Count;
            Status.SubTaskNumber = 0;
            foreach (var v in convexHull)
            {
                Status.SubTaskNumber++;
                var coord = v.coordinates;
                Array.Resize(ref coord, dimension);
                v.coordinates = coord;
            }
            delaunayFaces = new List<FaceData>(convexFaces.Select(f => f.Value));
            for (var i = delaunayFaces.Count - 1; i >= 0; i--)
                if (delaunayFaces[i].normal[dimension] >= 0)
                    delaunayFaces.RemoveAt(i);

            List<IFaceConvHull> userDelaunayFaces;
            if (face_Type != null)
            {
                Status.TotalSubTaskCount = delaunayFaces.Count;
                Status.SubTaskNumber = 0;
                userDelaunayFaces = new List<IFaceConvHull>(delaunayFaces.Count);
                foreach (var f in delaunayFaces)
                {
                    Status.SubTaskNumber++;
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
        public void FindVoronoiGraph(out List<IVertexConvHull> nodes, out List<Tuple<IVertexConvHull, IVertexConvHull>> edges,
            Type node_Type = null, int dimensions = -1)
        {
            Status.TotalTaskCount = 6;
            Status.TaskNumber = 0;
            if ((origVertices == null) || (origVertices.Count == 0))
                throw new Exception("Please input the vertices first with the \"InputVertices\" function.");
            if (dimensions == -1) determineDimension(origVertices);
            else dimension = dimensions;
            FindDelaunayTriangulation(null, dimension);
            Status.TaskNumber = 6;
            var voronoiNodes = new List<voronoiVertex>(delaunayFaces.Count);
            var p = delaunayFaces.Count;
            Status.TotalSubTaskCount = p + p * (p - 1) / 2 + p * dimension;
            Status.SubTaskNumber = 0;
            voronoiNodes = new List<IVertexConvHull>(delaunayFaces.Count);
            foreach (var f in delaunayFaces)
            {
                Status.SubTaskNumber++;
                var avg = new double[dimension];
                avg = f.vertices.Aggregate(avg, (current, v) => StarMath.add(current, v.coordinates));
                avg = StarMath.divide(avg, dimension + 1);
                voronoiNodes.Add(new voronoiVertex { vertex = makeNewVoronoiEdge(avg, node_Type), face = f });
            }

            HashSet<voronoiEdge> voronoiEdges = new HashSet<voronoiEdge>();
            var nodeDict = voronoiNodes.ToDictionary(n => n.face, n => n);

            foreach (var f in delaunayFaces)
            {
                foreach (var af in f.adjacentFaces)
                {
                    if (nodeDict.ContainsKey(af))
                    {
                        voronoiEdges.Add(new voronoiEdge { a = nodeDict[f], b = nodeDict[af] });
                    }
                };
            };
            
            nodes = voronoiNodes.Select(n => n.vertex).ToList();
            edges = voronoiEdges.Select(e => Tuple.Create(e.a.vertex, e.b.vertex)).ToList();
            
            for (var i = 0; i < delaunayFaces.Count; i++)
            {
                for (var j = 0; j < delaunayFaces[i].adjacentFaces.GetLength(0); j++)
                {
                    if (!nodeDict.ContainsKey(delaunayFaces[i].adjacentFaces[j]))
                    {
                        var edgeNodes = new List<IVertexConvHull>(delaunayFaces[i].vertices);
                        edgeNodes.RemoveAt(j);
                        var avg = new double[dimension];
                        avg = edgeNodes.Aggregate(avg, (current, v) => StarMath.add(current, v.coordinates));
                        avg = StarMath.divide(avg, dimension);
                        nodes.Add(makeNewVoronoiEdge(avg, node_Type));
                        edges.Add(Tuple.Create(nodes[i], nodes[nodes.Count - 1]));
                        Status.SubTaskNumber++;
                    }
                }
            }            
        }

        private IVertexConvHull makeNewVoronoiEdge(double[] avg, Type node_Type)
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