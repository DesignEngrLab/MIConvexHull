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
        /// Finds the convex hull vertices.
        /// </summary>
        /// <returns></returns>
        static List<IVertexConvHull> Find3D()
        {
            dimension = 3;
            center = new double[dimension];
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
            int[, ,] extremeVertexIndices = new int[3, 3, 3];
            double[, ,] extremeValues = new double[3, 3, 3];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        extremeValues[i, j, k] = double.NegativeInfinity;


            for (int m = 0; m < VCount; m++)
            {
                var n = origVertices[m];
                center = StarMath.add(center, n.location);
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        for (int k = 0; k < 3; k++)
                            if (!((i == 1) && (j == 1) && (k == 1)))
                            {
                                var extreme = StarMath.multiplyDot(new double[] { i - 1, j - 1, k - 1 }, n.location);
                                if (extreme > extremeValues[i, j, k])
                                {
                                    extremeVertexIndices[i, j, k] = m;
                                    extremeValues[i, j, k] = extreme;
                                }
                            }
            }
            #endregion
            for (int i = 0; i < dimension; i++) center[i] /= VCount;
            #region Step #2: Define up to 48 faces of the Disdyakis dodecahedron
            var AklToussaintIndices = new List<int>();
            var indicesToDelete = new Dictionary<int, Boolean>(new noEqualSortMaxtoMinInt());

            /* store vertices */
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        if ((!((i == 1) && (j == 1) && (k == 1)))
                        && !AklToussaintIndices.Contains(extremeVertexIndices[i, j, k]))
                            AklToussaintIndices.Add(extremeVertexIndices[i, j, k]);

            int numInSimplex = dimension + 1;
            double stepSize = ((double)(AklToussaintIndices.Count - 1)) / dimension;

            var convexHull = new List<IVertexConvHull>();
            for (int i = numInSimplex - 1; i >= 0; i--)
            {
                int index = (int)(i * stepSize);
                convexHull.Add(origVertices[AklToussaintIndices[index]]);
                indicesToDelete.Add(AklToussaintIndices[index], false);
                AklToussaintIndices.RemoveAt(index);
            }
            convexFaces = new List<IFaceConvHull>();
            for (int i = 0; i < numInSimplex; i++)
                convexFaces.Add(MakeFace(convexHull, i));
            foreach (int i in AklToussaintIndices)
            {
                var currentVertex = origVertices[i];
                indicesToDelete.Add(i, false);
                convexHull.Add(currentVertex);
                var overFaces = findOverFaces(convexFaces, currentVertex);
                replaceFace(convexFaces, overFaces.Values, currentVertex);
            }

            foreach (int i in indicesToDelete.Keys)
                origVertices.RemoveAt(i);

            // FixNonConvexFaces(convexFaces);
            VCount = origVertices.Count;
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
            var candVertices = new SortedList<double, CandidateHullVertexData>(new noEqualSortMaxtoMinDouble());
            /* Now a big loop. For each of the original vertices, check them with the 4 to 48 faces to see if they 
             * are inside or out. If they are out, add them to the proper row of the hullCands array. */
            for (int i = 0; i < VCount; i++)
            {
                var currentVertex = origVertices[i];
                var overFaces = findOverFaces(convexFaces, currentVertex);
                if (overFaces.Count > 0)
                    candVertices.Add(overFaces.Keys[0], new CandidateHullVertexData()
                    {
                        vertex = currentVertex,
                        otherFaces = overFaces
                    });
            }
            while (candVertices.Count > 0)
            {
                var currentVertex = candVertices.Values[0].vertex;
                var intersectFaces = convexFaces.Intersect(candVertices.Values[0].otherFaces.Values).ToList();
                candVertices.RemoveAt(0);
                if (intersectFaces.Count() > 0)
                {
                    convexHull.Add(currentVertex);
                    var affectedCands = (from c in candVertices
                                         where (c.Value.otherFaces.Values.Intersect(intersectFaces).Count() > 0)
                                         select c).ToList();
                    replaceFace(convexFaces, intersectFaces, currentVertex);
                    foreach (var affectedCand in affectedCands)
                    {
                        candVertices.Remove(affectedCand.Key);
                        var affectedVertex = affectedCand.Value.vertex;
                        var overFaces = findOverFaces(convexFaces, affectedVertex);
                        if (overFaces.Count > 0)
                            candVertices.Add(overFaces.Keys[0], new CandidateHullVertexData()
                            {
                                vertex = affectedVertex,
                                otherFaces = overFaces
                            });
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