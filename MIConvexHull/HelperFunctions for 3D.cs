/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2006, 2008 Matthew Ira Campbell, PhD.
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
namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// functions called from Find for the 3D case. 
    /// </summary>
    public static partial class ConvexHull
    {
        const double epsilon = 0.0000001;
        static Type faceType;
        /// <summary>
        /// Sumproducts the specified k.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <param name="i">The i.</param>
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
        /// Determines whether [is non negative] [the specified k].
        /// </summary>
        /// <param name="k">The integer, k.</param>
        /// <returns></returns>
        private static int isNonNegative(int i)
        {
            if (i < 0) return -1;
            else return 1;
        }

        /// <summary>
        /// Cycles the specified k from 0 to 2 while avoiding the value at b.
        /// </summary>
        /// <param name="k">The integer, k.</param>
        /// <param name="b">The integer, b.</param>
        /// <returns></returns>
        private static int cycle(int i, int b = int.MinValue)
        {
            if (((i % 3) == b) || ((i % 3) == -b))
                i++;
            return Math.Abs(i % 3);
        }


        /// <summary>
        /// Replace IFaceConvHull, j, in the list with three new faces.
        /// </summary>
        /// <param name="faces">The convex faces.</param>
        /// <param name="i">The i.</param>
        /// <param name="IVertexConvHull">The IVertexConvHull.</param>
        private static void replaceFace(List<IFaceConvHull> faces, int j, IVertexConvHull IVertexConvHull)
        {
            var oldFace = faces[j];
            faces.RemoveAt(j);
            faces.Add(MakeFace(IVertexConvHull, oldFace.v1, oldFace.v2));
            faces.Add(MakeFace(IVertexConvHull, oldFace.v2, oldFace.v3));
            faces.Add(MakeFace(IVertexConvHull, oldFace.v3, oldFace.v1));
        }


        public static IFaceConvHull MakeFace(IVertexConvHull v1, IVertexConvHull v2, IVertexConvHull v3)
        {
            if (v1.Equals(v2) || v2.Equals(v3) || v3.Equals(v1)) return null;
            double[] n = crossProduct(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z,
                v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);
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
            newFace.center = new double[]{((v1.X + v2.X + v3.X) / 3),
                    ((v1.Y + v2.Y + v3.Y) / 3),
                    ((v1.Z + v2.Z + v3.Z) / 3)};
            return newFace;
        }
    }
}