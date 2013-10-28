/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2013 David Sehnal, Matthew Campbell
 *
 *  This library is free software; you can redistribute it and/or modify it 
 *  under the terms of  the GNU Lesser General Public License as published by 
 *  the Free Software Foundation; either version 2.1 of the License, or 
 *  (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful, 
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of 
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser 
 *  General Public License for more details.
 *  
 *****************************************************************************/

namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ConvexHullInternal
    {
        bool Computed;
        readonly int Dimension;

        List<VertexWrap> InputVertices;
        List<VertexWrap> ConvexHull;
        FaceList UnprocessedFaces;
        List<ConvexFaceInternal> ConvexFaces;

        VertexWrap CurrentVertex;
        double MaxDistance;
        VertexWrap FurthestVertex;

        /// <summary>
        /// The centroid of the currently computed hull.
        /// </summary>
        double[] Center;
        
        ConvexFaceInternal[] UpdateBuffer;
        int[] UpdateIndices;

        Stack<ConvexFaceInternal> TraverseStack;

        VertexBuffer EmptyBuffer; // this is used for VerticesBeyond for faces that are on the convex hull
        VertexBuffer BeyondBuffer;
        List<ConvexFaceInternal> AffectedFaceBuffer;
        List<DeferredFace> ConeFaceBuffer;        
        HashSet<VertexWrap> SingularVertices;

        const int ConnectorTableSize = 2017;
        ConnectorList[] ConnectorTable;

        ObjectManager ObjectManager;
        MathHelper MathHelper;

        /// <summary>
        /// Initialize buffers and lists.
        /// </summary>
        void Initialize()
        {
            ConvexHull = new List<VertexWrap>();
            UnprocessedFaces = new FaceList(); // new LinkedList<ConvexFaceInternal>();
            ConvexFaces = new List<ConvexFaceInternal>();

            ObjectManager = new MIConvexHull.ObjectManager(Dimension);
            MathHelper = new MIConvexHull.MathHelper(Dimension);

            Center = new double[Dimension];            
            TraverseStack = new Stack<ConvexFaceInternal>();
            UpdateBuffer = new ConvexFaceInternal[Dimension];
            UpdateIndices = new int[Dimension];
            EmptyBuffer = new VertexBuffer();
            AffectedFaceBuffer = new List<ConvexFaceInternal>();
            ConeFaceBuffer = new List<DeferredFace>();
            SingularVertices = new HashSet<VertexWrap>();
            BeyondBuffer = new VertexBuffer();

            ConnectorTable = Enumerable.Range(0, ConnectorTableSize).Select(_ => new ConnectorList()).ToArray();           
        }

        /// <summary>
        /// Check the dimensionality of the input data.
        /// </summary>
        int DetermineDimension()
        {
            var r = new Random();
            var VCount = InputVertices.Count;
            var dimensions = new List<int>();
            for (var i = 0; i < 10; i++)
                dimensions.Add(InputVertices[r.Next(VCount)].Vertex.Position.Length);
            var dimension = dimensions.Min();
            if (dimension != dimensions.Max()) throw new ArgumentException("Invalid input data (non-uniform dimension).");
            return dimension;
        }

        /// <summary>
        /// Create the first faces from (dimension + 1) vertices.
        /// </summary>
        /// <returns></returns>
        ConvexFaceInternal[] InitiateFaceDatabase()
        {
            var faces = new ConvexFaceInternal[Dimension + 1];

            for (var i = 0; i < Dimension + 1; i++)
            {
                var vertices = ConvexHull.Where((_, j) => i != j).ToArray(); // Skips the i-th vertex
                var newFace = new ConvexFaceInternal(Dimension, new VertexBuffer());
                newFace.Vertices = vertices;
                Array.Sort(vertices, VertexWrapComparer.Instance);
                CalculateFacePlane(newFace);
                faces[i] = newFace;
            }

            // update the adjacency (check all pairs of faces)
            for (var i = 0; i < Dimension; i++)
            {
                for (var j = i + 1; j < Dimension + 1; j++) UpdateAdjacency(faces[i], faces[j]);
            }

            return faces;
        }
        
        /// <summary>
        /// Calculates the normal and offset of the hyper-plane given by the face's vertices.
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private bool CalculateFacePlane(ConvexFaceInternal face)
        {
            var vertices = face.Vertices;
            var normal = face.Normal;
            MathHelper.FindNormalVector(vertices, normal);

            if (double.IsNaN(normal[0]))
            {
                return false;
            }

            double offset = 0.0;
            double centerDistance = 0.0;
            var fi = vertices[0].PositionData;
            for (int i = 0; i < Dimension; i++)
            {
                double n = normal[i];
                offset += n * fi[i];
                centerDistance += n * Center[i];
            }
            face.Offset = -offset;
            centerDistance -= offset;

            if (centerDistance > 0)
            {
                for (int i = 0; i < Dimension; i++) normal[i] = -normal[i];
                face.Offset = offset;
                face.IsNormalFlipped = true;
            }
            else face.IsNormalFlipped = false;

            return true;
        }
                
        /// <summary>
        /// Tags all faces seen from the current vertex with 1.
        /// </summary>
        /// <param name="currentFace"></param>
        void TagAffectedFaces(ConvexFaceInternal currentFace)
        {
            AffectedFaceBuffer.Clear();
            AffectedFaceBuffer.Add(currentFace);
            TraverseAffectedFaces(currentFace);
        }
        
        /// <summary>
        /// Recursively traverse all the relevant faces.
        /// </summary>
        void TraverseAffectedFaces(ConvexFaceInternal currentFace)
        {
            TraverseStack.Clear();
            TraverseStack.Push(currentFace);
            currentFace.Tag = 1;

            while (TraverseStack.Count > 0)
            {
                var top = TraverseStack.Pop();
                for (int i = 0; i < Dimension; i++)
                {
                    var adjFace = top.AdjacentFaces[i];

                    if (adjFace.Tag == 0 && MathHelper.GetVertexDistance(CurrentVertex, adjFace) >= Constants.PlaneDistanceTolerance)
                    {
                        AffectedFaceBuffer.Add(adjFace);
                        adjFace.Tag = 1;
                        TraverseStack.Push(adjFace);
                    }
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
            for (i = 0; i < Dimension; i++) lv[i].Marked = false;

            // mark all vertices on the 2nd face
            for (i = 0; i < Dimension; i++) rv[i].Marked = true;

            // find the 1st false index
            for (i = 0; i < Dimension; i++) if (!lv[i].Marked) break;

            // no vertex was marked
            if (i == Dimension) return;

            // check if only 1 vertex wasn't marked
            for (int j = i + 1; j < Dimension; j++) if (!lv[j].Marked) return;

            // if we are here, the two faces share an edge
            l.AdjacentFaces[i] = r;

            // update the adj. face on the other face - find the vertex that remains marked
            for (i = 0; i < Dimension; i++) lv[i].Marked = false;
            for (i = 0; i < Dimension; i++)
            {
                if (rv[i].Marked) break;
            }
            r.AdjacentFaces[i] = l;
        }
        
        /// <summary>
        /// Creates a new deferred face.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="faceIndex"></param>
        /// <param name="pivot"></param>
        /// <param name="pivotIndex"></param>
        /// <param name="oldFace"></param>
        /// <returns></returns>
        DeferredFace MakeDeferredFace(ConvexFaceInternal face, int faceIndex, ConvexFaceInternal pivot, int pivotIndex, ConvexFaceInternal oldFace)
        {
            var ret = ObjectManager.GetDeferredFace();
            
            ret.Face = face;
            ret.FaceIndex = faceIndex;
            ret.Pivot = pivot;
            ret.PivotIndex = pivotIndex;
            ret.OldFace = oldFace;

            return ret;
        }

        /// <summary>
        /// Connect faces using a connector.
        /// </summary>
        /// <param name="connector"></param>
        void ConnectFace(FaceConnector connector)
        {
            var index = connector.HashCode % ConnectorTableSize;
            var list = ConnectorTable[index];

            for (var current = list.First; current != null; current = current.Next)
            {
                if (FaceConnector.AreConnectable(connector, current, Dimension))
                {
                    list.Remove(current);
                    FaceConnector.Connect(current, connector);
                    current.Face = null;
                    connector.Face = null;
                    ObjectManager.DepositConnector(current);
                    ObjectManager.DepositConnector(connector);
                    return;
                }
            }

            list.Add(connector);
        }

        /// <summary>
        /// Removes the faces "covered" by the current vertex and adds the newly created ones.
        /// </summary>
        private bool CreateCone()
        {
            var currentVertexIndex = CurrentVertex.Index;
            ConeFaceBuffer.Clear();

            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var oldFace = AffectedFaceBuffer[fIndex];

                // Find the faces that need to be updated
                int updateCount = 0;
                for (int i = 0; i < Dimension; i++)
                {
                    var af = oldFace.AdjacentFaces[i];
                    if (af.Tag == 0) // Tag == 0 when oldFaces does not contain af
                    {
                        UpdateBuffer[updateCount] = af;
                        UpdateIndices[updateCount] = i;
                        ++updateCount;
                    }
                }

                for (int i = 0; i < updateCount; i++)
                {
                    var adjacentFace = UpdateBuffer[i];

                    int oldFaceAdjacentIndex = 0;
                    var adjFaceAdjacency = adjacentFace.AdjacentFaces;
                    for (int j = 0; j < Dimension; j++)
                    {
                        if (object.ReferenceEquals(oldFace, adjFaceAdjacency[j]))
                        {
                            oldFaceAdjacentIndex = j;
                            break;
                        }
                    }

                    var forbidden = UpdateIndices[i]; // Index of the face that corresponds to this adjacent face

                    ConvexFaceInternal newFace;

                    int oldVertexIndex;
                    VertexWrap[] vertices;

                    newFace = ObjectManager.GetFace();
                    vertices = newFace.Vertices;
                    for (int j = 0; j < Dimension; j++) vertices[j] = oldFace.Vertices[j];
                    oldVertexIndex = vertices[forbidden].Index;

                    int orderedPivotIndex;

                    // correct the ordering
                    if (currentVertexIndex < oldVertexIndex)
                    {
                        orderedPivotIndex = 0;
                        for (int j = forbidden - 1; j >= 0; j--)
                        {
                            if (vertices[j].Index > currentVertexIndex) vertices[j + 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        orderedPivotIndex = Dimension - 1;
                        for (int j = forbidden + 1; j < Dimension; j++)
                        {
                            if (vertices[j].Index < currentVertexIndex) vertices[j - 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j - 1;
                                break;
                            }
                        }
                    }
                    
                    vertices[orderedPivotIndex] = CurrentVertex;

                    if (!CalculateFacePlane(newFace))
                    {
                        return false;
                    }

                    ConeFaceBuffer.Add(MakeDeferredFace(newFace, orderedPivotIndex, adjacentFace, oldFaceAdjacentIndex, oldFace));
                }
            }
            
            return true;
        }

        /// <summary>
        /// Commits a cone and adds a vertex to the convex hull.
        /// </summary>
        void CommitCone()
        {
            // Add the current vertex.
            ConvexHull.Add(CurrentVertex);
            
            // Fill the adjacency.
            for (int i = 0; i < ConeFaceBuffer.Count; i++)
            {
                var face = ConeFaceBuffer[i];

                var newFace = face.Face;
                var adjacentFace = face.Pivot;
                var oldFace = face.OldFace;
                var orderedPivotIndex = face.FaceIndex;

                newFace.AdjacentFaces[orderedPivotIndex] = adjacentFace;
                adjacentFace.AdjacentFaces[face.PivotIndex] = newFace;

                // let there be a connection.
                for (int j = 0; j < Dimension; j++)
                {
                    if (j == orderedPivotIndex) continue;
                    var connector = ObjectManager.GetConnector();
                    connector.Update(newFace, j, Dimension);
                    ConnectFace(connector);
                }

                // This could slightly help...
                if (adjacentFace.VerticesBeyond.Count < oldFace.VerticesBeyond.Count)
                {
                    FindBeyondVertices(newFace, adjacentFace.VerticesBeyond, oldFace.VerticesBeyond);
                }
                else
                {
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond, adjacentFace.VerticesBeyond);
                }

                // This face will definitely lie on the hull
                if (newFace.VerticesBeyond.Count == 0)
                {
                    ConvexFaces.Add(newFace);
                    UnprocessedFaces.Remove(newFace);
                    ObjectManager.DepositVertexBuffer(newFace.VerticesBeyond);
                    newFace.VerticesBeyond = EmptyBuffer;
                }
                else // Add the face to the list
                {
                    UnprocessedFaces.Add(newFace);
                }

                // recycle the object.
                ObjectManager.DepositDeferredFace(face);
            }

            // Recycle the affected faces.
            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var face = AffectedFaceBuffer[fIndex];
                UnprocessedFaces.Remove(face);
                ObjectManager.DepositFace(face);                
            }
        }
        
        /// <summary>
        /// Check whether the vertex v is beyond the given face. If so, add it to beyondVertices.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="beyondVertices"></param>
        /// <param name="v"></param>
        void IsBeyond(ConvexFaceInternal face, VertexBuffer beyondVertices, VertexWrap v)
        {
            double distance = MathHelper.GetVertexDistance(v, face);
            if (distance >= Constants.PlaneDistanceTolerance)
            {
                if (distance > MaxDistance)
                {
                    MaxDistance = distance;
                    FurthestVertex = v;
                }
                beyondVertices.Add(v);
            }
        }

        /// <summary>
        /// Used in the "initialization" code.
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face)
        {
            var beyondVertices = face.VerticesBeyond;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = null;

            int count = InputVertices.Count;
            for (int i = 0; i < count; i++) IsBeyond(face, beyondVertices, InputVertices[i]);

            face.FurthestVertex = FurthestVertex;
            //face.FurthestDistance = MaxDistance;
        }

        /// <summary>
        /// Used by update faces.
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face, VertexBuffer beyond, VertexBuffer beyond1)
        {
            var beyondVertices = BeyondBuffer;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = null;
            VertexWrap v;

            int count = beyond1.Count;
            for (int i = 0; i < count; i++) beyond1[i].Marked = true;
            CurrentVertex.Marked = false;
            count = beyond.Count;
            for (int i = 0; i < count; i++)
            {
                v = beyond[i];
                if (object.ReferenceEquals(v, CurrentVertex)) continue;
                v.Marked = false;
                IsBeyond(face, beyondVertices, v);
            }

            count = beyond1.Count;
            for (int i = 0; i < count; i++)
            {
                v = beyond1[i];
                if (v.Marked) IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = FurthestVertex;
            //face.FurthestDistance = MaxDistance;

            // Pull the old switch a roo
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            BeyondBuffer = temp;
        }
                
        /// <summary>
        /// Recalculates the centroid of the current hull.
        /// </summary>
        void UpdateCenter()
        {
            var count = ConvexHull.Count + 1;
            for (int i = 0; i < Dimension; i++) Center[i] *= (count - 1);
            double f = 1.0 / count;
            for (int i = 0; i < Dimension; i++) Center[i] = f * (Center[i] + CurrentVertex.PositionData[i]);
        }

        /// <summary>
        /// Removes the last vertex from the center.
        /// </summary>
        void RollbackCenter()
        {
            var count = ConvexHull.Count + 1;
            for (int i = 0; i < Dimension; i++) Center[i] *= count;
            double f = 1.0 / (count - 1);
            for (int i = 0; i < Dimension; i++) Center[i] = f * (Center[i] - CurrentVertex.PositionData[i]);
        }

        /// <summary>
        /// Find the (dimension+1) initial points and create the simplexes.
        /// </summary>
        void InitConvexHull()
        {
            var extremes = FindExtremes();
            var initialPoints = FindInitialPoints(extremes);

            // Add the initial points to the convex hull.
            foreach (var vertex in initialPoints)
            {
                CurrentVertex = vertex;
                // update center must be called before adding the vertex.
                UpdateCenter();
                ConvexHull.Add(CurrentVertex);
                InputVertices.Remove(vertex);

                // Because of the AklTou heuristic.
                extremes.Remove(vertex);
            }

            // Create the initial simplexes.
            var faces = InitiateFaceDatabase();

            // Init the vertex beyond buffers.
            foreach (var face in faces)
            {
                FindBeyondVertices(face);
                if (face.VerticesBeyond.Count == 0) ConvexFaces.Add(face); // The face is on the hull
                else UnprocessedFaces.Add(face);
            }
        }

        /// <summary>
        /// Finds (dimension + 1) initial points.
        /// </summary>
        /// <param name="extremes"></param>
        /// <returns></returns>
        private List<VertexWrap> FindInitialPoints(List<VertexWrap> extremes)
        {
            List<VertexWrap> initialPoints = new List<VertexWrap>();// { extremes[0], extremes[1] };

            VertexWrap first = null, second = null;
            double maxDist = 0;
            double[] temp = new double[Dimension];
            for (int i = 0; i < extremes.Count - 1; i++)
            {
                var a = extremes[i];
                for (int j = i + 1; j < extremes.Count; j++)
                {
                    var b = extremes[j];
                    MathHelper.SubtractFast(a.PositionData, b.PositionData, temp);
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
                double maximum = 0.000001;
                VertexWrap maxPoint = null;
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
                if (maxPoint != null) initialPoints.Add(maxPoint);
                else
                {
                    int vCount = InputVertices.Count;
                    for (int j = 0; j < vCount; j++)
                    {
                        var point = InputVertices[j];
                        if (initialPoints.Contains(point)) continue;

                        var val = GetSquaredDistanceSum(point, initialPoints);

                        if (val > maximum)
                        {
                            maximum = val;
                            maxPoint = point;
                        }
                    }

                    if (maxPoint != null) initialPoints.Add(maxPoint);
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
        double GetSquaredDistanceSum(VertexWrap pivot, List<VertexWrap> initialPoints)
        {
            var initPtsNum = initialPoints.Count;
            var sum = 0.0;
        
            for (int i = 0; i < initPtsNum; i++)
            {
                var initPt = initialPoints[i];
                for (int j = 0; j < Dimension; j++)
                {
                    double t = (initPt.PositionData[j] - pivot.PositionData[j]);
                    sum += t * t;
                }
            }

            return sum;
        }

        /// <summary>
        /// Finds the extremes in all dimensions.
        /// </summary>
        /// <returns></returns>
        private List<VertexWrap> FindExtremes()
        {
            var extremes = new List<VertexWrap>(2 * Dimension);

            int vCount = InputVertices.Count;
            for (int i = 0; i < Dimension; i++)
            {
                double min = double.MaxValue, max = double.MinValue;
                int minInd = 0, maxInd = 0;
                for (int j = 0; j < vCount; j++)
                {
                    var v = InputVertices[j].PositionData[i];
                    if (v < min)
                    {
                        min = v;
                        minInd = j;
                    }
                    if (v > max)
                    {
                        max = v;
                        maxInd = j;
                    }
                }

                if (minInd != maxInd)
                {
                    extremes.Add(InputVertices[minInd]);
                    extremes.Add(InputVertices[maxInd]);
                }
                else extremes.Add(InputVertices[minInd]);
            }
            return extremes;
        }

        /// <summary>
        /// The exception thrown if singular input data detected.
        /// </summary>
        void ThrowSingular()
        {
            throw new InvalidOperationException(
                    "ConvexHull: Singular input data (i.e. trying to triangulate a data that contain a regular lattice of points).\n"
                    + "Introducing some noise to the data might resolve the issue.");
        }

        /// <summary>
        /// Handles singular vertex.
        /// </summary>
        void HandleSingular()
        {
            RollbackCenter();
            SingularVertices.Add(CurrentVertex);

            // This means that all the affected faces must be on the hull and that all their "vertices beyond" are singular.
            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var face = AffectedFaceBuffer[fIndex];
                var vb = face.VerticesBeyond;
                for (int i = 0; i < vb.Count; i++)
                {
                    SingularVertices.Add(vb[i]);
                }

                ConvexFaces.Add(face);
                UnprocessedFaces.Remove(face);
                ObjectManager.DepositVertexBuffer(face.VerticesBeyond);
                face.VerticesBeyond = EmptyBuffer;
            }
        }

        /// <summary>
        /// Fins the convex hull.
        /// </summary>
        void FindConvexHull()
        {
            // Find the (dimension+1) initial points and create the simplexes.
            InitConvexHull();

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
                int count = AffectedFaceBuffer.Count;
                for (int i = 0; i < count; i++) AffectedFaceBuffer[i].Tag = 0;
            }
        }

        /// <summary>
        /// Wraps the vertices and determines the dimension if it's unknown.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="dim"></param>
        private ConvexHullInternal(IEnumerable<IVertex> vertices)
        {
            InputVertices = new List<VertexWrap>(vertices.Select((v, i) => new VertexWrap { Vertex = v, PositionData = v.Position, Index = i }));
            Dimension = DetermineDimension();
            Initialize();
        }

        /// <summary>
        /// Finds the vertices on the convex hull and optionally converts them to the TVertex array.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <param name="onlyCompute"></param>
        /// <returns></returns>
        private IEnumerable<TVertex> GetConvexHullInternal<TVertex>(bool onlyCompute = false) where TVertex : IVertex
        {
            if (Computed) return onlyCompute ? null : ConvexHull.Select(v => (TVertex)v.Vertex).ToArray();

            if (Dimension < 2) throw new ArgumentException("Dimension of the input must be 2 or greater.");

            FindConvexHull();
            Computed = true;
            return onlyCompute ? null : ConvexHull.Select(v => (TVertex)v.Vertex).ToArray();
        }

        /// <summary>
        /// Finds the convex hull and creates the TFace objects.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <returns></returns>
        private IEnumerable<TFace> GetConvexFacesInternal<TVertex, TFace>()
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            if (!Computed) GetConvexHullInternal<TVertex>(true);

            var faces = ConvexFaces;
            int cellCount = faces.Count;
            var cells = new TFace[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var face = faces[i];
                var vertices = new TVertex[Dimension];
                for (int j = 0; j < Dimension; j++) vertices[j] = (TVertex)face.Vertices[j].Vertex;
                cells[i] = new TFace
                {
                    Vertices = vertices,
                    Adjacency = new TFace[Dimension],
                    Normal = face.Normal
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                var face = faces[i];
                var cell = cells[i];
                for (int j = 0; j < Dimension; j++)
                {
                    if (face.AdjacentFaces[j] == null) continue;
                    cell.Adjacency[j] = cells[face.AdjacentFaces[j].Tag];
                }

                // Fix the vertex orientation.
                if (face.IsNormalFlipped)
                {
                    var tempVert = cell.Vertices[0];
                    cell.Vertices[0] = cell.Vertices[Dimension - 1];
                    cell.Vertices[Dimension - 1] = tempVert;

                    var tempAdj = cell.Adjacency[0];
                    cell.Adjacency[0] = cell.Adjacency[Dimension - 1];
                    cell.Adjacency[Dimension - 1] = tempAdj;
                }
            }
            
            return cells;
        }

        /// <summary>
        /// This is used by the Delaunay triangulation code.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static List<ConvexFaceInternal> GetConvexFacesInternal<TVertex, TFace>(IEnumerable<TVertex> data)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            ConvexHullInternal ch = new ConvexHullInternal(data.Cast<IVertex>());
            ch.GetConvexHullInternal<TVertex>(true);
            return ch.ConvexFaces;
        }

        /// <summary>
        /// This is called by the "ConvexHull" class.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
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
