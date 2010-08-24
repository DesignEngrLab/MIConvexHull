/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2006, 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with MIConvexHull.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://miconvexhull.codeplex.com
 *************************************************************************/
namespace MIConvexHullPluginNameSpace
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Linq;
    using StarMathLib;

    /// <summary>
    /// MIConvexHull for 3D.
    /// </summary>
    public static partial class ConvexHull
    {
        /// <summary>
        /// Find the convex hull for the 3D vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(IList vertices, int dimension = -1)
        {
            if (vertices as List<IVertexConvHull> != null)
                origVertices = new List<IVertexConvHull>(vertices.Count);
            else if (vertices as List<double[]> != null)
            {
                origVertices = new List<IVertexConvHull>(vertices.Count);
                for (int i = 0; i < vertices.Count; i++)
                    origVertices.Add(new defaultVertex() { location = (double[])vertices[i] });
            }
            else throw new Exception("List must be made up of IVertexConvHull objects or 1D double arrays.");
            if (dimension == -1) determineDimension(origVertices);
            Initialize(dimension);
            if (dimension == 2) Find2D();
            else FindConvexHull();
            return convexHull;
        }

        /// <summary>
        /// Find the convex hull for the 3D vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <param name="faces">The faces.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(IList vertices, out List<IFaceConvHull> faces,
            Type face_Type = null, int dimension = -1)
        {
            if (vertices as List<IVertexConvHull> != null)
                origVertices = new List<IVertexConvHull>((List<IVertexConvHull>)vertices);
            else if (vertices as List<double[]> != null)
            {
                origVertices = new List<IVertexConvHull>(vertices.Count);
                for (int i = 0; i < vertices.Count; i++)
                    origVertices.Add(new defaultVertex() { location = (double[])vertices[i] });
            }
            else throw new Exception("List must be made up of IVertexConvHull objects or 1D double arrays.");
            if (dimension == -1) determineDimension(origVertices);
            Initialize(dimension);
            faceType = face_Type;
            if (dimension == 2) Find2D();
            else FindConvexHull();

            faces = new List<IFaceConvHull>(convexFaces.Count);
            if (faceType != null)
            {
                foreach (var f in convexFaces)
                {
                    var constructor = faceType.GetConstructor(new Type[0]);
                    var newFace = (IFaceConvHull)constructor.Invoke(new object[0]);
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
        /// Find the convex hull for the 3D vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(object[] vertices, int dimension = -1)
        {
            var ListVerts = new List<IVertexConvHull>();
            if (vertices[0] as IFaceConvHull != null)
            {
                for (int i = 0; i < vertices.GetLength(0); i++)
                    ListVerts.Add((IVertexConvHull)vertices[i]);
            }
            else if ((vertices[0] as double[] != null) || (vertices[0] as float[] != null))
            {
                for (int i = 0; i < vertices.GetLength(0); i++)
                    ListVerts.Add(new defaultVertex() { location = (double[])vertices[i] });
            }
            return FindConvexHull(ListVerts, dimension);
        }
        /// <summary>
        /// Find the convex hull for the 3D vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <param name="faces">The faces.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> FindConvexHull(object[] vertices, out List<IFaceConvHull> faces,
            Type face_Type = null, int dimension = -1)
        {
            var ListVerts = new List<IVertexConvHull>();
            if (vertices[0] as IFaceConvHull != null)
            {
                for (int i = 0; i < vertices.GetLength(0); i++)
                    ListVerts.Add((IVertexConvHull)vertices[i]);
            }
            else if ((vertices[0] as double[] != null) || (vertices[0] as float[] != null))
            {
                for (int i = 0; i < vertices.GetLength(0); i++)
                    ListVerts.Add(new defaultVertex() { location = (double[])vertices[i] });
            }
            return FindConvexHull(ListVerts, out faces, face_Type, dimension);
        }

    }
}