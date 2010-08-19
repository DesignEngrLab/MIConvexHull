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
    using StarMathLib;
    using System.Linq;

    /// <summary>
    /// functions called from Find for the 3D case. 
    /// </summary>
    public static partial class ConvexHull
    {
        #region Make functions
        static void replaceFace(List<IFaceConvHull> faces, IList<IFaceConvHull> oldFaces, IVertexConvHull currentVertex)
        {
            faces.RemoveAll(f => oldFaces.Contains(f));
            var edges = findFreeEdges(oldFaces);
            foreach (var edge in edges)
            {
                faces.Add(MakeFace(currentVertex, edge));
            }

        }
        static IFaceConvHull MakeFace(List<IVertexConvHull> convexHull, int notFaceIndex)
        {
            var vertices = new List<IVertexConvHull>(convexHull);
            vertices.RemoveAt(notFaceIndex);
            return MakeFace(vertices);
        }
        static IFaceConvHull MakeFace(IVertexConvHull currentVertex, IVertexConvHull[] edge)
        {
            var vertices = new List<IVertexConvHull>(edge);
            vertices.Add(currentVertex);
            return MakeFace(vertices);
        }
        static IFaceConvHull MakeFace(List<IVertexConvHull> vertices)
        {
            var outDir = new double[dimension];
            foreach (var v in vertices)
                outDir = StarMath.add(outDir, v.location);
            outDir = StarMath.divide(outDir, dimension);
            outDir = StarMath.subtract(outDir, center);
            double[] normal = findNormalVector(vertices);
            if (StarMath.multiplyDot(normal, outDir) < 0)
            {
                if (dimension == 3 || dimension == 7)
                    vertices.Reverse();
                normal = StarMath.subtract(StarMath.makeZeroVector(dimension), normal);
            }
            IFaceConvHull newFace = null;
            if (faceType != null)
            {
                var constructor = faceType.GetConstructor(new Type[0]);
                newFace = (IFaceConvHull)constructor.Invoke(new object[0]);
            }
            if (newFace == null) newFace = new defFaceClass(dimension);

            if (newFace == null)
                throw new Exception("Face was not created. Problem with constructor.");
            if (StarMath.norm2(normal) < 0.9)
                throw new Exception("Face was not created. Probably repeat vertices in list.");
            newFace.vertices = vertices.ToArray();
            newFace.normal = normal;
            return newFace;
        }


        #endregion
        #region Find and Get functions
        /// <summary>
        /// Gets the IVertexConvHull from extreme matrix.
        /// </summary>
        /// <param name="extremeVertices">The extreme vertices matrix.</param>
        /// <param name="v">The three indices but these are from -1 to 1, need to adjust to 0 to 2.</param>
        /// <returns>the IVertexConvHull at the location in  extremeVertices</returns>
        static IVertexConvHull getVertexFromExtreme(IVertexConvHull[, ,] extremeVertices, int[] v)
        {
            return extremeVertices[v[0] + 1, v[1] + 1, v[2] + 1];
        }

        static SortedList<double, IFaceConvHull> findOverFaces(List<IFaceConvHull> convexFaces, IVertexConvHull currentVertex)
        {
            var overFaces = new SortedList<double, IFaceConvHull>(new noEqualSortMaxtoMinDouble());
            foreach (var face in convexFaces)
            {
                double dotP;
                if (overFace(currentVertex, face, out dotP))
                    overFaces.Add(dotP, face);
            }
            return overFaces;
        }

        static List<IVertexConvHull[]> findFreeEdges(IList<IFaceConvHull> faces)
        {
            var edges = new List<IVertexConvHull[]>();
            foreach (var f in faces)
            {
                var edge = new IVertexConvHull[dimension - 1];
                Array.Copy(f.vertices, 0, edge, 0, (dimension - 1));
                edges.Add((IVertexConvHull[])edge.Clone());
                Array.Copy(f.vertices, 1, edge, 0, (dimension - 1));
                edges.Add((IVertexConvHull[])edge.Clone());
                for (int i = 2; i < f.vertices.GetLength(0); i++)
                {
                    Array.Copy(f.vertices, i, edge, 0, (dimension - i));
                    Array.Copy(f.vertices, 0, edge, (dimension - i), (i - 1));
                    edges.Add((IVertexConvHull[])edge.Clone());
                }
            }
            for (int i = 0; i < faces.Count - 1; i++)
                for (int j = i + 1; j < faces.Count; j++)
                {
                    IVertexConvHull[] sharedEdge = null;
                    if (shareEdge(faces[i], faces[j], out sharedEdge))
                        edges.RemoveAll(e => sameEdge(e, sharedEdge));
                }
            return edges;
        }
        static double simplexVolumeFromCenter(IFaceConvHull face)
        {
            double[,] simplex = new double[dimension, dimension];
            for (int i = 0; i < dimension; i++)
                StarMath.SetColumn(i, simplex, StarMath.subtract(face.vertices[i].location, center));
            return StarMath.determinant(simplex);
        }
        private static double[] findNormalVector(List<IVertexConvHull> vertices)
        {
            double[] normal;
            if (dimension == 3 || dimension == 7)
                normal = StarMath.multiplyCross(StarMath.subtract(vertices[1].location, vertices[0].location),
                    StarMath.subtract(vertices[2].location, vertices[1].location));
            else
            {
                throw new NotImplementedException();
            }
            return StarMath.normalize(normal);
        }


        #endregion
        #region Predicates
        /// <summary>
        /// Checks if faces are the same.
        /// </summary>
        /// <param name="f1">The f1.</param>
        /// <param name="f2">The f2.</param>
        /// <returns></returns>
        static Boolean sameFace(IFaceConvHull f1, IFaceConvHull f2)
        {
            if (f1.Equals(f2)) return true;
            if (f1.vertices.Intersect(f2.vertices).Count() == dimension) return true;
            return false;
        }


        static Boolean sameEdge(IVertexConvHull[] e1, IVertexConvHull[] e2)
        {
            if (e1.Equals(e2)) return true;
            if (e1.Intersect(e2).Count() == (dimension - 1)) return true;
            return false;
        }


        static Boolean shareEdge(IFaceConvHull f1, IFaceConvHull f2, out IVertexConvHull[] edge)
        {
            edge = null;
            var sharedVerts = f1.vertices.Intersect(f2.vertices);
            if (sharedVerts.Count() < (dimension - 1)) return false;
            //sharedVerts.OrderBy(v => Array.IndexOf(f1.vertices, v));
            edge = sharedVerts.ToArray();
            return true;
        }

        static Boolean overFace(IVertexConvHull v, IFaceConvHull f, out double dotP)
        {
            dotP = StarMath.multiplyDot(f.normal, StarMath.subtract(v.location, f.vertices[0].location));
            return (dotP >= 0);
        }
        static Boolean sameDirection(double[] c, double[] p)
        {
            if (c.GetLength(0) != p.GetLength(0)) return false;
            for (int i = 0; i < c.GetLength(0); i++)
                if (c[i] / p[i] > 0) return true;
                else if (c[i] / p[i] < 0) return false;
            return false;
        }

        #endregion








    }
}