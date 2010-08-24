#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace MIConvexHullPluginNameSpace
{
    /// <summary>
    ///   MIConvexHull for 3D.
    /// </summary>
    public static partial class ConvexHull
    {
        /// <summary>
        ///   Finds the convex hull.
        /// </summary>
        /// <param name = "vertices">The vertices.</param>
        /// <param name = "dimensions">The dimension.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(IList vertices, int dimensions = -1)
        {
            if (vertices as List<IVertexConvHull> != null)
                origVertices = new List<IVertexConvHull>(vertices.Count);
            else if (vertices as List<double[]> != null)
            {
                origVertices = new List<IVertexConvHull>(vertices.Count);
                foreach (var v in vertices)
                    origVertices.Add(new defaultVertex {location = (double[]) v});
            }
            else throw new Exception("List must be made up of IVertexConvHull objects or 1D double arrays.");
            if (dimensions == -1) determineDimension(origVertices);
            Initialize(dimensions);
            if (dimensions == 2) Find2D();
            else FindConvexHull();
            return convexHull;
        }


        /// <summary>
        ///   Finds the convex hull.
        /// </summary>
        /// <param name = "vertices">The vertices.</param>
        /// <param name = "faces">The faces.</param>
        /// <param name = "face_Type">Type of the face_.</param>
        /// <param name = "dimensions">The dimension.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(IList vertices, out List<IFaceConvHull> faces,
                                                           Type face_Type = null, int dimensions = -1)
        {
            if (vertices as List<IVertexConvHull> != null)
                origVertices = new List<IVertexConvHull>((List<IVertexConvHull>) vertices);
            else if (vertices as List<double[]> != null)
            {
                origVertices = new List<IVertexConvHull>(vertices.Count);
                foreach (var v in vertices)
                    origVertices.Add(new defaultVertex {location = (double[]) v});
            }
            else throw new Exception("List must be made up of IVertexConvHull objects or 1D double arrays.");
            if (dimensions == -1) determineDimension(origVertices);
            Initialize(dimensions);
            faceType = face_Type;
            if (dimensions == 2) Find2D();
            else FindConvexHull();

            faces = new List<IFaceConvHull>(convexFaces.Count);
            if (faceType != null)
            {
                foreach (var f in convexFaces)
                {
                    var constructor = faceType.GetConstructor(new Type[0]);
                    var newFace = (IFaceConvHull) constructor.Invoke(new object[0]);
                    newFace.normal = f.Value.normal;
                    newFace.vertices = f.Value.vertices;
                    faces.Add(newFace);
                }
            }
            return convexHull;
        }


        /* These three overloads take longer than the ones above. They are provided in cases
         * where the users classes and collections are more like these. Ideally, the
         * user should declare there list of vertices as a List<IVertexConvHull>, but 
         * this is an unrealistic requirement. At any rate, these methods take about 50  
         * nano-second to add each one. */

        /// <summary>
        ///   Finds the convex hull.
        /// </summary>
        /// <param name = "vertices">The vertices.</param>
        /// <param name = "dimensions">The dimension.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(object[] vertices, int dimensions = -1)
        {
            var ListVerts = new List<IVertexConvHull>();
            if (vertices[0] as IFaceConvHull != null)
            {
                for (var i = 0; i < vertices.GetLength(0); i++)
                    ListVerts.Add((IVertexConvHull) vertices[i]);
            }
            else if ((vertices[0] as double[] != null) || (vertices[0] as float[] != null))
            {
                for (var i = 0; i < vertices.GetLength(0); i++)
                    ListVerts.Add(new defaultVertex {location = (double[]) vertices[i]});
            }
            return FindConvexHull(ListVerts, dimensions);
        }

        /// <summary>
        ///   Finds the convex hull.
        /// </summary>
        /// <param name = "vertices">The vertices.</param>
        /// <param name = "faces">The faces.</param>
        /// <param name = "face_Type">Type of the face_.</param>
        /// <param name = "dimensions">The dimension.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(object[] vertices, out List<IFaceConvHull> faces,
                                                           Type face_Type = null, int dimensions = -1)
        {
            var ListVerts = new List<IVertexConvHull>();
            if (vertices[0] as IFaceConvHull != null)
            {
                for (var i = 0; i < vertices.GetLength(0); i++)
                    ListVerts.Add((IVertexConvHull) vertices[i]);
            }
            else if ((vertices[0] as double[] != null) || (vertices[0] as float[] != null))
            {
                for (var i = 0; i < vertices.GetLength(0); i++)
                    ListVerts.Add(new defaultVertex {location = (double[]) vertices[i]});
            }
            return FindConvexHull(ListVerts, out faces, face_Type, dimensions);
        }
    }
}