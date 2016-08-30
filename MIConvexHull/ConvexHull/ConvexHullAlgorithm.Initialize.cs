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

using System;
using System.Collections.Generic;
using System.Linq;

namespace MIConvexHull
{
    /*
     * This part of the implementation handles initialization of the convex hull algorithm:
     * 
     * - Determine the dimension by looking at length of Position vector of 10 random data points from the input. 
     * - Identify 2 * Dimension extreme points in each direction.
     * - Pick (Dimension + 1) points from the extremes and construct the initial simplex.
     */

    /// <summary>
    /// Class ConvexHullAlgorithm.
    /// </summary>
    internal partial class ConvexHullAlgorithm
    {
        #region Starting functions and constructor
        /// <summary>
        /// The main function for the Convex Hull algorithm. It is static, but it creates
        /// an instantiation of this class in order to allow for parallel execution.
        /// Following this simple function, the constructor and the main function "FindConvexHull" is listed.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertices in the data.</typeparam>
        /// <typeparam name="TFace">The desired type of the faces.</typeparam>
        /// <param name="data">The data is the vertices as a collection of IVertices.</param>
        /// <param name="config">The configuration. If null, default ConvexHullComputationConfig.GetDefault() is used.</param>
        /// <returns>MIConvexHull.ConvexHull&lt;TVertex, TFace&gt;.</returns>
        internal static ConvexHull<TVertex, TFace> GetConvexHull<TVertex, TFace>(IList<TVertex> data,
                    ConvexHullComputationConfig config)
                    where TFace : ConvexFace<TVertex, TFace>, new()
                    where TVertex : IVertex
        {
            config = config ?? new ConvexHullComputationConfig();

            var vertices = new IVertex[data.Count];
            for (var i = 0; i < data.Count; i++) vertices[i] = data[i];
            var ch = new ConvexHullAlgorithm(vertices, false, config);
            ch.GetConvexHull();

            return new ConvexHull<TVertex, TFace>
            {
                Points = ch.GetHullVertices(data),
                Faces = ch.GetConvexFaces<TVertex, TFace>()
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvexHullAlgorithm"/> class.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="lift">if set to <c>true</c> [lift].</param>
        /// <param name="config">The configuration.</param>
        /// <exception cref="InvalidOperationException">
        /// PointTranslationGenerator cannot be null if PointTranslationType is enabled.
        /// or
        /// Dimension of the input must be 2 or greater.
        /// </exception>
        /// <exception cref="ArgumentException">There are too few vertices (m) for the n-dimensional space. (m must be greater " +
        ///                     "than the n, but m is " + NumberOfVertices + " and n is " + Dimension</exception>
        private ConvexHullAlgorithm(IVertex[] vertices, bool lift, ConvexHullComputationConfig config)
        {
            if (config.PointTranslationType != PointTranslationType.None && config.PointTranslationGenerator == null)
            {
                throw new InvalidOperationException(
                    "PointTranslationGenerator cannot be null if PointTranslationType is enabled.");
            }

            IsLifted = lift;
            Vertices = vertices;
            NumberOfVertices = vertices.Length;
            PlaneDistanceTolerance = config.PlaneDistanceTolerance;

            Dimension = DetermineDimension();
            if (Dimension < 2) throw new InvalidOperationException("Dimension of the input must be 2 or greater.");
            if (NumberOfVertices <= Dimension)
                throw new ArgumentException(
                    "There are too few vertices (m) for the n-dimensional space. (m must be greater " +
                    "than the n, but m is " + NumberOfVertices + " and n is " + Dimension);
            if (lift) Dimension++;

            UnprocessedFaces = new FaceList();
            ConvexFaces = new IndexBuffer();

            FacePool = new ConvexFaceInternal[(Dimension + 1) * 10]; // must be initialized before object manager
            AffectedFaceFlags = new bool[(Dimension + 1) * 10];
            ObjectManager = new ObjectManager(this);

            Center = new double[Dimension];
            TraverseStack = new IndexBuffer();
            UpdateBuffer = new int[Dimension];
            UpdateIndices = new int[Dimension];
            EmptyBuffer = new IndexBuffer();
            AffectedFaceBuffer = new IndexBuffer();
            ConeFaceBuffer = new SimpleList<DeferredFace>();
            SingularVertices = new HashSet<int>();
            BeyondBuffer = new IndexBuffer();

            ConnectorTable = new ConnectorList[ConnectorTableSize];
            for (var i = 0; i < ConnectorTableSize; i++) ConnectorTable[i] = new ConnectorList();

            VertexVisited = new bool[NumberOfVertices];
            InitializePositions(config);

            MathHelper = new MathHelper(Dimension, Positions);
        }

        /// <summary>
        /// Gets/calculates the convex hull. This is 
        /// </summary>
        private void GetConvexHull()
        {
            // Find the (dimension+1) initial points and create the simplexes.
            CreateInitialSimplex();

            // Expand the convex hull and faces.
            while (UnprocessedFaces.First != null)
            {
                var currentFace = UnprocessedFaces.First;
                CurrentVertex = currentFace.FurthestVertex;

                UpdateCenter();

                // The affected faces get tagged
                TagAffectedFaces(currentFace);

                // Create the cone from the currentVertex and the affected faces horizon.
                if (!SingularVertices.Contains(CurrentVertex) && CreateCone()) CommitCone();
                else HandleSingular();

                // Need to reset the tags
                var count = AffectedFaceBuffer.Count;
                for (var i = 0; i < count; i++) AffectedFaceFlags[AffectedFaceBuffer[i]] = false;
            }
        }

        #endregion

        /// <summary>
        /// Initialize the vertex positions based on the translation type from config.
        /// </summary>
        /// <param name="config">The configuration.</param>
        private void InitializePositions(ConvexHullComputationConfig config)
        {
            Positions = new double[NumberOfVertices * Dimension];
            var index = 0;
            if (IsLifted)
            {
                var origDim = Dimension - 1;
                var tf = config.PointTranslationGenerator;
                switch (config.PointTranslationType)
                {
                    case PointTranslationType.None:
                        foreach (var v in Vertices)
                        {
                            var lifted = 0.0;
                            for (var i = 0; i < origDim; i++)
                            {
                                var t = v.Position[i];
                                Positions[index++] = t;
                                lifted += t * t;
                            }
                            Positions[index++] = lifted;
                        }
                        break;
                    case PointTranslationType.TranslateInternal:
                        foreach (var v in Vertices)
                        {
                            var lifted = 0.0;
                            for (var i = 0; i < origDim; i++)
                            {
                                var t = v.Position[i] + tf();
                                Positions[index++] = t;
                                lifted += t * t;
                            }
                            Positions[index++] = lifted;
                        }
                        break;
                }
            }
            else
            {
                var tf = config.PointTranslationGenerator;
                switch (config.PointTranslationType)
                {
                    case PointTranslationType.None:
                        foreach (var v in Vertices)
                        {
                            for (var i = 0; i < Dimension; i++) Positions[index++] = v.Position[i];
                        }
                        break;
                    case PointTranslationType.TranslateInternal:
                        foreach (var v in Vertices)
                        {
                            for (var i = 0; i < Dimension; i++) Positions[index++] = v.Position[i] + tf();
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// Check the dimensionality of the input data.
        /// </summary>
        /// <returns>System.Int32.</returns>
        /// <exception cref="ArgumentException">Invalid input data (non-uniform dimension).</exception>
        private int DetermineDimension()
        {
            var r = new Random();
            var dimensions = new List<int>();
            for (var i = 0; i < 10; i++)
                dimensions.Add(Vertices[r.Next(NumberOfVertices)].Position.Length);
            var dimension = dimensions.Min();
            if (dimension != dimensions.Max())
                throw new ArgumentException("Invalid input data (non-uniform dimension).");
            return dimension;
        }

        /// <summary>
        /// Check if 2 faces are adjacent and if so, update their AdjacentFaces array.
        /// </summary>
        /// <param name="l">The l.</param>
        /// <param name="r">The r.</param>
        private void UpdateAdjacency(ConvexFaceInternal l, ConvexFaceInternal r)
        {
            var lv = l.Vertices;
            var rv = r.Vertices;
            int i;

            // reset marks on the 1st face
            for (i = 0; i < lv.Length; i++) VertexVisited[lv[i]] = false;

            // mark all vertices on the 2nd face
            for (i = 0; i < rv.Length; i++) VertexVisited[rv[i]] = true;

            // find the 1st false index
            for (i = 0; i < lv.Length; i++) if (!VertexVisited[lv[i]]) break;

            // no vertex was marked
            if (i == Dimension) return;

            // check if only 1 vertex wasn't marked
            for (var j = i + 1; j < lv.Length; j++) if (!VertexVisited[lv[j]]) return;

            // if we are here, the two faces share an edge
            l.AdjacentFaces[i] = r.Index;

            // update the adj. face on the other face - find the vertex that remains marked
            for (i = 0; i < lv.Length; i++) VertexVisited[lv[i]] = false;
            for (i = 0; i < rv.Length; i++)
            {
                if (VertexVisited[rv[i]]) break;
            }
            r.AdjacentFaces[i] = l.Index;
        }

        /// <summary>
        /// Find the (dimension+1) initial points and create the simplexes.
        /// Creates the initial simplex of n+1 vertices by using points from the bounding box.
        /// Special care is taken to ensure that the vertices chosen do not result in a degenerate shape
        /// where vertices are collinear (co-planar, etc). This would technically be resolved when additional
        /// vertices are checked in the main loop, but: 1) a degenerate simplex would not eliminate any other
        /// vertices (thus no savings there), 2) the creation of the face normal is prone to error.
        /// </summary>
        private void CreateInitialSimplex()
        {
            #region Get the best points

            var boundingBoxPoints = FindBoundingBoxPoints(Vertices);
            var vertex0 = boundingBoxPoints[0].First(); // these are min and max vertices along
            var vertex1 = boundingBoxPoints[0].Last(); // the dimension that had the fewest points
            boundingBoxPoints[0].RemoveAt(0);
            boundingBoxPoints[0].RemoveAt(boundingBoxPoints[0].Count - 1);
            var initialPoints = new List<int> { vertex0, vertex1 };
            VertexVisited[vertex0] = VertexVisited[vertex1] = true;
            CurrentVertex = vertex0; UpdateCenter();
            CurrentVertex = vertex1; UpdateCenter();
            var edgeUnitVectors = new List<double[]> { makeUnitVector(vertex0, vertex1) };
            var numberLeft = boundingBoxPoints.Sum(bb => bb.Count);
            var dimensionIndex = 0;
            while (initialPoints.Count < Dimension + 1 && numberLeft > 0)
            {
                dimensionIndex++;
                if (dimensionIndex == Dimension) dimensionIndex = 0;
                var bestNewIndex = -1;
                var lowestDotProduct = 1.0;
                double[] bestUnitVector = { };
                for (var i = boundingBoxPoints[dimensionIndex].Count - 1; i >= 0; i--)
                {
                    var vIndex = boundingBoxPoints[dimensionIndex][i];
                    if (initialPoints.Contains(vIndex)) boundingBoxPoints[dimensionIndex].RemoveAt(i);
                    else
                    {
                        var newUnitVector = makeUnitVector(vertex0, vIndex);
                        var maxDotProduct = calcMaxDotProduct(edgeUnitVectors, newUnitVector);
                        if (lowestDotProduct > maxDotProduct)
                        {
                            lowestDotProduct = maxDotProduct;
                            bestNewIndex = vIndex;
                            bestUnitVector = newUnitVector;
                        }
                    }
                }
                numberLeft = boundingBoxPoints.Sum(bb => bb.Count);
                if (lowestDotProduct >= Constants.MaxDotProductInSimplex) continue;
                boundingBoxPoints[dimensionIndex].Remove(bestNewIndex);
                edgeUnitVectors.Add(bestUnitVector);
                initialPoints.Add(bestNewIndex);
                // Mark the vertex so that it's not included in any beyond set.
                VertexVisited[bestNewIndex] = true;
                CurrentVertex = bestNewIndex;
                // update center must be called before adding the vertex.
                UpdateCenter();
            }
            var index = -1;
            while (initialPoints.Count < Dimension + 1 && ++index < NumberOfVertices)
            {
                if (VertexVisited[index]) continue;
                var newUnitVector = makeUnitVector(vertex0, index);
                if (calcMaxDotProduct(edgeUnitVectors, newUnitVector) >= Constants.MaxDotProductInSimplex)
                    continue;
                edgeUnitVectors.Add(newUnitVector);
                initialPoints.Add(index);
            }
            if (initialPoints.Count < Dimension + 1)
                throw new ArgumentException("The input data is degenerate. It appears to exist in " + Dimension +
                    " dimensions, but it is a " + (Dimension - 1) + " dimensional set (i.e. the point of collinear,"
                    + " coplanar, or co-hyperplanar.)");

            #endregion

            #region Create the first faces from (dimension + 1) vertices.

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
                for (var j = i + 1; j < Dimension + 1; j++) UpdateAdjacency(FacePool[faces[i]], FacePool[faces[j]]);

            #endregion

            #region Init the vertex beyond buffers.

            foreach (var faceIndex in faces)
            {
                var face = FacePool[faceIndex];
                FindBeyondVertices(face);
                if (face.VerticesBeyond.Count == 0) ConvexFaces.Add(face.Index); // The face is on the hull
                else UnprocessedFaces.Add(face);
            }

            #endregion

            // Set all vertices to false (unvisited).
            foreach (var vertex in initialPoints) VertexVisited[vertex] = false;
        }


        /// <summary>
        /// Used in the "initialization" code.
        /// </summary>
        /// <param name="face">The face.</param>
        private void FindBeyondVertices(ConvexFaceInternal face)
        {
            var beyondVertices = face.VerticesBeyond;
            MaxDistance = double.NegativeInfinity;
            FurthestVertex = 0;
            for (var i = 0; i < NumberOfVertices; i++)
            {
                if (VertexVisited[i]) continue;
                IsBeyond(face, beyondVertices, i);
            }

            face.FurthestVertex = FurthestVertex;
        }

        /// <summary>
        /// Calculates the maximum dot product.
        /// </summary>
        /// <param name="edgeUnitVectors">The edge unit vectors.</param>
        /// <param name="newUnitVector">The new unit vector.</param>
        /// <returns>System.Double.</returns>
        private double calcMaxDotProduct(List<double[]> edgeUnitVectors, double[] newUnitVector)
        {
            var maxDotProduct = 0.0;
            foreach (double[] edgeUnitVector in edgeUnitVectors)
            {
                var dot = calcDotProduct(edgeUnitVector, newUnitVector);
                if (maxDotProduct < dot) maxDotProduct = dot;
            }
            return maxDotProduct;
        }

        private double calcDotProduct(double[] a, double[] b)
        {
            var dot = 0.0;
            for (var dimIndex = 0; dimIndex < Dimension; dimIndex++)
                dot += a[dimIndex] * b[dimIndex];
            return Math.Abs(dot);
        }

        /// <summary>
        /// Makes the unit vector.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <returns>System.Double[].</returns>
        private double[] makeUnitVector(int v1, int v2)
        {
            var vector = new double[Dimension];
            for (var i = 0; i < Dimension; i++)
                vector[i] = GetCoordinate(v1, i) - GetCoordinate(v2, i);
            var magnitude = 0.0;
            for (var i = 0; i < Dimension; i++)
                magnitude += vector[i] * vector[i];
            magnitude = Math.Sqrt(magnitude);
            for (var i = 0; i < Dimension; i++)
                vector[i] /= magnitude;
            return vector;
        }

        /// <summary>
        /// Finds the bounding box points.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns>List&lt;List&lt;System.Int32&gt;&gt;.</returns>
        private List<List<int>> FindBoundingBoxPoints(IVertex[] vertices)
        {
            var extremes = new List<List<int>>();
            var fewestExtremes = int.MaxValue;
            for (var i = 0; i < Dimension; i++)
            {
                var minIndices = new List<int>();
                var maxIndices = new List<int>();
                double min = double.MaxValue, max = double.MinValue;
                for (var j = 0; j < NumberOfVertices; j++)
                {
                    var v = GetCoordinate(j, i);
                    var difference = min - v;
                    if (difference >= PlaneDistanceTolerance)
                    {
                        // you found a better solution than before, clear out the list and store new value
                        min = v;
                        minIndices.Clear();
                        minIndices.Add(j);
                    }
                    else if (difference > 0)
                    {
                        // you found a solution slightly better than before, clear out those that are no longer on the list and store new value
                        min = v;
                        minIndices.RemoveAll(index => min - GetCoordinate(index, i) > PlaneDistanceTolerance);
                        minIndices.Add(j);
                    }
                    else if (difference > -PlaneDistanceTolerance)
                    {
                        //same or almost as good as current limit, so store it
                        minIndices.Add(j);
                    }
                    difference = v - max;
                    if (difference >= PlaneDistanceTolerance)
                    {
                        // you found a better solution than before, clear out the list and store new value
                        max = v;
                        maxIndices.Clear();
                        maxIndices.Add(j);
                    }
                    else if (difference > 0)
                    {
                        // you found a solution slightly better than before, clear out those that are no longer on the list and store new value
                        max = v;
                        maxIndices.RemoveAll(index => min - GetCoordinate(index, i) > PlaneDistanceTolerance);
                        maxIndices.Add(j);
                    }
                    else if (difference > -PlaneDistanceTolerance)
                    {
                        //same or almost as good as current limit, so store it
                        maxIndices.Add(j);
                    }
                }
                // in the CreateInitialSimplex method that calls this we want the most restrictive bounds first, so we do a
                // simple sorting - in a way - we put the dimensions with the fewest points first.
                // this is why the following if-else takes care of.
                minIndices.AddRange(maxIndices);
                if (minIndices.Count <= fewestExtremes)
                {
                    extremes.Insert(0, minIndices);
                    fewestExtremes = minIndices.Count;
                }
                else extremes.Add(minIndices);
            }
            return extremes;
        }

        #region Fields

        /// <summary>
        /// Corresponds to the dimension of the data.
        /// When the "lifted" hull is computed, Dimension is automatically incremented by one.
        /// </summary>
        internal readonly int Dimension;

        /// <summary>
        /// Are we on a paraboloid?
        /// </summary>
        private readonly bool IsLifted;

        /// <summary>
        /// Explained in ConvexHullComputationConfig.
        /// </summary>
        private readonly double PlaneDistanceTolerance;

        /*
         * Representation of the input vertices.
         * 
         * - In the algorithm, a vertex is represented by its index in the Vertices array.
         *   This makes the algorithm a lot faster (up to 30%) than using object reference everywhere.
         * - Positions are stored as a single array of values. Coordinates for vertex with index i
         *   are stored at indices <i * Dimension, (i + 1) * Dimension)
         * - VertexMarks are used by the algorithm to help identify a set of vertices that is "above" (or "beyond") 
         *   a specific face.
         */
        /// <summary>
        /// The vertices
        /// </summary>
        private readonly IVertex[] Vertices;
        /// <summary>
        /// The positions
        /// </summary>
        private double[] Positions;
        /// <summary>
        /// The vertex marks
        /// </summary>
        private readonly bool[] VertexVisited;

        private readonly int NumberOfVertices;

        /*
         * The triangulation faces are represented in a single pool for objects that are being reused.
         * This allows for represent the faces as integers and significantly speeds up many computations.
         * - AffectedFaceFlags are used to mark affected faces/
         */
        /// <summary>
        /// The face pool
        /// </summary>
        internal ConvexFaceInternal[] FacePool;
        /// <summary>
        /// The affected face flags
        /// </summary>
        internal bool[] AffectedFaceFlags;

        /// <summary>
        /// Used to track the size of the current hull in the Update/RollbackCenter functions.
        /// </summary>
        private int ConvexHullSize;

        /// <summary>
        /// A list of faces that that are not a part of the final convex hull and still need to be processed.
        /// </summary>
        private readonly FaceList UnprocessedFaces;

        /// <summary>
        /// A list of faces that form the convex hull.
        /// </summary>
        private readonly IndexBuffer ConvexFaces;

        /// <summary>
        /// The vertex that is currently being processed.
        /// </summary>
        private int CurrentVertex;

        /// <summary>
        /// A helper variable to determine the furthest vertex for a particular convex face.
        /// </summary>
        private double MaxDistance;

        /// <summary>
        /// A helper variable to help determine the index of the vertex that is furthest from the face that is currently being
        /// processed.
        /// </summary>
        private int FurthestVertex;

        /// <summary>
        /// The centroid of the currently computed hull.
        /// </summary>
        private readonly double[] Center;

        /*
         * Helper arrays to store faces for adjacency update.
         * This is just to prevent unnecessary allocations.
         */
        /// <summary>
        /// The update buffer
        /// </summary>
        private readonly int[] UpdateBuffer;
        /// <summary>
        /// The update indices
        /// </summary>
        private readonly int[] UpdateIndices;

        /// <summary>
        /// Used to determine which faces need to be updated at each step of the algorithm.
        /// </summary>
        private readonly IndexBuffer TraverseStack;

        /// <summary>
        /// Used for VerticesBeyond for faces that are on the convex hull.
        /// </summary>
        private readonly IndexBuffer EmptyBuffer;

        /// <summary>
        /// Used to determine which vertices are "above" (or "beyond") a face
        /// </summary>
        private IndexBuffer BeyondBuffer;

        /// <summary>
        /// Stores faces that are visible from the current vertex.
        /// </summary>
        private readonly IndexBuffer AffectedFaceBuffer;

        /// <summary>
        /// Stores faces that form a "cone" created by adding new vertex.
        /// </summary>
        private readonly SimpleList<DeferredFace> ConeFaceBuffer;

        /// <summary>
        /// Stores a list of "singular" (or "generate", "planar", etc.) vertices that cannot be part of the hull.
        /// </summary>
        private readonly HashSet<int> SingularVertices;

        /// <summary>
        /// The connector table helps to determine the adjacency of convex faces.
        /// Hashing is used instead of pairwise comparison. This significantly speeds up the computations,
        /// especially for higher dimensions.
        /// </summary>
        private readonly ConnectorList[] ConnectorTable;

        /// <summary>
        /// The connector table size
        /// </summary>
        private const int ConnectorTableSize = 2017;

        /// <summary>
        /// Manages the memory allocations and storage of unused objects.
        /// Saves the garbage collector a lot of work.
        /// </summary>
        private readonly ObjectManager ObjectManager;

        /// <summary>
        /// Helper class for handling math related stuff.
        /// </summary>
        private readonly MathHelper MathHelper;

        #endregion
    }
}