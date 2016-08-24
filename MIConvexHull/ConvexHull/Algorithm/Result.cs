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
    using System.Collections.Generic;

    /*
     * Code here transforms the result to its final form 
     */
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// This is called by the "ConvexHull" class.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig.GetDefault() is used.</param>
        /// <returns></returns>
        internal static ConvexHull<TVertex, TFace> GetConvexHull<TVertex, TFace>(IList<TVertex> data, ConvexHullComputationConfig config)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            config = config ?? new ConvexHullComputationConfig();

            var vertices = new IVertex[data.Count];
            for (int i = 0; i < data.Count; i++) vertices[i] = data[i];
            ConvexHullInternal ch = new ConvexHullInternal(vertices, false, config);
            ch.FindConvexHull();

            var hull = ch.GetHullVertices(data);

            return new ConvexHull<TVertex, TFace> { Points = hull, Faces = ch.GetConvexFaces<TVertex, TFace>() };
        }
        
        TVertex[] GetHullVertices<TVertex>(IList<TVertex> data)
        {
            int cellCount = ConvexFaces.Count;
            int hullVertexCount = 0;
            int vertexCount = Vertices.Length;

            for (int i = 0; i < vertexCount; i++) VertexMarks[i] = false;
            
            for (int i = 0; i < cellCount; i++)
            {
                var vs = FacePool[ConvexFaces[i]].Vertices;
                for (int j = 0; j < vs.Length; j++)
                {
                    var v = vs[j];
                    if (!VertexMarks[v])
                    {
                        VertexMarks[v] = true;
                        hullVertexCount++;
                    }
                }
            }

            var result = new TVertex[hullVertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                if (VertexMarks[i]) result[--hullVertexCount] = data[i];
            }

            return result;
        }

        /// <summary>
        /// Finds the convex hull and creates the TFace objects.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <returns></returns>
        TFace[] GetConvexFaces<TVertex, TFace>()
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            var faces = ConvexFaces;
            int cellCount = faces.Count;
            var cells = new TFace[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var face = FacePool[faces[i]];
                var vertices = new TVertex[Dimension];
                for (int j = 0; j < Dimension; j++)
                {
                    vertices[j] = (TVertex)this.Vertices[face.Vertices[j]];
                }

                cells[i] = new TFace
                {
                    Vertices = vertices,
                    Adjacency = new TFace[Dimension],
                    Normal = IsLifted ? null : face.Normal
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                var face = FacePool[faces[i]];
                var cell = cells[i];
                for (int j = 0; j < Dimension; j++)
                {
                    if (face.AdjacentFaces[j] < 0) continue;
                    cell.Adjacency[j] = cells[FacePool[face.AdjacentFaces[j]].Tag];
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
    }
}
