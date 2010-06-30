/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2006, 2008 Matthew Ira Campbell, PhD.
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
namespace MIConvexNameSpace
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// MIConvexHull for 3D is not yet complete. Most of the code from Find2D is shown below.
    /// Look for my 5-stars comments for things to do ("*****")
    /// </summary>
    public static partial class MIConvexHull
    {
        /// <summary>
        /// Finds the convex hull vertices.
        /// </summary>
        /// <param name="vertices">All of the vertices as a list.</param>
        /// <returns></returns>
        public static List<vertex> Find3D(List<vertex> vertices)
        {
            /* first, the original vertices are copied as they will be modified
             * by this function. */
            var origVertices = new List<vertex>(vertices);
            var origVNum = origVertices.Count;

            #region Step 1 : Define Convex Rhombicuboctahedron
            /* The first step is to quickly identify the four to 26 vertices based on the
             * Akl-Toussaint heuristic. In order to do this, I use a 3D matrix to help keep
             * track of the extreme. The 26 extrema can be see as approaching the cloud of 
             * points from the 26 faces of the Disdyakis dodecahedron 
             * (http://en.wikipedia.org/wiki/Disdyakis_dodecahedron although it may be easier
             * to understand by considering its dual, the Truncated cuboctahedron
             * (http://en.wikipedia.org/wiki/Truncated_cuboctahedron). This also corresponds
             * to base-3 (min,center,max) in three dimensions. Three raised to the third power
             * though is 27. the point at the center (0,0,0) is not used therefore 27 - 1 = 26.
             */
            vertex[, ,] extremeVertices = new vertex[3, 3, 3];
            int[, ,] extremeVertexIndices = new int[3, 3, 3];
            double[, ,] extremeValues = new double[3, 3, 3];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        extremeValues[i, j, k] = double.NegativeInfinity;


            for (int m = 0; m < origVNum; m++)
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
            var convexHull = new List<vertex>();
            var convexFaces = new List<face>();
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

            /* Faces: I couldn't find any logic in what made a positive direction to a face. As a result, the
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
                    var f = face.MakeFace(getVertexFromExtreme(extremeVertices, baseVert),
                       getVertexFromExtreme(extremeVertices, v1), getVertexFromExtreme(extremeVertices, v2));
                    if (f != null) convexFaces.Add(f);
                }
            }

            origVNum = origVertices.Count;
            //var cvxVNum = convexHull.Count;
            var cvxFNum = convexFaces.Count;
            var last = cvxFNum - 1;
            #endregion

            #region Step 3 : Find Signed-Distance to each convex edge
            /* Of the 4 to 48 faces identified in the convex hull, we now define a matrix called edgeUnitVectors, 
             * check these against the remaining vertices. Just like for 2D we create an array of lists of tuples.
             * As we find new candidate convex points, we store them in hullCands. The second
             * part of the tuple (Item2 is a double) is the manhattan distance of the point from the center of the
             * face. This manhattan distance is comprised of the distance parallel to the face (dot-product) plus
             * the distance perpendicular to the face (cross-product). This is used to order the vertices that
             * are found for a particular side (More on this in 23 lines). */
            var hullCands = new List<Tuple<vertex, double>>[cvxFNum];
            /* initialize the face Lists s.t. members can be added below. */
            for (int j = 0; j < cvxFNum; j++) hullCands[j] = new List<Tuple<vertex, double>>();

            /* Now a big loop. For each of the original vertices, check them with the 4 to 48 faces to see if they 
             * are inside or out. If they are out, add them to the proper row of the hullCands array. */
            for (int i = 0; i < origVNum; i++)
            {
                for (int j = 0; j < cvxFNum; j++)
                {
                    var bX = origVertices[i].X - convexFaces[j].center.X;
                    var bY = origVertices[i].Y - convexFaces[j].center.Y;
                    var bZ = origVertices[i].Z - convexFaces[j].center.Z;
                    var dotP = dotProduct(convexFaces[j].normal.X, convexFaces[j].normal.Y, convexFaces[j].normal.Z, bX, bY, bZ);
                    if (dotP >= 0)
                    {
                        var crossP = crossProduct(convexFaces[j].normal.X, convexFaces[j].normal.Y, convexFaces[j].normal.Z, bX, bY, bZ);
                        var magCross = Math.Sqrt((crossP.X * crossP.X) + (crossP.Y * crossP.Y) + (crossP.Z * crossP.Z));
                        var newSideCand = Tuple.Create(origVertices[i], dotP + magCross);
                        int k = 0;
                        while ((k < hullCands[j].Count) && (newSideCand.Item2 > hullCands[j][k].Item2)) k++;
                        hullCands[j].Insert(k, newSideCand);
                        break;
                    }
                }
            }
            #endregion

            #region Step 3: now check the remaining hull candidates
            for (int i = cvxFNum - 1; i >= 0; i--)
            {
                if (hullCands[i].Count == 1)
                {
                    /* If there is one and only one candidate, it must be in the convex hull. Add it now. */
                    convexHull.Add(hullCands[i][0].Item1);
                    /* what about faces? should these be updated? Maybe, but our resulting polyhedron is not 
                     * as pretty as a Delaunay-ized one. We'll have some poor-aspect ratio triangles. */
                    replaceFace(convexFaces, i, hullCands[i][0].Item1);
                }
                else if (hullCands[i].Count > 1)
                {
                    var subFaces = new List<face>();
                    subFaces.Add(convexFaces[i]);
                    for (int j = hullCands[i].Count - 1; j >= 0; j--)
                    {
                        for (int k = subFaces.Count - 1; k >= 0; k--)
                        {
                            var bX = hullCands[i][j].Item1.X - subFaces[k].center.X;
                            var bY = hullCands[i][j].Item1.Y - subFaces[k].center.Y;
                            var bZ = hullCands[i][j].Item1.Z - subFaces[k].center.Z;
                            var dotP = dotProduct(subFaces[k].normal.X, subFaces[k].normal.Y, subFaces[k].normal.Z, bX, bY, bZ);
                            if (dotP >= 0)
                            {
                                replaceFace(subFaces, k, hullCands[i][j].Item1);
                                convexHull.Add(hullCands[i][j].Item1);
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            /* finally return the hull points. */
            return convexHull;

        }


        /// <summary>
        /// An overload that takes the vertices as an nX3 matrix, where the first column
        /// is the x values of the matrix, the second column is the y values, and the third is the
        /// z values. It returns a similar matrix comprised only of the convex hull ordered in a 
        /// counter-clock-wise loop.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns></returns>
        public static double[,] Find3D(double[,] vertices)
        {
            var numRows = vertices.GetLength(0);
            var vList = new List<vertex>(numRows);
            for (int i = 0; i < numRows; i++)
                vList.Add(new vertex(vertices[i, 0], vertices[i, 1]));

            List<vertex> convexHull = Find2D(vList);
            numRows = convexHull.Count;
            double[,] result = new double[numRows, 2];
            for (int i = 0; i < numRows; i++)
            {
                result[i, 0] = convexHull[i].X;
                result[i, 1] = convexHull[i].Y;
            }
            return result;
        }
    }
}