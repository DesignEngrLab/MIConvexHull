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

    /// <summary>
    /// functions called from Find for the 3D case. 
    /// </summary>
    public static partial class ConvexHull
    {
        /// <summary>
        /// Sumproduct of the i,j,k values with the vertex position.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <param name="k">The k.</param>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private static double sumproduct(int i, int j, int k, IVertexConvHull n)
        {
            return ((i - 1) * n.X + (j - 1) * n.Y + (k - 1) * n.Z);
        }


        /// <summary>
        /// The cross product of the two 3D double vectors a and b
        /// </summary>
        /// <param name="aX">X-component of the a vector.</param>
        /// <param name="aY">Y-component of the a vector.</param>
        /// <param name="aZ">Z-component of the a vector.</param>
        /// <param name="bX">X-component of the b vector.</param>
        /// <param name="bY">Y-component of the b vector.</param>
        /// <param name="bZ">Z-component of the b vector.</param>
        /// <returns>A IVertexConvHull of the resulting vector.</returns>
        internal static double[] crossProduct(double aX, double aY, double aZ,
            double bX, double bY, double bZ)
        {
            return new double[] {
                ((aY * bZ) - (bY * aZ)),
                ((aZ * bX) - (bZ * aX)),
                ((aX * bY) - (bX * aY))};
        }


        /// <summary>
        /// The dot product of the two 3D double vectors a and b
        /// </summary>
        /// <param name="aX">X-component of the a vector.</param>
        /// <param name="aY">Y-component of the a vector.</param>
        /// <param name="aZ">Z-component of the a vector.</param>
        /// <param name="bX">X-component of the b vector.</param>
        /// <param name="bY">Y-component of the b vector.</param>
        /// <param name="bZ">Z-component of the b vector.</param>
        /// <returns>A double value that contains the dot product</returns>
        private static double dotProduct(double aX, double aY, double aZ, double bX, double bY, double bZ)
        {
            return (aX * bX) + (aY * bY) + (aZ * bZ);
        }

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
            Boolean result = false;
            if (f1.v1.Equals(f2.v1) || f1.v1.Equals(f2.v2) || f1.v1.Equals(f2.v3))
                vFrom = f1.v1;
            if (f1.v2.Equals(f2.v1) || f1.v2.Equals(f2.v2) || f1.v2.Equals(f2.v3))
            {
                if (vFrom != null)
                {
                    vTo = f1.v2;
                    result = true;
                }
                else if (f1.v3.Equals(f2.v1) || f1.v3.Equals(f2.v2) || f1.v3.Equals(f2.v3))
                {
                    vFrom = f1.v2;
                    vTo = f1.v3;
                    result = true;
                }
            }
            else if ((vFrom != null) && (f1.v3.Equals(f2.v1) || f1.v3.Equals(f2.v2) || f1.v3.Equals(f2.v3)))
            {
                vTo = vFrom;
                vFrom = f1.v3;
                result = true;
            }
            return result;
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
            if (!(f1.v1.Equals(f2.v1) || f1.v1.Equals(f2.v2) || f1.v1.Equals(f2.v3))) return false;
            if (!(f1.v2.Equals(f2.v1) || f1.v2.Equals(f2.v2) || f1.v2.Equals(f2.v3))) return false;
            if (!(f1.v3.Equals(f2.v1) || f1.v3.Equals(f2.v2) || f1.v3.Equals(f2.v3))) return false;
            return true;
        }

        ///// <summary>
        /// Fixes the non-convex faces.
        /// </summary>
        /// <param name="convexFaces">The convex faces.</param>
        /// <param name="numberToCheck">The number to check.</param>
        private static void FixNonConvexFaces(List<IFaceConvHull> convexFaces, int numberToCheck = -1)
        {
            int lastNew = convexFaces.Count - numberToCheck;
            if (numberToCheck < 0) lastNew = 0;
            /* While these vertices are clearly part of the hull, the faces may not be. Now we quickly run through the
             * faces to identify if they neighbor with a non-convex face. This can be determined by taking the cross-
             * product of the normals of the two faces. If the direction of the resulting vector, c, is not aligned
             * with the direction of the first face's edge vector (the one shared with the other face) then we need
             * to rearrange the faces - essentially we change the faces from the 2 offending faces to the other two
             * that make up the simplex (tetrahedron) shape defined by the four vertices. */

            for (int i = convexFaces.Count - 1; i >= lastNew; i--)
            {
                numberToCheck--;
                for (int j = i - 1; j >= 0; j--)
                {
                    IVertexConvHull vFrom, vTo;
                    if (sameFace(convexFaces[i], convexFaces[j])) throw new Exception("Here I am! Your two-week bug. You shouldn't have two" +
                        " identical faces in the convex hull. Please fix.");
                    if (shareEdge(convexFaces[i], convexFaces[j], out vFrom, out vTo))
                    {
                        var c = crossProduct(convexFaces[i].normal[0], convexFaces[i].normal[1], convexFaces[i].normal[2],
                            convexFaces[j].normal[0], convexFaces[j].normal[1], convexFaces[j].normal[2]);
                        if (!sameDirection(c, new double[] { (vTo.X - vFrom.X), (vTo.Y - vFrom.Y), (vTo.Z - vFrom.Z) }))
                        {
                            IVertexConvHull viDiff = ConvexHull.findNonSharedVertex(convexFaces[i], vFrom, vTo);
                            IVertexConvHull vjDiff = ConvexHull.findNonSharedVertex(convexFaces[j], vFrom, vTo);
                            var newFace1 = MakeFace3D(viDiff, vjDiff, vTo);
                            var newFace2 = MakeFace3D(vjDiff, viDiff, vFrom);
                            if ((newFace1 == null) || (newFace2 == null))
                                throw new Exception("One of both faces are null.");
                            convexFaces.Add(newFace1);
                            convexFaces.Add(newFace2);
                            numberToCheck += 3;
                            convexFaces.RemoveAt(i);
                            convexFaces.RemoveAt(j);
                            FixNonConvexFaces(convexFaces, numberToCheck);
                            return;
                        }
                    }
                }
            }
        }

        private static Boolean sameDirection(double[] c, double[] p)
        {
            if (c.GetLength(0) != p.GetLength(0)) return false;
            for (int i = 0; i < c.GetLength(0); i++)
                if (c[i] / p[i] > 0) return true;
                else if (c[i] / p[i] < 0) return false;
            return false;
        }


        private static Boolean faceContainsVertex(IFaceConvHull face, IVertexConvHull vertex)
        {
            var bX = vertex.X - face.v1.X;
            var bY = vertex.Y - face.v1.Y;
            var bZ = vertex.Z - face.v1.Z;
            var point = crossProduct(face.normal[0], face.normal[1], face.normal[2], bX, bY, bZ);
            point = crossProduct(point[0], point[1], point[2], face.normal[0], face.normal[1], face.normal[2]);

            if (!sameDirection(crossProduct(face.v2.X - face.v1.X, face.v2.Y - face.v1.Y, face.v2.Z - face.v1.Z,
                point[0], point[1], point[2]), face.normal))
                return false;
            point = new double[] { point[0] + face.v1.X, point[1] + face.v1.Y, point[2] + face.v1.Z };
            if (!sameDirection(crossProduct(face.v3.X - face.v2.X, face.v3.Y - face.v2.Y, face.v3.Z - face.v2.Z,
                 point[0] - face.v2.X, point[1] - face.v2.Y, point[2] - face.v2.Z), face.normal))
                return false;
            if (!sameDirection(crossProduct(face.v1.X - face.v3.X, face.v1.Y - face.v3.Y, face.v1.Z - face.v3.Z,
                 point[0] - face.v3.X, point[1] - face.v3.Y, point[2] - face.v3.Z), face.normal))
                return false;
            return true;
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
            if (!iFaceConvHull.v1.Equals(vFrom) && !iFaceConvHull.v1.Equals(vTo))
                return iFaceConvHull.v1;
            if (!iFaceConvHull.v2.Equals(vFrom) && !iFaceConvHull.v2.Equals(vTo))
                return iFaceConvHull.v2;
            return iFaceConvHull.v3;

        }


        /// <summary>
        /// Replace IFaceConvHull, j, in the list with three new faces.
        /// </summary>
        /// <param name="faces">The convex faces.</param>
        /// <param name="j">The index, j.</param>
        /// <param name="IVertexConvHull">The IVertexConvHull.</param>
        private static void replaceFace(List<IFaceConvHull> faces, int j, IVertexConvHull IVertexConvHull)
        {
            var oldFace = faces[j];
            faces.RemoveAt(j);
            var f1 = MakeFace3D(IVertexConvHull, oldFace.v1, oldFace.v2);
            var f2 = MakeFace3D(IVertexConvHull, oldFace.v2, oldFace.v3);
            var f3 = MakeFace3D(IVertexConvHull, oldFace.v3, oldFace.v1);
            if ((f1 == null) || (f2 == null) || (f3 == null))
                throw new Exception();
            //if (dotProduct(oldFace.normal[0], oldFace.normal[1], oldFace.normal[2], f1.normal[0], f1.normal[1], f1.normal[2]) < 0)
            //    throw new Exception();
            //if (dotProduct(oldFace.normal[0], oldFace.normal[1], oldFace.normal[2], f2.normal[0], f2.normal[1], f2.normal[2]) < 0)
            //    throw new Exception();
            //if (dotProduct(oldFace.normal[0], oldFace.normal[1], oldFace.normal[2], f3.normal[0], f3.normal[1], f3.normal[2]) < 0)
            //    throw new Exception();
            faces.Add(f1); faces.Add(f2); faces.Add(f3);
            //faces.Add(MakeFace(IVertexConvHull, oldFace.v1, oldFace.v2));
            //faces.Add(MakeFace(IVertexConvHull, oldFace.v2, oldFace.v3));
            //faces.Add(MakeFace(IVertexConvHull, oldFace.v3, oldFace.v1));
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
            if (dotProduct(f.normal[0], f.normal[1], f.normal[2], outDir[0], outDir[1], outDir[2]) < 0)
            {
                verts.Reverse();
                f = MakeFace3D(verts);
                if (dotProduct(f.normal[0], f.normal[1], f.normal[2], outDir[0], outDir[1], outDir[2]) < 0)
                    throw new Exception("both negative?!?");
            }
            return f;
        }

        static Boolean overFace(IVertexConvHull v, IFaceConvHull f, out double dotP)
        {
            var bX = v.X - f.v1.X;
            var bY = v.Y - f.v1.Y;
            var bZ = v.Z - f.v1.Z;
            dotP = dotProduct(f.normal[0], f.normal[1], f.normal[2], bX, bY, bZ);
            return (dotP >= 0);
        }

        /// <summary>
        /// Makes a new face.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="v3">The v3.</param>
        /// <returns></returns>
        public static IFaceConvHull MakeFace3D(List<IVertexConvHull> vertices)
        { return MakeFace3D(vertices[0], vertices[1], vertices[2]); }
        public static IFaceConvHull MakeFace3D(IVertexConvHull v1, IVertexConvHull v2, IVertexConvHull v3)
        {
            if (v1.Equals(v2) || v2.Equals(v3) || v3.Equals(v1)) return null;
            double[] n = crossProduct(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z,
                v3.X - v2.X, v3.Y - v2.Y, v3.Z - v2.Z);
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
            if (newFace == null) newFace = new defFaceClass();

            newFace.v1 = v1;
            newFace.v2 = v2;
            newFace.v3 = v3;
            newFace.normal = n;
            //newFace.center = new double[]{((v1.X + v2.X + v3.X) / 3),
            //        ((v1.Y + v2.Y + v3.Y) / 3),
            //        ((v1.Z + v2.Z + v3.Z) / 3)};
            return newFace;
        }
    }
}