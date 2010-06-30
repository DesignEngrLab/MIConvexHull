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
            throw new NotImplementedException();
            /* first, the original vertices are copied as they will be modified
             * by this function. */ 
            var origVertices = new List<vertex>(vertices);

            /* convexHullCCW is the result of this function. It is a list of 
             * vertices found in the original vertices and ordered to make a
             * counter-clockwise loop beginning with the leftmost (minimum
             * value of X) vertex. */ 
            var convexHullCCW = new List<vertex>();

            #region Step 1 : Define Convex Octogon
            /* The first step is to quickly identify the three to eight vertices based on the
             * Akl-Toussaint heuristic. */
            /***** need additional points for 3D ******/
            /***** does it still work for 3D? *****/
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;
            double maxSum = double.NegativeInfinity;
            double maxDiff = double.NegativeInfinity;
            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double minSum = double.PositiveInfinity;
            double minDiff = double.PositiveInfinity;
            vertex nodeMaxX = null, nodeMaxY = null, nodeMaxSum = null, nodeMaxDiff = null;
            vertex nodeMinX = null, nodeMinY = null, nodeMinSum = null, nodeMinDiff = null;
            foreach (vertex n in origVertices)
            {
                if (n.X > maxX) { nodeMaxX = n; maxX = n.X; }
                if (n.Y > maxY) { nodeMaxY = n; maxY = n.Y; }
                if ((n.X + n.Y) > maxSum) { nodeMaxSum = n; maxSum = n.X + n.Y; }
                if ((n.X - n.Y) > maxDiff) { nodeMaxDiff = n; maxDiff = n.X - n.Y; }
                if (n.X < minX) { nodeMinX = n; minX = n.X; }
                if (n.Y < minY) { nodeMinY = n; minY = n.Y; }
                if ((n.X + n.Y) < minSum) { nodeMinSum = n; minSum = n.X + n.Y; }
                if ((n.X - n.Y) < minDiff) { nodeMinDiff = n; minDiff = n.X - n.Y; }
            }
            convexHullCCW.Add(nodeMinX); origVertices.Remove(nodeMinX);
            if (!convexHullCCW.Contains(nodeMinSum)) { convexHullCCW.Add(nodeMinSum); origVertices.Remove(nodeMinSum); }
            if (!convexHullCCW.Contains(nodeMinY)) { convexHullCCW.Add(nodeMinY); origVertices.Remove(nodeMinY); }
            if (!convexHullCCW.Contains(nodeMaxDiff)) { convexHullCCW.Add(nodeMaxDiff); origVertices.Remove(nodeMaxDiff); }
            if (!convexHullCCW.Contains(nodeMaxX)) { convexHullCCW.Add(nodeMaxX); origVertices.Remove(nodeMaxX); }
            if (!convexHullCCW.Contains(nodeMaxSum)) { convexHullCCW.Add(nodeMaxSum); origVertices.Remove(nodeMaxSum); }
            if (!convexHullCCW.Contains(nodeMaxY)) { convexHullCCW.Add(nodeMaxY); origVertices.Remove(nodeMaxY); }
            if (!convexHullCCW.Contains(nodeMinDiff)) { convexHullCCW.Add(nodeMinDiff); origVertices.Remove(nodeMinDiff); }
            #endregion

            /* the following limits are used extensively in for-loop below. In order to reduce the arithmetic calls and
             * steamline the code, these are established. */
            var origVNum = origVertices.Count;
            var cvxVNum = convexHullCCW.Count;
            var last = cvxVNum - 1;

            #region Step 2 : Find Signed-Distance to each convex edge
            /***** need to check distances to faces *****/
            var convexVectInfo = new double[cvxVNum, 4];

            for (int i = 0; i < last; i++)
            {
                convexVectInfo[i, 0] = convexHullCCW[i + 1].X - convexHullCCW[i].X;
                convexVectInfo[i, 1] = convexHullCCW[i + 1].Y - convexHullCCW[i].Y;
                /***** and for z *****/
                convexVectInfo[i, 3] = Math.Sqrt(convexVectInfo[i, 0] * convexVectInfo[i, 0] +
                    convexVectInfo[i, 1] * convexVectInfo[i, 1]);
            }
            convexVectInfo[last, 0] = convexHullCCW[0].X - convexHullCCW[last].X;
            convexVectInfo[last, 1] = convexHullCCW[0].Y - convexHullCCW[last].Y;
            /***** and for z *****/
            convexVectInfo[last, 3] = Math.Sqrt(convexVectInfo[last, 0] * convexVectInfo[last, 0] +
                convexVectInfo[last, 1] * convexVectInfo[last, 1]);

            /***** does this now need to be a 3-tuple? are two distances needed? well, at any rate, sorting wouldn't
             ***** be possible. hmmm... *****/
            var hullCands = new List<Tuple<vertex, double>>[cvxVNum];
        
            for (int j = 0; j < cvxVNum; j++) hullCands[j] = new List<Tuple<vertex, double>>();

            /* Now a big loop. For each of the original vertices, check them with the 3 to 8 edges to see if they 
             * are inside or out. If they are out, add them to the proper row of the hullCands array. */
            for (int i = 0; i < origVNum; i++)
            {
                for (int j = 0; j < cvxVNum; j++)
                {
                    var bX = origVertices[i].X - convexHullCCW[j].X;
                    var bY = origVertices[i].Y - convexHullCCW[j].Y;
           
                    if (crossProduct(convexVectInfo[j, 0], convexVectInfo[j, 1], bX, bY) <= 0)
                    {

                        Tuple<vertex, double> newSideCand = Tuple.Create(origVertices[i],
                            dotProduct(convexVectInfo[j, 0], convexVectInfo[j, 1], bX, bY));
                        int k = 0;
                        while ((k < hullCands[j].Count) && (newSideCand.Item2 > hullCands[j][k].Item2)) k++;
                        hullCands[j].Insert(k, newSideCand);
                        break;
                    }
                }
            }
            #endregion

            #region Step 3: now check the remaining hull candidates
            /* Now it's time to go through our array of sorted lists of tuples. We search backwards through
             * the current convex hull points s.t. any additions will not confuse our for-loop indexers. */
            for (int j = cvxVNum; j > 0; j--)
            {
                if (hullCands[j - 1].Count == 1)
                    /* If there is one and only one candidate, it must be in the convex hull. Add it now. */
                    convexHullCCW.Insert(j, hullCands[j - 1][0].Item1);
                else if (hullCands[j - 1].Count > 1)
                {
                    /* If there's more than one than...Well, now comes the tricky part. Here is where the
                     * most time is spent for large sets. this is the O(N*logN) part (the previous steps
                     * were all linear). The above octagon trick was to conquer and divide the candidates. */

                    /* a renaming for compactness and clarity */
                    var hc = hullCands[j - 1];

                    /* put the known starting vertex as the beginning of the list. No need for the "positionAlong"
                     * anymore since the list is now sorted. At any rate, the positionAlong is zero. */
                    hc.Insert(0, Tuple.Create(convexHullCCW[j - 1], 0.0));
                    /* put the ending vertex on the end of the list. Need to check if it wraps back around to 
                     * the first in the loop (hence the simple condition). */
                    if (j == cvxVNum) hc.Add(Tuple.Create(convexHullCCW[0], double.NaN));
                    else hc.Add(Tuple.Create(convexHullCCW[j], double.NaN));

                    /* Now starting from second from end, work backwards looks for places where the angle 
                     * between the vertices in concave (which would produce a negative value of z). */
                    int i = hc.Count - 2;
                    while (i > 0)
                    {
                        var zValue = crossProduct(hc[i].Item1.X - hc[i - 1].Item1.X,
                            hc[i].Item1.Y - hc[i - 1].Item1.Y,
                            hc[i + 1].Item1.X - hc[i].Item1.X,
                            hc[i + 1].Item1.Y - hc[i].Item1.Y);
                        if (zValue < 0)
                        {
                            /* remove any vertices that create concave angles. */
                            hc.RemoveAt(i);
                            /* but don't reduce i since we need to check the previous angle again. Well, 
                             * if you're back to the end you do need to reduce i (hence the line below). */
                            if (i == hc.Count - 1) i--;
                        }
                        /* if the angle is convex, then continue toward the start, i-- */
                        else i--;
                    }
                    /* for each of the remaining vertices in hullCands[j-1], add them to the convexHullCCW. 
                     * Here we insert them backwards (i counts down) to simplify the insert operation (i.e.
                     * since all are inserted @ j, the previous inserts are pushed up to j+1, j+2, etc. */
                    for (i = hc.Count - 2; i > 0; i--)
                        convexHullCCW.Insert(j, hc[i].Item1);
                }
            }
            #endregion

            /* finally return the hull points. */
            return convexHullCCW;
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