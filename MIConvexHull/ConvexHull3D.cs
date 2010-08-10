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

    /// <summary>
    /// MIConvexHull for 3D.
    /// </summary>
    public static partial class ConvexHull
    {
        static List<IFaceConvHull> convexFaces;
        /// <summary>
        /// Finds the convex hull vertices.
        /// </summary>
        /// <returns></returns>
        static List<IVertexConvHull> Find3D()
        {
            var VCount = origVertices.Count;

            #region Step 1 : Define Convex Rhombicuboctahedron
            /* The first step is to quickly identify the four to 26 vertices based on the
             * Akl-Toussaint heuristic. In order to do this, I use a 3D matrix to help keep
             * track of the extremse. The 26 extrema can be see as approaching the cloud of 
             * points from the 26 vertices of the Disdyakis dodecahedron 
             * (http://en.wikipedia.org/wiki/Disdyakis_dodecahedron although it may be easier
             * to understand by considering its dual, the Truncated cuboctahedron
             * (http://en.wikipedia.org/wiki/Truncated_cuboctahedron). This also corresponds
             * to base-3 (min,center,max) in three dimensions. Three raised to the third power
             * though is 27. the point at the center (0,0,0) is not used therefore 27 - 1 = 26.
             */
            IVertexConvHull[, ,] extremeVertices = new IVertexConvHull[3, 3, 3];
            int[, ,] extremeVertexIndices = new int[3, 3, 3];
            double[, ,] extremeValues = new double[3, 3, 3];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        extremeValues[i, j, k] = double.NegativeInfinity;


            for (int m = 0; m < VCount; m++)
            {
                var n = origVertices[m];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        for (int k = 0; k < 3; k++)
                            if (!((i == 1) && (j == 1) && (k == 1)))
                            {
                                if (sumproduct(i, j, k, n) > extremeValues[i, j, k])
                                {
                                    extremeVertices[i, j, k] = n;
                                    extremeVertexIndices[i, j, k] = m;
                                    extremeValues[i, j, k] = sumproduct(i, j, k, n);
                                }
                            }
            }
            #endregion

            #region Step #2: Define up to 48 faces of the Disdyakis dodecahedron
            var sortedIndices = new List<int>();
            var convexHull = new List<IVertexConvHull>();
            convexFaces = new List<IFaceConvHull>();
            /* store vertices */
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        if ((!((i == 1) && (j == 1) && (k == 1)))
                        && !sortedIndices.Contains(extremeVertexIndices[i, j, k]))
                        {
                            convexHull.Add(extremeVertices[i, j, k]);
                            sortedIndices.Add(extremeVertexIndices[i, j, k]);
                        }
            sortedIndices.Sort();
            for (int i = sortedIndices.Count - 1; i >= 0; i--) origVertices.RemoveAt(sortedIndices[i]);

            /* Faces: I couldn't find any logic in what made a positive direction to a IFaceConvHull. As a result, the
             * following code is a bit difficult to follow. */
            var adjacentVertID = new int[9, 2] { 
            { -1, -1 }, { 0, -1 }, { +1, -1 }, { +1, 0 }, { +1, +1 }, { 0, +1 }, { -1, +1 }, { -1, 0 }, { -1, -1 }
            };

            for (int i = -3; i < 3; i++)
            {
                var baseVert = new int[3];
                baseVert[cycle(i)] = isNonNegative(i);
                for (int j = 0; j < 8; j++)
                {
                    var v1 = (int[])baseVert.Clone();
                    v1[cycle(i + 1, i)] = adjacentVertID[j, 0];
                    v1[cycle(i + 2, i)] = adjacentVertID[j, 1];
                    var v2 = (int[])baseVert.Clone();
                    v2[cycle(i + 1, i)] = adjacentVertID[j + 1, 0];
                    v2[cycle(i + 2, i)] = adjacentVertID[j + 1, 1];
                    var f = MakeFace(getVertexFromExtreme(extremeVertices, baseVert),
                       getVertexFromExtreme(extremeVertices, v1), getVertexFromExtreme(extremeVertices, v2));
                    if ((f != null) && (!convexFaces.Exists(cf => sameFace(cf, f)))) convexFaces.Add(f);
                }
            }
            FixNonConvexFaces(convexFaces);
            VCount = origVertices.Count;
            var cvxFNum = convexFaces.Count;
            #endregion

            #region Step 3 : Find Signed-Distance to each convex edge
            /* Of the 4 to 48 faces identified in the convex hull, we now define a matrix called edgeUnitVectors, 
             * check these against the remaining vertices. Just like for 2D we create an array of lists of tuples.
             * As we find new candidate convex points, we store them in hullCands. The second
             * part of the tuple (Item2 is a double) is the manhattan distance of the point from the center of the
             * IFaceConvHull. This manhattan distance is comprised of the distance parallel to the IFaceConvHull (dot-product) plus
             * the distance perpendicular to the IFaceConvHull (cross-product). This is used to order the vertices that
             * are found for a particular side (More on this in 23 lines). */
            //var hullCands = new SortedList<double, IVertexConvHull>;
            var candVertices = new SortedList<double, CandidateHullVertexData>(new noEqualSort());

            //var candVerticesOverEdge = new SortedList<double, CandidateHullVertexData>();

            /* Now a big loop. For each of the original vertices, check them with the 4 to 48 faces to see if they 
             * are inside or out. If they are out, add them to the proper row of the hullCands array. */
            for (int i = 0; i < VCount; i++)
            {
                double maxDotP = double.NegativeInfinity;
                SortedList<double, IFaceConvHull> overFaces = new SortedList<double, IFaceConvHull>(new noEqualSort());
                for (int j = 0; j < cvxFNum; j++)
                {
                    var bX = origVertices[i].X - convexFaces[j].v1.X;
                    var bY = origVertices[i].Y - convexFaces[j].v1.Y;
                    var bZ = origVertices[i].Z - convexFaces[j].v1.Z;
                    var dotP = dotProduct(convexFaces[j].normal[0], convexFaces[j].normal[1], convexFaces[j].normal[2], bX, bY, bZ);
                    if (dotP >= 0)
                    {
                        if (dotP >= maxDotP) maxDotP = dotP;
                        overFaces.Add(dotP, convexFaces[j]);
                    }
                }
                if (maxDotP >= 0)
                    candVertices.Add(maxDotP, new CandidateHullVertexData()
                    {
                        vertex = origVertices[i],
                        otherFaces = overFaces
                    });
            }
            VCount = candVertices.Count;
            for (int i = VCount - 1; i >= 0; i--)
            {
                var otherFaces = candVertices.Values[i].otherFaces;
                var vertex = candVertices.Values[i].vertex;
                for (int j = otherFaces.Count - 1; j >= 0; j--)
                {
                    if (convexFaces.Contains(otherFaces.Values[j]))
                    {
                        convexHull.Add(vertex);
                        replaceFace(convexFaces, convexFaces.IndexOf(otherFaces.Values[j]), vertex);
                        FixNonConvexFaces(convexFaces, 3);
                        candVertices.RemoveAt(i);
                        break;
                    }
                }
            }
            VCount = candVertices.Count;
            for (int i = VCount - 1; i >= 0; i--)
            {
                var vertex = candVertices.Values[i].vertex;
                for (int j = 0; j < convexFaces.Count; j++)
                {
                    var bX = vertex.X - convexFaces[j].v1.X;
                    var bY = vertex.Y - convexFaces[j].v1.Y;
                    var bZ = vertex.Z - convexFaces[j].v1.Z;
                    var dotP = dotProduct(convexFaces[j].normal[0], convexFaces[j].normal[1], convexFaces[j].normal[2], bX, bY, bZ);
                    if (dotP >= 0)
                    {
                        convexHull.Add(vertex);
                        replaceFace(convexFaces, j, vertex);
                        FixNonConvexFaces(convexFaces, 3);
                        break;
                    }
                }
            }
            #endregion
            return convexHull;
        }

        /// <summary>
        /// Find the convex hull for the 3D vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> Find3D(List<IVertexConvHull> vertices, Type face_Type = null)
        {
            /* first, the original vertices are copied as they will be modified
             * by this function. */
            faceType = face_Type;
            origVertices = new List<IVertexConvHull>(vertices);
            return Find3D();
        }
        /// <summary>
        /// Find the convex hull for the 3D vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <param name="faces">The faces.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> Find3D(List<IVertexConvHull> vertices, Type face_Type, out List<IFaceConvHull> faces)
        {
            /* first, the original vertices are copied as they will be modified
             * by this function. */
            faceType = face_Type;
            origVertices = new List<IVertexConvHull>(vertices);
            var convexVertices = Find3D();
            faces = convexFaces;
            return convexVertices;
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
        public static List<IVertexConvHull> Find3D(IList vertices, Type face_Type = null)
        {
            object[] arrayOfVertices = new object[vertices.Count];
            vertices.CopyTo(arrayOfVertices, 0);
            return Find3D(arrayOfVertices, face_Type);
        }
        /// <summary>
        /// Find the convex hull for the 3D vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> Find3D(object[] vertices, Type face_Type = null)
        {
            faceType = face_Type;
            origVertices = new List<IVertexConvHull>();
            for (int i = 0; i < vertices.GetLength(0); i++)
                origVertices.Add((IVertexConvHull)vertices[i]);
            return Find3D();
        }
        /// <summary>
        /// Find the convex hull for the 3D vertices.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="face_Type">Type of the face_.</param>
        /// <param name="faces">The faces.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> Find3D(IList vertices, Type face_Type, IList faces)
        {
            faceType = face_Type;
            var convexVertices = Find3D(vertices, faces[0].GetType());
            faces = convexFaces;
            return convexVertices;
        }

    }
}