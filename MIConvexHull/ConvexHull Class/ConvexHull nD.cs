#region
using System;
using System.Collections.Generic;
using System.Linq;
using StarMathLib;

#endregion

namespace MIConvexHull
{
    /// <summary>
    ///   MIConvexHull for 3and higher dimensions.
    /// </summary>
    public partial class ConvexHull
    {
        /// <summary>
        ///   Finds the convex hull vertices.
        /// </summary>
        /// <returns></returns>
        private void FindConvexHull()
        {
            var VCount = origVertices.Count;
            /* as a heuristic, we limit the number of solutions created in the first loop, by an albeit, 
             * artificial formulation. This is to prevent the process from stagnating in this step in higher 
             * dimensions when the number of solutions on the Akl-Toussaint polygon gets too high (3^dimension).*/
            var maxAklTousNumber = coeffDimensions * dimension + coeffOffset;
            maxAklTousNumber = Math.Min(maxAklTousNumber, coeffNumVertices * VCount);
            /* of course, this limit is a moot point if there simply aren't enough points in the original
             * set of vertices. Therefore, it should at least be dimension + 1, the number of vertices in the
             * simplex. */
            maxAklTousNumber = Math.Max(maxAklTousNumber, dimension + 1);
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
            var midPoint = (numExtremes - 1) / 2;
            var flip = 1;
            do
            {
                var index = findIndex(ternaryPosition, midPoint);
                if (index == midPoint) continue;
                for (var m = 0; m < VCount; m++)
                {
                    var extreme = flip * MathUtils.multiplyDotFast(ternaryPosition, origVertices[m].coordinates, dimension);
                    if (extreme <= extremeValues[index]) continue;
                    AklToussaintIndices[index] = m;
                    extremeValues[index] = extreme;
                }
                flip *= -1;
                if (AklToussaintIndices.Distinct().Count() > maxAklTousNumber) break;
            } while (incrementTernaryPosition(ternaryPosition));
            AklToussaintIndices = AklToussaintIndices.Distinct().ToList();
            AklToussaintIndices.Remove(-1);
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
            var justTheFaces = convexFaces.Select(f => f.Value).ToArray();
            foreach (var face in justTheFaces)
            {
                convexFaces.Delete(face.fibCell);
                face.fibCell = null;
                findBeyondVertices(face, origVertices, dimension);
                if (face.verticesBeyond.Count == 0)
                {
                    face.fibCell = convexFaces.Enqueue(-1.0, face);
                }
                else
                {
                    double key = face.minVertexBeyond.Item1;
                    face.fibCell = convexFaces.Enqueue(key, face);
                }
            }

            #endregion
            
            #region Step #4: Now a final loop to expand the convex hull and faces based on these beyond vertices
            while (!convexFaces.IsEmpty && convexFaces.Top.Priority >= 0)
            {
                var tf = convexFaces.Top.Value;
                var currentFace = tf;
                var currentVertex = currentFace.minVertexBeyond.Item2;
                convexHull.Add(currentVertex);
                updateCenter(currentVertex);

                var primaryFaces = findAffectedFaces(currentFace, currentVertex);
                updateFaces(primaryFaces, currentVertex, true);
            }
            #endregion
        }

    }
}