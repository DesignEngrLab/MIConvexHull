﻿/******************************************************************************
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
    using System.Collections.Generic;

    /*
     * Code here handles triangulation related stuff.
     */
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// Computes the Delaunay triangulation.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TCell"></typeparam>
        /// <param name="data"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        internal static TCell[] GetDelaunayTriangulation<TVertex, TCell>(IList<TVertex> data, TriangulationComputationConfig config)
            where TCell : TriangulationCell<TVertex, TCell>, new()
            where TVertex : IVertex
        {
            config = config ?? new TriangulationComputationConfig();

            var vertices = new IVertex[data.Count];
            for (int i = 0; i < data.Count; i++) vertices[i] = data[i];
            ConvexHullInternal ch = new ConvexHullInternal(vertices, true, config);
            ch.FindConvexHull();
            ch.PostProcessTriangulation(config);
            
            return ch.GetConvexFaces<TVertex, TCell>();
        }

        /// <summary>
        /// Remove the upper faces from the hull.
        /// Remove empty boundary cells if shifting was used.
        /// </summary>
        /// <param name="config"></param>
        void PostProcessTriangulation(TriangulationComputationConfig config)
        {
            RemoveUpperFaces();
            if (config.PointTranslationType == PointTranslationType.TranslateInternal)
            {
                RemoveEmptyBoundaryCells(config.ZeroCellVolumeTolerance);
            }
        }

        /// <summary>
        /// Removes up facing Tetrahedrons from the triangulation.
        /// </summary>
        void RemoveUpperFaces()
        {
            var delaunayFaces = ConvexFaces;
            var dimension = Dimension - 1;

            // Remove the "upper" faces
            for (var i = delaunayFaces.Count - 1; i >= 0; i--)
            {
                var candidateIndex = delaunayFaces[i];
                var candidate = FacePool[candidateIndex];
                if (candidate.Normal[dimension] >= 0.0)
                {
                    for (int fi = 0; fi < candidate.AdjacentFaces.Length; fi++)
                    {
                        var af = candidate.AdjacentFaces[fi];
                        if (af >= 0)
                        {
                            var face = FacePool[af];
                            for (int j = 0; j < face.AdjacentFaces.Length; j++)
                            {
                                if (face.AdjacentFaces[j] == candidateIndex)
                                {
                                    face.AdjacentFaces[j] = -1;
                                }
                            }
                        }
                    }
                    delaunayFaces[i] = delaunayFaces[delaunayFaces.Count - 1];
                    delaunayFaces.Pop();
                }
            }
        }

        /// <summary>
        /// Removes the empty boundary cells that might have been created using PointTranslationType.TranslateInternal.
        /// </summary>
        /// <param name="tolerance"></param>
        void RemoveEmptyBoundaryCells(double tolerance)
        {
            var faces = ConvexFaces;
            var pool = FacePool;
            var dimension = Dimension - 1;

            bool[] visited = new bool[pool.Length];
            bool[] remove = new bool[pool.Length];
            IndexBuffer toTest = new IndexBuffer();

            for (var i = faces.Count - 1; i >= 0; i--)
            {
                var adj = pool[faces[i]].AdjacentFaces;
                for (int j = 0; j < adj.Length; j++)
                {
                    if (adj[j] < 0)
                    {
                        toTest.Push(faces[i]);
                        break;
                    }
                }
            }

            double[][] buffer = new double[dimension][];
            for (int i = 0; i < dimension; i++) buffer[i] = new double[dimension];

            var simplexVolumeBuffer = new MathHelper.SimplexVolumeBuffer(dimension);
            while (toTest.Count > 0)
            {
                var top = toTest.Pop();
                var face = pool[top];
                visited[top] = true;

                if (MathHelper.GetSimplexVolume(face, Vertices, simplexVolumeBuffer) < tolerance)
                {
                    remove[top] = true;

                    var adj = face.AdjacentFaces;
                    for (int j = 0; j < adj.Length; j++)
                    {
                        var n = adj[j];
                        if (n >= 0 && !visited[n]) toTest.Push(n);
                    }
                }
            }

            for (int i = faces.Count - 1; i >= 0; i--)
            {
                if (remove[faces[i]])
                {
                    var candidateIndex = faces[i];
                    var candidate = pool[candidateIndex];
                    for (int fi = 0; fi < candidate.AdjacentFaces.Length; fi++)
                    {
                        var af = candidate.AdjacentFaces[fi];
                        if (af >= 0)
                        {
                            var face = pool[af];
                            for (int j = 0; j < face.AdjacentFaces.Length; j++)
                            {
                                if (face.AdjacentFaces[j] == candidateIndex)
                                {
                                    face.AdjacentFaces[j] = -1;
                                }
                            }
                        }
                    }

                    faces[i] = faces[faces.Count - 1];
                    faces.Pop();
                }
            }
        }
    }
}
