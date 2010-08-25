#region
using System;
using System.Linq;
using System.Collections.Generic;
using StarMathLib;

#endregion

namespace MIConvexHullPluginNameSpace
{
    /// <summary>
    ///   MIConvexHull for 3D.
    /// </summary>
    public static partial class ConvexHull
    {
        /// <summary>
        ///   Finds the convex hull vertices.
        /// </summary>
        /// <returns></returns>
        private static void FindConvexHull()
        {
            var VCount = origVertices.Count;

            #region Step 1 : Define Convex Rhombicuboctahedron

            var numExtremes = (int)Math.Pow(3, dimension);
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
            var AklToussaintIndices = new List<int>(numExtremes);
            var extremeValues = new double[numExtremes];
            for (var k = 0; k < numExtremes; k++)
            {
                AklToussaintIndices.Add(-1);
                extremeValues[k] = double.NegativeInfinity;
            }
            var ternaryPosition = new int[dimension];
            for (var k = 0; k < dimension; k++)
                ternaryPosition[k] = -1;
            int midPoint = (numExtremes - 1) / 2;
            do
            {
                var index = findIndex(ternaryPosition, midPoint);
                if (index == midPoint) continue;
                for (var m = 0; m < VCount; m++)
                {
                    var extreme = StarMath.multiplyDot(ternaryPosition, origVertices[m].location);
                    if (extreme <= extremeValues[index]) continue;
                    AklToussaintIndices[index] = m;
                    extremeValues[index] = extreme;
                }
            } while (incrementTernaryPosition(ternaryPosition));
            AklToussaintIndices.RemoveAt(midPoint);
            AklToussaintIndices = AklToussaintIndices.Distinct().ToList();
            AklToussaintIndices.Sort(new noEqualSortMaxtoMinInt());
            #endregion

            #region Step #2: Define up to 48 faces of the Disdyakis dodecahedron
            for (var i = 0; i < AklToussaintIndices.Count; i++)
            {
                var currentVertex = origVertices[AklToussaintIndices[i]];
                convexHull.Add(currentVertex);
                updateCenter(currentVertex);
                if (i == dimension)
                    convexFaces = initiateFaceDatabase();
                else if (i > dimension)
                {
                    var facesUnderVertex = findFacesBeneathInitialVertices(currentVertex);
                    updateFaces(facesUnderVertex, currentVertex);
                }
                origVertices.RemoveAt(AklToussaintIndices[i]);
            }

            #endregion

            #region Step #3: Consider all remaining vertices. Store them with the faces that they are 'beyond'
            var justTheFaces = new List<FaceData>(convexFaces.Values);
            foreach (var face in justTheFaces)
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
                updateCenter(currentVertex);

                var primaryFaces = findAffectedFaces(currentFace, currentVertex);
                updateFaces(primaryFaces, currentVertex);
            }

            #endregion
        }

        private static Boolean incrementTernaryPosition(int[] ternaryPosition, int position = 0)
        {
            if (position == ternaryPosition.GetLength(0)) return false;
            ternaryPosition[position]++;
            if (ternaryPosition[position] == 2)
            {
                ternaryPosition[position] = -1;
                return incrementTernaryPosition(ternaryPosition, ++position);
            }
            return true;
        }

        private static int findIndex(IList<int> ternaryPosition, int midPoint)
        {
            var index = midPoint;
            var power = 1;
            for (var i = 0; i < dimension; i++)
            {
                index += power * ternaryPosition[i];
                power *= 3;
            }
            return index;
        }
    }
}