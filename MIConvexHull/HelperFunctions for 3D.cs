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
        /// <summary>
        /// Gets the IVertexConvHull from extreme matrix.
        /// </summary>
        /// <param name="extremeVertices">The extreme vertices matrix.</param>
        /// <param name="v">The three indices but these are from -1 to 1, need to adjust to 0 to 2.</param>
        /// <returns>the IVertexConvHull at the location in  extremeVertices</returns>
        private static IVertexConvHull getVertexFromExtreme(IVertexConvHull[, ,] extremeVertices, int[] v)
        {
            return extremeVertices[v[0] + 1, v[1] + 1, v[2] + 1];
        }


        /// <summary>
        /// Determines whether the two faces share an edge.
        /// </summary>
        /// <param name="f1">The face, f1.</param>
        /// <param name="f2">The face, f2.</param>
        /// <param name="vFrom">The shared from vertex.</param>
        /// <param name="vTo">The shared to vertex.</param>
        /// <returns></returns>
        private static Boolean shareEdge(IFaceConvHull f1, IFaceConvHull f2, out IVertexConvHull vFrom, out IVertexConvHull vTo)
        {
            vFrom = null;
            vTo = null;
            var sharedVerts = f1.vertices.Intersect(f2.vertices);
            if (sharedVerts.Count() <= 1) return false;
            if (!sharedVerts.Contains(f1.vertices[0]))
            {
                vFrom = f1.vertices[1];
                vTo = f1.vertices[2];
            }
            if (!sharedVerts.Contains(f1.vertices[1]))
            {
                vFrom = f1.vertices[2];
                vTo = f1.vertices[0];
            }
            if (!sharedVerts.Contains(f1.vertices[2]))
            {
                vFrom = f1.vertices[0];
                vTo = f1.vertices[1];
            }
            return true;
        }


        private static Boolean shareEdge(IFaceConvHull f1, IFaceConvHull f2, out IVertexConvHull[] edge)
        {
            edge = null;
            throw new NotImplementedException();
            return false;
        }
        private static SortedList<double, IFaceConvHull> findOverFaces(List<IFaceConvHull> convexFaces, IVertexConvHull currentVertex)
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

        private static void replaceFace(List<IFaceConvHull> faces, IList<IFaceConvHull> oldFaces, IVertexConvHull currentVertex)
        {
            oldFaces = new List<IFaceConvHull>(oldFaces);
            if (oldFaces.Count == 0) return;
            else if (oldFaces.Count == 1) replaceOneFace(faces, oldFaces[0], currentVertex);
            // else if (oldFaces.Count == 2) replaceTwoFaces(faces, oldFaces, currentVertex);
            else
            {
                foreach (var f in oldFaces)
                    faces.Remove(f);
                IFaceConvHull overFace = null;
                var minOverVectorNorm = double.PositiveInfinity;
                var vertexOutVector = StarMath.normalize(StarMath.subtract(currentVertex.location, center));
                for (int i = 0; i < oldFaces.Count; i++)
                {
                    double overVectorNorm = findOverVectorNorm(vertexOutVector, currentVertex, oldFaces[i]);
                    if (overVectorNorm < minOverVectorNorm)
                    {
                        overFace = oldFaces[i];
                        minOverVectorNorm = overVectorNorm;
                    }
                }
                if (overFace.vertices.Contains(currentVertex)) Console.WriteLine("");
                replaceOneFace((List<IFaceConvHull>)oldFaces, overFace, currentVertex);
                maximizeHullFaces(oldFaces);//, 3);
                faces.AddRange(oldFaces);
            }
        }

        private static double findOverVectorNorm(double[] vertexOutVector, IVertexConvHull currentVertex, IFaceConvHull face)
        {
            var result = new double[dimension];
            foreach (var v in face.vertices)
                result = StarMath.add(result, StarMath.subtract(currentVertex.location, v.location));
            result = StarMath.normalize(result);
            return StarMath.norm2(StarMath.subtract(vertexOutVector, result));
        }

        private static void replaceTwoFaces(List<IFaceConvHull> faces, IList<IFaceConvHull> oldFaces, IVertexConvHull v)
        {
            IVertexConvHull vFrom, vTo;
            if (shareEdge(oldFaces[0], oldFaces[1], out vFrom, out vTo))
            {
                faces.Remove(oldFaces[0]);
                faces.Remove(oldFaces[1]);
                IVertexConvHull v0Diff = ConvexHull.findNonSharedVertex(oldFaces[0], vFrom, vTo);
                IVertexConvHull v1Diff = ConvexHull.findNonSharedVertex(oldFaces[1], vFrom, vTo);
                faces.Add(MakeFace3D(v, v0Diff, vFrom));
                faces.Add(MakeFace3D(v, vFrom, v1Diff));
                faces.Add(MakeFace3D(v, v1Diff, vTo));
                faces.Add(MakeFace3D(v, vTo, v0Diff));
            }
            else throw new Exception("how is this possible?");
        }

        private static void replaceOneFace(List<IFaceConvHull> faces, IFaceConvHull oldFace, IVertexConvHull v)
        {
            faces.Remove(oldFace);
            faces.Add(MakeFace3D(v, oldFace.vertices[0], oldFace.vertices[1]));
            faces.Add(MakeFace3D(v, oldFace.vertices[1], oldFace.vertices[2]));
            faces.Add(MakeFace3D(v, oldFace.vertices[2], oldFace.vertices[0]));
        }

        /// <summary>
        /// Checks if faces are the same.
        /// </summary>
        /// <param name="f1">The f1.</param>
        /// <param name="f2">The f2.</param>
        /// <returns></returns>
        private static Boolean sameFace(IFaceConvHull f1, IFaceConvHull f2)
        {
            if (f1.Equals(f2)) return true;
            //if ((f1.normal[0] != f2.normal[0]) || (f1.normal[1] != f2.normal[1]) || (f1.normal[2] != f2.normal[2]))
            //    return false;
            if (!(f1.vertices[0].Equals(f2.vertices[0]) || f1.vertices[0].Equals(f2.vertices[1]) || f1.vertices[0].Equals(f2.vertices[2]))) return false;
            if (!(f1.vertices[1].Equals(f2.vertices[0]) || f1.vertices[1].Equals(f2.vertices[1]) || f1.vertices[1].Equals(f2.vertices[2]))) return false;
            if (!(f1.vertices[2].Equals(f2.vertices[0]) || f1.vertices[2].Equals(f2.vertices[1]) || f1.vertices[2].Equals(f2.vertices[2]))) return false;
            return true;
        }



        private static double distanceToFaceCenter(IVertexConvHull v, IFaceConvHull f)
        {
            var c = new double[dimension];
            for (int i = 0; i < dimension; i++)
                for (int j = 0; j < dimension; j++)
                    c[j] = f.vertices[i].location[j];
            return StarMath.norm2(c, v.location);
        }

        private static void maximizeHullFaces(IList<IFaceConvHull> convexFaces, int numberNew = -1)
        {
            if (numberNew == 0) return;
            if (numberNew < 0) numberNew = convexFaces.Count;
            int lastNew = convexFaces.Count - numberNew;
            /* While these vertices are clearly part of the hull, the faces may not be. Now we quickly run through the
             * faces to identify if they neighbor with a non-convex face. This can be determined by taking the cross-
             * product of the normals of the two faces. If the direction of the resulting vector, c, is not aligned
             * with the direction of the first face's edge vector (the one shared with the other face) then we need
             * to rearrange the faces - essentially we change the faces from the 2 offending faces to the other two
             * that make up the simplex (tetrahedron) shape defined by the four vertices. */
            for (int i = convexFaces.Count - 1; i >= lastNew; i--)
            {
                numberNew--;
                for (int j = i - 1; j >= 0; j--)
                {
                    IVertexConvHull vFrom, vTo;
                    if (shareEdge(convexFaces[i], convexFaces[j], out vFrom, out vTo))
                    {
                        IVertexConvHull viDiff = ConvexHull.findNonSharedVertex(convexFaces[i], vFrom, vTo);
                        IVertexConvHull vjDiff = ConvexHull.findNonSharedVertex(convexFaces[j], vFrom, vTo);
                        var newFace1 = MakeFace3D(viDiff, vjDiff, vTo);
                        var newFace2 = MakeFace3D(vjDiff, viDiff, vFrom);
                        if ((simplexVolumeFromCenter(newFace1) + simplexVolumeFromCenter(newFace2)) >
                           (simplexVolumeFromCenter(convexFaces[i]) + simplexVolumeFromCenter(convexFaces[j])))
                        {
                            convexFaces.RemoveAt(i);
                            convexFaces.RemoveAt(j);
                            //if (!convexFaces.Exists(f => sameFace(f, newFace1)))
                            convexFaces.Add(newFace1);
                            //else Console.Write("The face already exists. But this is a symptom of something else. What? I don't know.");
                            //if (!convexFaces.Exists(f => sameFace(f, newFace2)))
                            convexFaces.Add(newFace2);
                            //else Console.Write("The face already exists. But this is a symptom of something else. What? I don't know.");
                            numberNew += 2;
                            maximizeHullFaces(convexFaces);//, numberNew);
                            return;
                        }
                    }
                }
            }
        }


        private static double simplexVolumeFromCenter(IFaceConvHull face)
        {
            double[,] simplex = new double[dimension, dimension];
            for (int i = 0; i < dimension; i++)
                StarMath.SetColumn(i, simplex, StarMath.subtract(face.vertices[i].location, center));
            return StarMath.determinant(simplex);
        }

        private static Boolean sameDirection(double[] c, double[] p)
        {
            if (c.GetLength(0) != p.GetLength(0)) return false;
            for (int i = 0; i < c.GetLength(0); i++)
                if (c[i] / p[i] > 0) return true;
                else if (c[i] / p[i] < 0) return false;
            return false;
        }



        /// <summary>
        /// Finds the vertex that is NOT shared (that is, not the two provided).
        /// </summary>
        /// <param name="iFaceConvHull">The i face conv hull.</param>
        /// <param name="vFrom">The shared vertex,  vFrom.</param>
        /// <param name="vTo">The shared vertex, vTo.</param>
        /// <returns></returns>
        private static IVertexConvHull findNonSharedVertex(IFaceConvHull iFaceConvHull, IVertexConvHull vFrom, IVertexConvHull vTo)
        {
            if (!iFaceConvHull.vertices[0].Equals(vFrom) && !iFaceConvHull.vertices[0].Equals(vTo))
                return iFaceConvHull.vertices[0];
            if (!iFaceConvHull.vertices[1].Equals(vFrom) && !iFaceConvHull.vertices[1].Equals(vTo))
                return iFaceConvHull.vertices[1];
            return iFaceConvHull.vertices[2];

        }




        private static IFaceConvHull MakeFace(List<IVertexConvHull> convexHull, int notFaceIndex)
        {
            var outDir = new double[dimension];
            for (int i = 0; i < convexHull.Count; i++)
                if (i != notFaceIndex)
                    for (int j = 0; j < dimension; j++)
                        outDir[j] += convexHull[i].location[j] / dimension;
            for (int j = 0; j < dimension; j++)
                outDir[j] -= convexHull[notFaceIndex].location[j];

            var verts = new List<IVertexConvHull>();
            for (int i = 0; i < convexHull.Count; i++)
                if (i != notFaceIndex)
                    verts.Add(convexHull[i]);

            var f = MakeFace3D(verts);
            if (StarMath.multiplyDot(f.normal, outDir) < 0)
            {
                verts.Reverse();
                f = MakeFace3D(verts);
            }
            return f;
        }

        static Boolean overFace(IVertexConvHull v, IFaceConvHull f, out double dotP)
        {
            dotP = StarMath.multiplyDot(f.normal, StarMath.subtract(v.location, f.vertices[0].location));
            return (dotP >= 0);
        }

        /// <summary>
        /// Makes the face in 3D.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <returns>the newly created face</returns>
        static IFaceConvHull MakeFace3D(List<IVertexConvHull> vertices)
        { return MakeFace3D(vertices[0], vertices[1], vertices[2]); }

        static IFaceConvHull MakeFace3D(IVertexConvHull v1, IVertexConvHull v2, IVertexConvHull v3)
        {
            if (v1.Equals(v2) || v2.Equals(v3) || v3.Equals(v1)) return null;
            double[] n = StarMath.multiplyCross(StarMath.subtract(v2.location, v1.location),
                StarMath.subtract(v3.location, v2.location));
            var nMag = Math.Sqrt((n[0] * n[0]) + (n[1] * n[1]) + (n[2] * n[2]));
            if (nMag < epsilon) return null;
            n[0] /= nMag;
            n[1] /= nMag;
            n[2] /= nMag;

            IFaceConvHull newFace = null;
            if (faceType != null)
            {
                var constructor = faceType.GetConstructor(new Type[0]);
                newFace = (IFaceConvHull)constructor.Invoke(new object[0]);
            }
            if (newFace == null) newFace = new defFaceClass(dimension);

            newFace.vertices[0] = v1;
            newFace.vertices[1] = v2;
            newFace.vertices[2] = v3;
            newFace.normal = n;
            return newFace;
        }
    }
}