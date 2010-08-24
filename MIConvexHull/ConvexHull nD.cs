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
        private static void FindConvexHull()
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
                                var extreme = StarMath.multiplyDot(new double[] { i - 1, j - 1, k - 1 }, n.location);
                                if (extreme > extremeValues[i, j, k])
                                {
                                    extremeVertexIndices[i, j, k] = m;
                                    extremeValues[i, j, k] = extreme;
                                }
                            }
            }
            #endregion
            #region Step #2: Define up to 48 faces of the Disdyakis dodecahedron
            var AklToussaintIndices = new List<int>();

            /* store vertices */
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        if ((!((i == 1) && (j == 1) && (k == 1)))
                        && !AklToussaintIndices.Contains(extremeVertexIndices[i, j, k]))
                            AklToussaintIndices.Add(extremeVertexIndices[i, j, k]);
            AklToussaintIndices.Sort(new noEqualSortMaxtoMinInt());
            for (int i = 0; i < AklToussaintIndices.Count; i++)
            {
                var currentVertex = origVertices[AklToussaintIndices[i]];
                convexHull.Add(currentVertex);
                updateCenter(convexHull, currentVertex);
                if (i == dimension)
                    convexFaces = initiateFaceDatabase();
                else if (i > dimension)
                {
                    var facesUnderVertex = findFacesBeneathInitialVertices(convexFaces, currentVertex);
                    updateFaces(facesUnderVertex, currentVertex);
                }
                origVertices.RemoveAt(AklToussaintIndices[i]);
            }
            #endregion

            #region Step #3: Consider all remaining vertices. Store them with the faces that they are 'beyond'
            var justTheFaces = new List<FaceData>(convexFaces.Values);
            foreach(var face in justTheFaces)
            {
                convexFaces.RemoveAt(convexFaces.IndexOfValue(face));
                face.verticesBeyond = findBeyondVertices(face, origVertices);
                if (face.verticesBeyond.Count == 0)
                    convexFaces.Add(-1.0, face);
                else convexFaces.Add(face.verticesBeyond.Keys[0], face);
            }
            #endregion

            #region Step #4: Now a final loop to expand the convex hull and faces based on these beyond vertices
            while (convexFaces.Keys[0] >= 0)
            {
                var currentFace = convexFaces.Values[0];
                var currentVertex = currentFace.verticesBeyond.Values[0];
                convexHull.Add(currentVertex);
                updateCenter(convexHull, currentVertex);

                var primaryFaces = findAffectedFaces(currentFace, currentVertex);
                updateFaces(primaryFaces, currentVertex);
            }
            #endregion
        }
    }
}