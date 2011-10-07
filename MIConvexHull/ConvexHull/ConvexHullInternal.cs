namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ConvexHullInternal
    {
        class NoEqualSortMaxtoMinInt : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                if (x > y) return -1;
                return 1;
            }
        }
        
        private const double CoeffNumVertices = 0.25;
        private const double CoeffDimensions = 2;
        private const double CoeffOffset = 1250;

        List<VertexWrap> origVertices;
        List<VertexWrap> convexHull;
        internal FibonacciHeap<double, ConvexFaceInternal> convexFaceHeap;
        List<ConvexFaceInternal> convexFaces;

        bool computed = false;
        bool initializing;

        int dimension;
        double[] center;

        #region "Buffers"
        double[] ntX, ntY, ntZ;
        double[] nDRightSide;
        double[,] nDNormalMatrix;

        ConvexFaceInternal[] updateBuffer;
        int[] updateIndices;

        Stack<ConvexFaceInternal> recycledFaceStack;
        List<ConvexFaceInternal> newFaceBuffer;
        List<ConvexFaceInternal> affectedFaceBuffer;
        List<VertexWrap> beyondBuffer;
        #endregion

        /// <summary>
        /// Initialize buffers and lists.
        /// </summary>
        void Initialize()
        {
            convexHull = new List<VertexWrap>();
            convexFaceHeap = new FibonacciHeap<double, ConvexFaceInternal>(HeapDirection.Decreasing, (x, y) => (x < y) ? -1 : 1);
            convexFaces = new List<ConvexFaceInternal>();
            center = new double[dimension];

            ntX = new double[dimension];
            ntY = new double[dimension];
            ntZ = new double[dimension];
            outDir = new double[dimension];
            updateBuffer = new ConvexFaceInternal[dimension];
            updateIndices = new int[dimension];
            recycledFaceStack = new Stack<ConvexFaceInternal>();
            newFaceBuffer = new List<ConvexFaceInternal>();
            affectedFaceBuffer = new List<ConvexFaceInternal>();
            beyondBuffer = new List<VertexWrap>();

            if (dimension > 4)
            {
                nDRightSide = new double[dimension];
                for (var i = 0; i < dimension; i++) nDRightSide[i] = 1.0;
                nDNormalMatrix = new double[dimension, dimension];
            }
        }

        /// <summary>
        /// Check the dimensionality of the input data.
        /// </summary>
        /// <param name="vertices"></param>
        private void DetermineDimension(IList<VertexWrap> vertices)
        {
            var r = new Random();
            var VCount = vertices.Count;
            var dimensions = new List<int>();
            for (var i = 0; i < 10; i++)
                dimensions.Add(vertices[r.Next(VCount)].Vertex.Position.Length);
            dimension = dimensions.Min();
            if (dimension != dimensions.Max())
            {
                throw new ArgumentException("Invalid input data (non-uniform dimension).");
            }
        }

        #region Ternary Counter functions

        private bool IncrementTernaryPosition(int[] ternaryPosition, int position = 0)
        {
            if (position == ternaryPosition.GetLength(0)) return false;
            ternaryPosition[position]++;
            if (ternaryPosition[position] == 2)
            {
                ternaryPosition[position] = -1;
                return IncrementTernaryPosition(ternaryPosition, ++position);
            }
            return true;
        }

        private int FindIndex(int[] ternaryPosition, int midPoint)
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
        #endregion

        #region Make functions

        ///// <summary>
        ///// Determine if a tetrahedron is zero volume. This is used to detect degeneracy
        ///// in the InitiateFaceDatabase function.
        ///// </summary>
        ///// <param name="vertices"></param>
        ///// <returns></returns>
        //private bool IsZeroVolume(IList<VertexWrap> vertices)
        //{
        //    Matrix m = new Matrix(dimension);

        //    var v0 = vertices[0].Vertex.Position;
        //    for (int i = 1; i <= dimension; i++)
        //    {
        //        m.SetColumn(i - 1, MathEx.SubtractFast(vertices[i].Vertex.Position, v0, dimension));
        //    }

        //    double det = m.Determinant();
        //    return double.IsNaN(det) || System.Math.Abs(det) < 1e-8;
        //}

        /// <summary>
        /// Create the first faces from (dimension + 1) vertices.
        /// </summary>
        /// <returns></returns>
        void InitiateFaceDatabase()
        {
            for (var i = 0; i < dimension + 1; i++)
            {
                var vertices = convexHull.Where((_, j) => i != j).ToArray();
                var newFace = new ConvexFaceInternal(dimension);
                newFace.Vertices = vertices;
                CalculateNormal(newFace);
                newFace.FibCell = convexFaceHeap.Enqueue(0.0, newFace);
            }

            // update the adjacency (check all pairs of faces)
            var faces = convexFaceHeap.ToArray();
            for (var i = 0; i < dimension; i++)
            {
                for (var j = i + 1; j < dimension + 1; j++)
                {
                    UpdateAdjacency(faces[i].Value, faces[j].Value);
                }
            }
        }

        double[] outDir;

        /// <summary>
        /// Create a face from given vertices and calculates the normal. outDir is "buffered" to prevent
        /// unneeded allocations on the heap.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        private void CalculateNormal(ConvexFaceInternal face)
        {
            for (int j = 0; j < dimension; j++)
            {
                outDir[j] = 0;
            }

            var vertices = face.Vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                var pos = vertices[i].PositionData;
                for (int j = 0; j < dimension; j++)
                {
                    outDir[j] += pos[j];
                }
            }

            double f = 1.0 / (double)dimension;
            for (int j = 0; j < dimension; j++)
            {
                outDir[j] = outDir[j] * f - center[j];
            }

            var normal = face.Normal;
            FindNormalVector(vertices, normal);

            double dot = 0;
            for (int i = 0; i < dimension; i++) dot += normal[i] * outDir[i];

            if (dot < 0)
            {
                for (int i = 0; i < dimension; i++) normal[i] = -normal[i];
                if (dimension == 3) vertices.Reverse();
            }
        }

        #endregion

        #region Find, Get and Update functions

        /// <summary>
        /// Find all faces that can "see" the given vertex.
        /// </summary>
        /// <param name="currentVertex"></param>
        /// <returns></returns>
        private List<ConvexFaceInternal> FindFacesBeneathInitialVertices(VertexWrap currentVertex)
        {
            var facesUnder = new List<ConvexFaceInternal>();

            foreach (var face in convexFaceHeap.GetValues())
            {
                if (IsVertexOverFace(currentVertex, face) >= 0)
                    facesUnder.Add(face);
            }
            return facesUnder;
        }
        
        /// <summary>
        /// Check if the vertex is "visible" from the face.
        /// The vertex is "over face" if the return value is >= 0.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="f"></param>
        /// <param name="dotP"></param>
        /// <param name="n"></param>
        /// <returns>The vertex is "over face" if the return value is >= 0</returns>
        private double IsVertexOverFace(VertexWrap v, ConvexFaceInternal f)
        {
            double[] n = f.Normal;
            double[] l = v.PositionData;
            double[] r = f.Vertices[0].PositionData;
            double acc = 0;

            // subtract and dot (n . (l - r))
            for (int i = 0; i < dimension; i++)
            {
                acc += n[i] * (l[i] - r[i]);
            }

            return acc;
        }

        void FindAffectedFaces(ConvexFaceInternal currentFace, VertexWrap currentVertex)
        {
            affectedFaceBuffer.Clear();
            affectedFaceBuffer.Add(currentFace);
            TraverseAffectedFaces(currentFace, currentVertex);
        }

        /// <summary>
        /// Recursively traverse all the relevant faces.
        /// </summary>
        void TraverseAffectedFaces(ConvexFaceInternal currentFace, VertexWrap currentVertex)
        {
            currentFace.Tag = 1;

            for (int i = 0; i < dimension; i++)
            {
                var adjFace = currentFace.AdjacentFaces[i];

                if (adjFace != null && adjFace.Tag == 0 && IsVertexOverFace(currentVertex, adjFace) >= 0)
                {
                    affectedFaceBuffer.Add(adjFace);
                    TraverseAffectedFaces(adjFace, currentVertex);
                }
            }
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
            for (i = 0; i < dimension; i++) lv[i].Marked = false;

            // mark all vertices on the 2nd face
            for (i = 0; i < dimension; i++) rv[i].Marked = true;

            // find the 1st false index
            for (i = 0; i < dimension; i++) if (!lv[i].Marked) break;

            // no vertex was marked
            if (i == dimension) return;

            // check if only 1 vertex wasn't marked
            for (int j = i + 1; j < dimension; j++) if (!lv[j].Marked) return;

            // if we are here, the two faces share an edge
            l.AdjacentFaces[i] = r;
            
            // update the adj. face on the other face - find the vertex that remains marked
            for (i = 0; i < dimension; i++) lv[i].Marked = false;
            for (i = 0; i < dimension; i++)
            {
                if (rv[i].Marked) break;
            }
            r.AdjacentFaces[i] = l;
        }

        void UpdateNewFaceAdjacency(ConvexFaceInternal newFace, ConvexFaceInternal otherFace)
        {
            var lv = newFace.Vertices;
            var rv = otherFace.Vertices;
            
            // reset marks
            for (int i = 0; i < dimension; i++)
            {
                rv[i].Marked = false;
            }

            // Mark new face vertices
            for (int i = 0; i < dimension; i++) lv[i].Marked = true;

            newFace.AdjacentFaces[0] = otherFace;

            // find non-marked vertices
            for (int i = 0; i < dimension; i++)
            {
                if (!rv[i].Marked)
                {
                    otherFace.AdjacentFaces[i] = newFace;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Recycle face for future use.
        /// </summary>
        void RecycleFace(ConvexFaceInternal face)
        {
            for (int i = 0; i < dimension; i++)
            {
                face.AdjacentFaces[i] = null;
            }
        }
        
        /// <summary>
        /// Remove the faces "covered" by the current vertex and add the newly created ones.
        /// </summary>
        /// <param name="oldFaces"></param>
        /// <param name="currentVertex"></param>
        private void UpdateFaces(List<ConvexFaceInternal> oldFaces, VertexWrap currentVertex)
        {
            var newFaces = newFaceBuffer;
            newFaces.Clear();

            for (int fIndex = 0; fIndex < oldFaces.Count; fIndex++)
            {
                var oldFace = oldFaces[fIndex];

                // Find the faces that need to be updated
                int updateCount = 0;
                for (int i = 0; i < dimension; i++)
                {
                    var af = oldFace.AdjacentFaces[i];
                    if (af != null && af.Tag == 0) // Tag == 0 when oldFaces does not contain af
                    {
                        updateBuffer[updateCount] = af;
                        updateIndices[updateCount] = i;
                        ++updateCount;
                    }
                }

                // Recycle the face for future use
                if (updateCount == 0)
                {
                    // Push the face to the bottom of the heap
                    convexFaceHeap.ChangeKey(oldFace.FibCell, -1.0);
                    RecycleFace(oldFace);
                    recycledFaceStack.Push(oldFace);
                }

                for (int i = 0; i < updateCount; i++)
                {
                    var adjacentFace = updateBuffer[i];
                    var forbidden = updateIndices[i]; // Index of the face that corresponds to this adjacent face

                    ConvexFaceInternal newFace;

                    // Recycle the oldFace
                    if (i == updateCount - 1)
                    {
                        RecycleFace(oldFace);
                        newFace = oldFace;
                        var vertices = newFace.Vertices;
                        for (int j = forbidden; j > 0; j--)
                        {
                            vertices[j] = vertices[j - 1];
                        }
                        vertices[0] = currentVertex;
                    }
                    else // Pop a face from the recycled stack or a new one
                    {
                        newFace = recycledFaceStack.Count != 0 ? recycledFaceStack.Pop() : new ConvexFaceInternal(dimension);
                        var vertices = newFace.Vertices;
                        vertices[0] = currentVertex;
                        for (int j = 0, c = 1; j < dimension; j++)
                        {
                            if (j != forbidden) vertices[c++] = oldFace.Vertices[j];
                        }
                    }

                    CalculateNormal(newFace);
                    UpdateNewFaceAdjacency(newFace, adjacentFace);

                    FindBeyondVertices(newFace, adjacentFace.VerticesBeyond, oldFace.VerticesBeyond, currentVertex);

                    newFaces.Add(newFace);

                    // This face will definitely lie on the hull
                    if (newFace.VerticesBeyond.Count == 0 && !initializing)
                    {
                        convexFaces.Add(newFace);
                        if (newFace.FibCell != null) convexFaceHeap.Delete(newFace.FibCell);
                        newFace.FibCell = null;
                    }
                    else // Update the face heap
                    {
                        if (newFace.FibCell != null) convexFaceHeap.ChangeKey(newFace.FibCell, newFace.MinVertexKey);
                        else newFace.FibCell = convexFaceHeap.Enqueue(newFace.MinVertexKey, newFace);
                    }
                }
            }

            // If no face was added, quit ...
            if (newFaces.Count == 0) return;

            // Check all pairs of faces and update their adjacency
            for (var i = 0; i < newFaces.Count - 1; i++)
            {
                for (var j = i + 1; j < newFaces.Count; j++)
                {
                    UpdateAdjacency(newFaces[i], newFaces[j]);

                    // No need to continue if all faces were filled
                    int k;
                    for (k = 0; k < dimension; k++)
                    {
                        if (newFaces[i].AdjacentFaces[k] == null) break;
                    }
                    if (k == dimension) break;
                }
            }
        }
        
        void SubtractFast(double[] x, double[] y, double[] target)
        {
            for (int i = 0; i < dimension; i++)
            {
                target[i] = x[i] - y[i];
            }
        }
           
        void FindNormalVector4D(VertexWrap[] vertices, double[] normal)
        {
            SubtractFast(vertices[1].PositionData, vertices[0].PositionData, ntX);
            SubtractFast(vertices[2].PositionData, vertices[0].PositionData, ntY);
            SubtractFast(vertices[3].PositionData, vertices[0].PositionData, ntZ);

            var x = ntX;
            var y = ntY;
            var z = ntZ;

            // This was generated using Mathematica
            var nx = x[3] * (y[2] * z[1] - y[1] * z[2])
                   + x[2] * (y[1] * z[3] - y[3] * z[1])
                   + x[1] * (y[3] * z[2] - y[2] * z[3]);
            var ny = x[3] * (y[0] * z[2] - y[2] * z[0])
                   + x[2] * (y[3] * z[0] - y[0] * z[3])
                   + x[0] * (y[2] * z[3] - y[3] * z[2]);
            var nz = x[3] * (y[1] * z[0] - y[0] * z[1])
                   + x[1] * (y[0] * z[3] - y[3] * z[0])
                   + x[0] * (y[3] * z[1] - y[1] * z[3]);
            var nw = x[2] * (y[0] * z[1] - y[1] * z[0])
                   + x[1] * (y[2] * z[0] - y[0] * z[2])
                   + x[0] * (y[1] * z[2] - y[2] * z[1]);

            double norm = System.Math.Sqrt(nx * nx + ny * ny + nz * nz + nw * nw);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
            normal[3] = f * nw;
        }

        void FindNormalVector3D(VertexWrap[] vertices, double[] normal)
        {
            SubtractFast(vertices[1].PositionData, vertices[0].PositionData, ntX);
            SubtractFast(vertices[2].PositionData, vertices[0].PositionData, ntY);

            var x = ntX;
            var y = ntY;

            var nx = x[1] * y[2] - x[2] * y[1];
            var ny = x[2] * y[0] - x[0] * y[2];
            var nz = x[0] * y[1] - x[1] * y[0];

            double norm = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
        }

        private void FindNormalVector(VertexWrap[] vertices, double[] normalData)
        {            
            if (dimension == 3)
            {
                FindNormalVector3D(vertices, normalData);
            }
            else if (dimension == 4)
            {
                FindNormalVector4D(vertices, normalData);
            }
            else
            {
                double[] normal;
                var b = nDRightSide; 
                var A = nDNormalMatrix;
                for (var i = 0; i < dimension; i++)
                    StarMath.SetRow(i, A, vertices[i].Vertex.Position);
                normal = StarMath.solve(A, b);
                StarMath.normalize(normal, dimension);
                for (int i = 0; i < dimension; i++) normalData[i] = normal[i];
            }

            if (double.IsNaN(normalData[0]))
            {
                throw new InvalidOperationException("The input data cannot be triangulated.\nProbably, the reason is that the input data is too regular in some regions. Introducing some noise to the data might solve this problem.");
            }
        }

        private void IsBeyond(ConvexFaceInternal face, List<VertexWrap> beyondVertices, ref double min, ref VertexWrap minV, VertexWrap v)
        {
            double dotP = IsVertexOverFace(v, face);
            if (dotP >= 0)
            {
                if (dotP > min)
                {
                    min = dotP;
                    minV = v;
                }
                beyondVertices.Add(v);
            }
        }

        /// <summary>
        /// Used in the "initialization" code.
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face, List<VertexWrap> vertices)
        {
            var beyondVertices = face.VerticesBeyond;

            double min = double.NegativeInfinity;
            VertexWrap minV = null;

            int count = vertices.Count;
            for (int i = 0; i < count; i++)
            {
                var v = vertices[i];
                IsBeyond(face, beyondVertices, ref min, ref minV, v);
            }
            face.MinVertex = minV;
            face.MinVertexKey = beyondVertices.Count > 0 ? min : -1.0;
        }

        /// <summary>
        /// Used by update faces.
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face, List<VertexWrap> beyond, List<VertexWrap> beyond1, VertexWrap current)
        {
            var beyondVertices = beyondBuffer; //face.VerticesBeyond;

            double min = double.NegativeInfinity;
            VertexWrap minV = null;

            for (int i = 0; i < beyond1.Count; i++) beyond1[i].Marked = true;
            current.Marked = false;
            int count = beyond.Count;
            for (int i = 0; i < count; i++)
            {
                var v = beyond[i];
                if (v == current)
                {
                    continue;
                }
                v.Marked = false;
                IsBeyond(face, beyondVertices, ref min, ref minV, v);
            }

            count = beyond1.Count;
            for (int i = 0; i < count; i++)
            {
                var v = beyond1[i];
                if (v.Marked) IsBeyond(face, beyondVertices, ref min, ref minV, v);
            }

            face.MinVertex = minV;
            face.MinVertexKey = beyondVertices.Count > 0 ? min : -1.0;

            // Pull the old switch a roo
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            beyondBuffer = temp;
        }
        


        private void UpdateCenter(VertexWrap currentVertex)
        {
            for (int i = 0; i < dimension; i++) center[i] *= (convexHull.Count - 1);
            double f = 1.0 / convexHull.Count;
            for (int i = 0; i < dimension; i++) center[i] = f * (center[i] + currentVertex.PositionData[i]);
        }

        #endregion

        class LexComparer : IComparer<VertexWrap>
        {
            int dim;

            public int Compare(VertexWrap vx, VertexWrap vy)
            {
                var x = vx.Vertex.Position;
                var y = vy.Vertex.Position;
                for (int i = 0; i < dim; i++)
                {
                    if (x[i] < y[i]) return -1;
                    if (x[i] > y[i]) return 1;
                }

                return 0;
            }

            public LexComparer(int dim)
            {
                this.dim = dim;
            }
        }

        #region 2D Convex Hull code
        private void Find2D()
        {
            var origVNum = origVertices.Count;

            #region Step 1 : Define Convex Octogon

            /* The first step is to quickly identify the three to eight vertices based on the
             * Akl-Toussaint heuristic. */
            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;
            var maxSum = double.NegativeInfinity;
            var maxDiff = double.NegativeInfinity;
            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var minSum = double.PositiveInfinity;
            var minDiff = double.PositiveInfinity;

            /* the array of extreme is comprised of: 0.minX, 1. minSum, 2. minY, 3. maxDiff, 4. MaxX, 5. MaxSum, 6. MaxY, 7. MinDiff. */
            var extremeVertices = new VertexWrap[8];
            //  int[] extremeVertexIndices = new int[8]; I thought that this might speed things up. That is, to use this to RemoveAt
            // as oppoaws to the Remove in line 91, which I thought might be slow. Turns out I was wrong - plus code is more succinct
            // way.
            for (var i = 0; i < origVNum; i++)
            {
                var n = origVertices[i];
                if (n.PositionData[0] < minX)
                {
                    extremeVertices[0] = n;
                    minX = n.PositionData[0];
                }
                if ((n.PositionData[0] + n.PositionData[1]) < minSum)
                {
                    extremeVertices[1] = n;
                    minSum = n.PositionData[0] + n.PositionData[1];
                }
                if (n.PositionData[1] < minY)
                {
                    extremeVertices[2] = n;
                    minY = n.PositionData[1];
                }
                if ((n.PositionData[0] - n.PositionData[1]) > maxDiff)
                {
                    extremeVertices[3] = n;
                    maxDiff = n.PositionData[0] - n.PositionData[1];
                }
                if (n.PositionData[0] > maxX)
                {
                    extremeVertices[4] = n;
                    maxX = n.PositionData[0];
                }
                if ((n.PositionData[0] + n.PositionData[1]) > maxSum)
                {
                    extremeVertices[5] = n;
                    maxSum = n.PositionData[0] + n.PositionData[1];
                }
                if (n.PositionData[1] > maxY)
                {
                    extremeVertices[6] = n;
                    maxY = n.PositionData[1];
                }
                if ((n.PositionData[0] - n.PositionData[1]) >= minDiff) continue;
                extremeVertices[7] = n;
                minDiff = n.PositionData[0] - n.PositionData[1];
            }

            /* convexHullCCW is the result of this function. It is a list of 
             * vertices found in the original vertices and ordered to make a
             * counter-clockwise loop beginning with the leftmost (minimum
             * value of X) IVertexConvHull. */
            var convexHullCCW = new List<VertexWrap>();
            for (var i = 0; i < 8; i++)
                if (!convexHullCCW.Contains(extremeVertices[i]))
                {
                    convexHullCCW.Add(extremeVertices[i]);
                    origVertices.Remove(extremeVertices[i]);
                }

            #endregion

            /* the following limits are used extensively in for-loop below. In order to reduce the arithmetic calls and
             * steamline the code, these are established. */
            origVNum = origVertices.Count;
            var cvxVNum = convexHullCCW.Count;
            var last = cvxVNum - 1;

            #region Step 2 : Find Signed-Distance to each convex edge

            /* Of the 3 to 8 vertices identified in the convex hull, we now define a matrix called edgeUnitVectors, 
             * which includes the unit vectors of the edges that connect the vertices in a counter-clockwise loop. 
             * The first column corresponds to the X-value,and  the second column to the Y-value. Calculating this 
             * should not take long since there are only 3 to 8 members currently in hull, and it will save time 
             * comparing to all the result vertices. */
            var edgeUnitVectors = new double[cvxVNum, 2];
            double magnitude;
            for (var i = 0; i < last; i++)
            {
                edgeUnitVectors[i, 0] = (convexHullCCW[i + 1].PositionData[0] - convexHullCCW[i].PositionData[0]);
                edgeUnitVectors[i, 1] = (convexHullCCW[i + 1].PositionData[1] - convexHullCCW[i].PositionData[1]);
                magnitude = Math.Sqrt(edgeUnitVectors[i, 0] * edgeUnitVectors[i, 0] +
                                      edgeUnitVectors[i, 1] * edgeUnitVectors[i, 1]);
                edgeUnitVectors[i, 0] /= magnitude;
                edgeUnitVectors[i, 1] /= magnitude;
            }
            edgeUnitVectors[last, 0] = convexHullCCW[0].PositionData[0] - convexHullCCW[last].PositionData[0];
            edgeUnitVectors[last, 1] = convexHullCCW[0].PositionData[1] - convexHullCCW[last].PositionData[1];
            magnitude = Math.Sqrt(edgeUnitVectors[last, 0] * edgeUnitVectors[last, 0] +
                                  edgeUnitVectors[last, 1] * edgeUnitVectors[last, 1]);
            edgeUnitVectors[last, 0] /= magnitude;
            edgeUnitVectors[last, 1] /= magnitude;

            /* Originally, I was storing all the distances from the vertices to the convex hull points
             * in a big 3D matrix. This is not necessary and the storage may be difficult to handle for large
             * sets. However, I have kept these lines of code here because they could be useful in establishing
             * the voronoi sets. */
            //var signedDists = new double[2, origVNum, cvxVNum];

            /* An array of sorted dictionaries! As we find new candidate convex points, we store them here. The second
             * part of the tuple (Item2 is a double) is the "positionAlong" - this is used to order the nodes that
             * are found for a particular side (More on this in 23 lines). */
            var hullCands = new SortedList<double, VertexWrap>[cvxVNum];
            /* initialize the 3 to 8 Lists s.t. members can be added below. */
            for (var j = 0; j < cvxVNum; j++) hullCands[j] = new SortedList<double, VertexWrap>();

            /* Now a big loop. For each of the original vertices, check them with the 3 to 8 edges to see if they 
             * are inside or out. If they are out, add them to the proper row of the hullCands array. */
            for (var i = 0; i < origVNum; i++)
            {
                for (var j = 0; j < cvxVNum; j++)
                {
                    var b = new[]
                                {
                                    origVertices[i].PositionData[0] - convexHullCCW[j].PositionData[0],
                                    origVertices[i].PositionData[1] - convexHullCCW[j].PositionData[1]
                                };
                    //signedDists[0, k, i] = signedDistance(convexVectInfo[i, 0], convexVectInfo[i, 1], bX, bY, convexVectInfo[i, 2]);
                    //signedDists[1, k, i] = positionAlong(convexVectInfo[i, 0], convexVectInfo[i, 1], bX, bY, convexVectInfo[i, 2]);
                    //if (signedDists[0, k, i] <= 0)
                    /* Again, these lines are commented because the signedDists has been removed. This data may be useful in 
                     * other applications though. In the condition below, any signed distance that is negative is outside of the
                     * original polygon. It is only possible for the IVertexConvHull to be outside one of the 3 to 8 edges, so once we
                     * add it, we break out of the inner loop (gotta save time where we can!). */
                    double val = edgeUnitVectors[j, 0] * b[1] - edgeUnitVectors[j, 1] * b[0]; // Cross2D from StarMath
                    if (val > 0) continue;
                    val = edgeUnitVectors[j, 0] * b[0] + edgeUnitVectors[j, 1] * b[1]; // GetRow + dot
                    hullCands[j].Add(val, origVertices[i]);
                    break;
                }
            }

            #endregion

            #region Step 3: now check the remaining hull candidates

            /* Now it's time to go through our array of sorted lists of tuples. We search backwards through
             * the current convex hull points s.t. any additions will not confuse our for-loop indexers. */
            for (var j = cvxVNum; j > 0; j--)
            {
                if (hullCands[j - 1].Count == 1)
                    /* If there is one and only one candidate, it must be in the convex hull. Add it now. */
                    convexHullCCW.InsertRange(j, hullCands[j - 1].Values);
                else if (hullCands[j - 1].Count > 1)
                {
                    /* If there's more than one than...Well, now comes the tricky part. Here is where the
                     * most time is spent for large sets. this is the O(N*logN) part (the previous steps
                     * were all linear). The above octagon trick was to conquer and divide the candidates. */

                    /* a renaming for compactness and clarity */
                    var hc = new List<VertexWrap>(hullCands[j - 1].Values);

                    /* put the known starting IVertexConvHull as the beginning of the list. No need for the "positionAlong"
                     * anymore since the list is now sorted. At any rate, the positionAlong is zero. */
                    hc.Insert(0, convexHullCCW[j - 1]);
                    /* put the ending IVertexConvHull on the end of the list. Need to check if it wraps back around to 
                     * the first in the loop (hence the simple condition). */
                    if (j == cvxVNum) hc.Add(convexHullCCW[0]);
                    else hc.Add(convexHullCCW[j]);

                    /* Now starting from second from end, work backwards looks for places where the angle 
                     * between the vertices in concave (which would produce a negative value of z). */
                    var i = hc.Count - 2;
                    while (i > 0)
                    {
                        double lX = hc[i].PositionData[0] - hc[i - 1].PositionData[0], lY = hc[i].PositionData[1] - hc[i - 1].PositionData[1];
                        double rX = hc[i + 1].PositionData[0] - hc[i].PositionData[0], rY = hc[i + 1].PositionData[1] - hc[i].PositionData[1];
                        double zValue = lX * rY - lY * rX;
                        //var zValue = StarMath.multiplyCross2D(StarMath.subtract(hc[i].PositionData, hc[i - 1].PositionData),
                        //                                      StarMath.subtract(hc[i + 1].PositionData, hc[i].PositionData));
                        if (zValue < 0)
                        {
                            /* remove any vertices that create concave angles. */
                            hc.RemoveAt(i);
                            /* but don't reduce k since we need to check the previous angle again. Well, 
                             * if you're back to the end you do need to reduce k (hence the line below). */
                            if (i == hc.Count - 1) i--;
                        }
                        /* if the angle is convex, then continue toward the start, k-- */
                        else i--;
                    }
                    /* for each of the remaining vertices in hullCands[i-1], add them to the convexHullCCW. 
                     * Here we insert them backwards (k counts down) to simplify the insert operation (k.e.
                     * since all are inserted @ i, the previous inserts are pushed up to i+1, i+2, etc. */
                    for (i = hc.Count - 2; i > 0; i--)
                        convexHullCCW.Insert(j, hc[i]);
                }
            }
            #endregion

            convexHull = convexHullCCW;
        }
        #endregion

        private void AklTouHeuristic()
        {
            var VCount = origVertices.Count;
            origVertices.Sort(new LexComparer(dimension));

            // Step 1 : Define Convex Rhombicuboctahedron

            /* as a heuristic, we limit the number of solutions created in the first loop, by an albeit, 
             * artificial formulation. This is to prevent the process from stagnating in this step in higher 
             * dimensions when the number of solutions on the Akl-Toussaint polygon gets too high (3^dimension).*/
            var maxAklTousNumber = CoeffDimensions * dimension + CoeffOffset;
            maxAklTousNumber = System.Math.Min(maxAklTousNumber, CoeffNumVertices * VCount);
            /* of course, this limit is a moot point if there simply aren't enough points in the original
             * set of vertices. Therefore, it should at least be dimension + 1, the number of vertices in the
             * simplex. */
            maxAklTousNumber = System.Math.Max(maxAklTousNumber, dimension + 1);

            var numExtremes = (int)System.Math.Pow(3, dimension);
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
                var index = FindIndex(ternaryPosition, midPoint);
                if (index == midPoint) continue;
                for (var m = 0; m < VCount; m++)
                {
                    double acc = 0;
                    for (int fi = 0; fi < dimension; fi++)
                    {
                        acc += (double)ternaryPosition[fi] * origVertices[m].PositionData[fi];
                    }

                    var extreme = flip * acc;
                    if (extreme < extremeValues[index]) continue;
                    AklToussaintIndices[index] = m;
                    extremeValues[index] = extreme;
                }
                flip *= -1;
                if (AklToussaintIndices.Distinct().Count() > maxAklTousNumber) break;
            } while (IncrementTernaryPosition(ternaryPosition));
            AklToussaintIndices = AklToussaintIndices.Distinct().ToList();
            AklToussaintIndices.Remove(-1);
            AklToussaintIndices.Sort(new NoEqualSortMaxtoMinInt());

            // Step #2: Define up to 48 faces of the Disdyakis dodecahedron
            for (var i = 0; i < AklToussaintIndices.Count; i++)
            {
                var currentVertex = origVertices[AklToussaintIndices[i]];
                convexHull.Add(currentVertex);
                UpdateCenter(currentVertex);
                if (i == dimension) InitiateFaceDatabase();
                else if (i > dimension)
                {
                    var facesUnderVertex = FindFacesBeneathInitialVertices(currentVertex);
                    // Tag the faces
                    for (int j = 0; j < facesUnderVertex.Count; j++) facesUnderVertex[j].Tag = 1;
                    UpdateFaces(facesUnderVertex, currentVertex);
                    // "Untag" the faces
                    for (int j = 0; j < facesUnderVertex.Count; j++) facesUnderVertex[j].Tag = 0;
                }
                origVertices.RemoveAt(AklToussaintIndices[i]);
            }
        }

        private void FindConvexHull()
        {
            // Find initial approximation of the convex hull
            initializing = true;
            AklTouHeuristic();
            initializing = false;

            // Consider all remaining vertices. Store them with the faces that they are 'beyond'
            var justTheFaces = convexFaceHeap.GetValues().ToArray();
            foreach (var face in justTheFaces)
            {
                FindBeyondVertices(face, origVertices);
                if (face.VerticesBeyond.Count == 0)
                {
                    convexFaces.Add(face);
                    convexFaceHeap.Delete(face.FibCell);
                    face.FibCell = null;
                }
                else convexFaceHeap.ChangeKey(face.FibCell, face.MinVertexKey);
            }

            // Now a final loop to expand the convex hull and faces based on these beyond vertices
            while (!convexFaceHeap.IsEmpty && convexFaceHeap.Top.Priority >= 0)
            {
                var currentFace = convexFaceHeap.Top.Value;
                var currentVertex = currentFace.MinVertex;
                convexHull.Add(currentVertex);
                UpdateCenter(currentVertex);

                // The affected faces get tagged
                FindAffectedFaces(currentFace, currentVertex);
                UpdateFaces(affectedFaceBuffer, currentVertex);
                // Need to reset the tags
                int count = affectedFaceBuffer.Count;
                for (int i = 0; i < count; i++) affectedFaceBuffer[i].Tag = 0;
            }

            // Remove any remaining recycled faces
            while (recycledFaceStack.Count > 0)
            {
                var f = recycledFaceStack.Pop();
                convexFaceHeap.Delete(f.FibCell);
                f.FibCell = null;
            }
        }      

        private ConvexHullInternal(IEnumerable<IVertex> vertices)
        {
            origVertices = new List<VertexWrap>(vertices.Select((v, i) => new VertexWrap { Vertex = v, PositionData = v.Position }));
        }

        private IEnumerable<TVertex> GetConvexHullInternal<TVertex>(int dimensions = -1, bool onlyCompute = false) where TVertex : IVertex
        {
            if (computed) return onlyCompute ? null : convexHull.Select(v => (TVertex)v.Vertex).ToArray();
            
            if (dimensions == -1) DetermineDimension(origVertices);
            else dimension = dimensions;
            Initialize();
            if (dimension < 2) throw new ArgumentException("Dimensions of space must be 2 or greater.");
            if (!computed)
            {
                if (dimension == 2) Find2D();
                else FindConvexHull();
                computed = true;
            }
            return onlyCompute ? null : convexHull.Select(v => (TVertex)v.Vertex).ToArray();
        }

        public static IEnumerable<TVertex> GetConvexHull<TVertex>(IEnumerable<TVertex> data) where TVertex : IVertex
        {
            ConvexHullInternal ch = new ConvexHullInternal(data.Cast<IVertex>());
            return ch.GetConvexHullInternal<TVertex>();
        }

        private IEnumerable<TFace> GetConvexFacesInternal<TVertex, TFace>() 
            where TFace : ConvexFace<TVertex, TFace>, new() 
            where TVertex : IVertex
        {
            if (!computed) GetConvexHullInternal<TVertex>(-1, true);

            var faces = convexFaces;
            int cellCount = faces.Count;
            var cells = new TFace[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var face = faces[i];
                var vertices = new TVertex[dimension];
                for (int j = 0; j < dimension; j++) vertices[j] = (TVertex)face.Vertices[j].Vertex;
                cells[i] = new TFace
                {
                    Vertices = vertices,
                    AdjacentFaces = new TFace[dimension],
                    Normal = face.Normal
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                var face = faces[i];
                var cell = cells[i];
                for (int j = 0; j < dimension; j++)
                {
                    if (face.AdjacentFaces[j] == null) continue;
                    cell.AdjacentFaces[j] = cells[face.AdjacentFaces[j].Tag];
                }
            }
            
            return cells;
        }

        /// <summary>
        /// This is used by the Delaunay triangulation code.
        /// </summary>
        internal static List<ConvexFaceInternal> GetConvexFacesInternal<TVertex, TFace>(IEnumerable<TVertex> data)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            ConvexHullInternal ch = new ConvexHullInternal(data.Cast<IVertex>());
            ch.GetConvexHullInternal<TVertex>(-1, true);
            return ch.convexFaces;
        }

        internal static IEnumerable<TFace> GetConvexFaces<TVertex, TFace>(IEnumerable<TVertex> data)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            ConvexHullInternal ch = new ConvexHullInternal(data.Cast<IVertex>());
            return ch.GetConvexFacesInternal<TVertex, TFace>();
        }
        
        internal static Tuple<IEnumerable<TVertex>, IEnumerable<TFace>> GetConvexHullAndFaces<TVertex, TFace>(IEnumerable<IVertex> data)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            ConvexHullInternal ch = new ConvexHullInternal(data);
            return Tuple.Create(
                ch.GetConvexHullInternal<TVertex>(),
                ch.GetConvexFacesInternal<TVertex, TFace>());
        }
    }
}
