/******************************************************************************
 *
 * The MIT License (MIT)
 *
 * MIConvexHull, Copyright (c) 2015 David Sehnal, Matthew Campbell
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *  
 *****************************************************************************/

namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /*
     * This part of the implementation handles initialization of the convex hull algorithm:
     * 
     * - Determine the dimension by looking at length of Position vector of 10 random data points from the input. 
     * - Identify 2 * Dimension extreme points in each direction.
     * - Pick (Dimension + 1) points from the extremes and construct the initial simplex.
     */
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// Wraps the vertices and determines the dimension if it's unknown.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="lift"></param>
        /// <param name="config"></param>
        private ConvexHullInternal(IVertex[] vertices, bool lift, ConvexHullComputationConfig config)
        {
            if (config.PointTranslationType != PointTranslationType.None && config.PointTranslationGenerator == null)
            {
                throw new InvalidOperationException("PointTranslationGenerator cannot be null if PointTranslationType is enabled.");
            }

            this.IsLifted = lift;
            this.Vertices = vertices;
            this.PlaneDistanceTolerance = config.PlaneDistanceTolerance;

            Dimension = DetermineDimension();
            if (Dimension < 2) throw new InvalidOperationException("Dimension of the input must be 2 or greater.");

            if (lift) Dimension++;
            InitializeData(config);
        }

        /// <summary>
        /// Check the dimensionality of the input data.
        /// </summary>
        int DetermineDimension()
        {
            var r = new Random();
            var vCount = Vertices.Length;
            var dimensions = new List<int>();
            for (var i = 0; i < 10; i++)
                dimensions.Add(Vertices[r.Next(vCount)].Position.Length);
            var dimension = dimensions.Min();
            if (dimension != dimensions.Max()) throw new ArgumentException("Invalid input data (non-uniform dimension).");
            return dimension;
        }

        /// <summary>
        /// Create the first faces from (dimension + 1) vertices.
        /// </summary>
        /// <returns></returns>
        int[] CreateInitialHull(List<int> initialPoints)
        {
            var faces = new int[Dimension + 1];

            for (var i = 0; i < Dimension + 1; i++)
            {
                var vertices = new int[Dimension];
                for (int j = 0, k = 0; j <= Dimension; j++)
                {
                    if (i != j) vertices[k++] = initialPoints[j];
                }
                var newFace = FacePool[ObjectManager.GetFace()];
                newFace.Vertices = vertices;
                Array.Sort(vertices);
                MathHelper.CalculateFacePlane(newFace, Center);
                faces[i] = newFace.Index;
            }

            // update the adjacency (check all pairs of faces)
            for (var i = 0; i < Dimension; i++)
            {
                for (var j = i + 1; j < Dimension + 1; j++) UpdateAdjacency(FacePool[faces[i]], FacePool[faces[j]]);
            }

            return faces;
        }


        /// <summary>
        /// Check if 2 faces are adjacent and if so, update their AdjacentFaces array.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        void UpdateAdjacency(ConvexFaceInternal l, ConvexFaceInternal r)
        {
            var lv = l.Vertices;
            var rv = r.Vertices;
            int i;

            // reset marks on the 1st face
            for (i = 0; i < lv.Length; i++) VertexMarks[lv[i]] = false;

            // mark all vertices on the 2nd face
            for (i = 0; i < rv.Length; i++) VertexMarks[rv[i]] = true;

            // find the 1st false index
            for (i = 0; i < lv.Length; i++) if (!VertexMarks[lv[i]]) break;

            // no vertex was marked
            if (i == Dimension) return;

            // check if only 1 vertex wasn't marked
            for (int j = i + 1; j < lv.Length; j++) if (!VertexMarks[lv[j]]) return;

            // if we are here, the two faces share an edge
            l.AdjacentFaces[i] = r.Index;

            // update the adj. face on the other face - find the vertex that remains marked
            for (i = 0; i < lv.Length; i++) VertexMarks[lv[i]] = false;
            for (i = 0; i < rv.Length; i++)
            {
                if (VertexMarks[rv[i]]) break;
            }
            r.AdjacentFaces[i] = l.Index;
        }

        /// <summary>
        /// Init the hull if Vertices.Length == Dimension.
        /// </summary>
        void InitSingle()
        {
            var vertices = new int[Dimension];
            for (int i = 0; i < Vertices.Length; i++)
            {
                vertices[i] = i;
            }

            var newFace = FacePool[ObjectManager.GetFace()];
            newFace.Vertices = vertices;
            Array.Sort(vertices);
            MathHelper.CalculateFacePlane(newFace, Center);

            // Make sure the normal point downwards in case this is used for triangulation
            if (newFace.Normal[Dimension - 1] >= 0.0)
            {
                for (int i = 0; i < Dimension; i++)
                {
                    newFace.Normal[i] *= -1.0;
                }
                newFace.Offset = -newFace.Offset;
                newFace.IsNormalFlipped = !newFace.IsNormalFlipped;
            }

            ConvexFaces.Add(newFace.Index);
        }

        /// <summary>
        /// Find the (dimension+1) initial points and create the simplexes.
        /// </summary>
        void InitConvexHull()
        {
            if (Vertices.Length < Dimension)
            {
                // In this case, there cannot be a single convex face, so we return an empty result.                
                return;
            }
            else if (Vertices.Length == Dimension)
            {
                // The vertices are all on the hull and form a single simplex.
                InitSingle();
                return;
            }

            var extremes = FindExtremes();
            var initialPoints = FindInitialPoints(extremes);

            // Add the initial points to the convex hull.
            foreach (var vertex in initialPoints)
            {
                CurrentVertex = vertex;
                // update center must be called before adding the vertex.
                UpdateCenter();

                // Mark the vertex so that it's not included in any beyond set.
                VertexMarks[vertex] = true;
            }

            // Create the initial simplexes.
            var faces = CreateInitialHull(initialPoints);

            // Init the vertex beyond buffers.
            foreach (var faceIndex in faces)
            {
                var face = FacePool[faceIndex];
                FindBeyondVertices(face);
                if (face.VerticesBeyond.Count == 0) ConvexFaces.Add(face.Index); // The face is on the hull
                else UnprocessedFaces.Add(face);
            }

            // Unmark the vertices
            foreach (var vertex in initialPoints) VertexMarks[vertex] = false;

        }


        /// <summary>
        /// Used in the "initialization" code.
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face)
        {
            var beyondVertices = face.VerticesBeyond;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = 0;

            int count = Vertices.Length;
            for (int i = 0; i < count; i++)
            {
                if (VertexMarks[i]) continue;
                IsBeyond(face, beyondVertices, i);
            }

            face.FurthestVertex = FurthestVertex;
        }

        /// <summary>
        /// Finds (dimension + 1) initial points.
        /// </summary>
        /// <param name="extremes"></param>
        /// <returns></returns>
        private List<int> FindInitialPoints(List<int> extremes)
        {
            List<int> initialPoints = new List<int>();

            int first = -1, second = -1;
            double maxDist = 0;
            double[] temp = new double[Dimension];
            for (int i = 0; i < extremes.Count - 1; i++)
            {
                var a = extremes[i];
                for (int j = i + 1; j < extremes.Count; j++)
                {
                    var b = extremes[j];
                    MathHelper.SubtractFast(a, b, temp);
                    var dist = MathHelper.LengthSquared(temp);
                    if (dist > maxDist)
                    {
                        first = a;
                        second = b;
                        maxDist = dist;
                    }
                }
            }

            initialPoints.Add(first);
            initialPoints.Add(second);

            for (int i = 2; i <= Dimension; i++)
            {
                double maximum = double.NegativeInfinity;
                int maxPoint = -1;
                for (int j = 0; j < extremes.Count; j++)
                {
                    var extreme = extremes[j];
                    if (initialPoints.Contains(extreme)) continue;

                    var val = GetSquaredDistanceSum(extreme, initialPoints);

                    if (val > maximum)
                    {
                        maximum = val;
                        maxPoint = extreme;
                    }
                }

                if (maxPoint >= 0) initialPoints.Add(maxPoint);
                else
                {
                    int vCount = Vertices.Length;
                    for (int j = 0; j < vCount; j++)
                    {
                        if (initialPoints.Contains(j)) continue;

                        var val = GetSquaredDistanceSum(j, initialPoints);

                        if (val > maximum)
                        {
                            maximum = val;
                            maxPoint = j;
                        }
                    }

                    if (maxPoint >= 0) initialPoints.Add(maxPoint);
                    else ThrowSingular();
                }
            }
            return initialPoints;
        }

        /// <summary>
        /// Computes the sum of square distances to the initial points.
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="initialPoints"></param>
        /// <returns></returns>
        double GetSquaredDistanceSum(int pivot, List<int> initialPoints)
        {
            var initPtsNum = initialPoints.Count;
            var sum = 0.0;

            for (int i = 0; i < initPtsNum; i++)
            {
                var initPt = initialPoints[i];
                for (int j = 0; j < Dimension; j++)
                {
                    double t = GetCoordinate(initPt, j) - GetCoordinate(pivot, j);
                    sum += t * t;
                }
            }

            return sum;
        }

        private int LexCompare(int u, int v)
        {
            int uOffset = u * Dimension, vOffset = v * Dimension;
            for (int i = 0; i < Dimension; i++)
            {
                double x = Positions[uOffset + i], y = Positions[vOffset + i];
                int comp = x.CompareTo(y);
                if (comp != 0) return comp;
            }
            return 0;
        }

        /// <summary>
        /// Finds the extremes in all dimensions.
        /// </summary>
        /// <returns></returns>
        private List<int> FindExtremes()
        {
            var extremes = new HashSet<int>();

            int vCount = Vertices.Length;
            for (int i = 0; i < Dimension; i++)
            {
                double min = double.MaxValue, max = double.MinValue;
                int minInd = 0, maxInd = 0;
                for (int j = 0; j < vCount; j++)
                {
                    var v = GetCoordinate(j, i);
                    var diff = min - v;
                    if (diff >= PlaneDistanceTolerance)
                    {
                        min = v;
                        minInd = j;
                    }

                    diff = v - max;
                    if (diff >= PlaneDistanceTolerance)
                    {
                        max = v;
                        maxInd = j;
                    }
                }

                extremes.Add(minInd);
                extremes.Add(maxInd);
            }

            // Do we have enough unique extreme points?
            if (extremes.Count <= Dimension)
            {
                // If not, just add the "first" non-included ones.
                int i = 0;
                while (i < vCount && extremes.Count <= Dimension)
                {
                    extremes.Add(i);
                    i++;
                }
            }

            return extremes.ToList();
        }

        /// <summary>
        /// The exception thrown if singular input data detected.
        /// </summary>
        void ThrowSingular()
        {
            throw new InvalidOperationException(
                    "Singular input data (i.e. trying to triangulate a data that contain a regular lattice of points) detected. "
                    + "Introducing some noise to the data might resolve the issue.");
        }
    }
}
