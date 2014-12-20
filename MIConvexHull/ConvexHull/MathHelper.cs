/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2014 David Sehnal, Matthew Campbell
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

    /// <summary>
    /// A helper class mostly for normal computation. If convex hulls are computed
    /// in higher dimensions, it might be a good idea to add a specific
    /// FindNormalVectorND function.
    /// </summary>
    class MathHelper
    {
        readonly int Dimension;

        double[] PositionData;

        double[] ntX, ntY, ntZ;
        double[] nDNormalSolveVector;
        double[][] jaggedNDMatrix;

        /// <summary>
        /// does gaussian elimination.
        /// </summary>
        /// <param name="nDim"></param>
        /// <param name="pfMatr"></param>
        /// <param name="pfVect"></param>
        /// <param name="pfSolution"></param>
        static void GaussElimination(int nDim, double[][] pfMatr, double[] pfVect, double[] pfSolution)
        {
            double fMaxElem;
            double fAcc;

            int i, j, k, m;

            for (k = 0; k < (nDim - 1); k++) // base row of matrix
            {
                var rowK = pfMatr[k];

                // search of line with max element
                fMaxElem = Math.Abs(rowK[k]);
                m = k;
                for (i = k + 1; i < nDim; i++)
                {
                    if (fMaxElem < Math.Abs(pfMatr[i][k]))
                    {
                        fMaxElem = pfMatr[i][k];
                        m = i;
                    }
                }

                // permutation of base line (index k) and max element line(index m)                
                if (m != k)
                {
                    var rowM = pfMatr[m];
                    for (i = k; i < nDim; i++)
                    {
                        fAcc = rowK[i];
                        rowK[i] = rowM[i];
                        rowM[i] = fAcc;
                    }
                    fAcc = pfVect[k];
                    pfVect[k] = pfVect[m];
                    pfVect[m] = fAcc;
                }

                //if( pfMatr[k*nDim + k] == 0.0) return 1; // needs improvement !!!

                // triangulation of matrix with coefficients
                for (j = (k + 1); j < nDim; j++) // current row of matrix
                {
                    var rowJ = pfMatr[j];
                    fAcc = -rowJ[k] / rowK[k];
                    for (i = k; i < nDim; i++)
                    {
                        rowJ[i] = rowJ[i] + fAcc * rowK[i];
                    }
                    pfVect[j] = pfVect[j] + fAcc * pfVect[k]; // free member recalculation
                }
            }

            for (k = (nDim - 1); k >= 0; k--)
            {
                var rowK = pfMatr[k];
                pfSolution[k] = pfVect[k];
                for (i = (k + 1); i < nDim; i++)
                {
                    pfSolution[k] -= (rowK[i] * pfSolution[i]);
                }
                pfSolution[k] = pfSolution[k] / rowK[k];
            }
        }

        /// <summary>
        /// Squared length of the vector.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double LengthSquared(double[] x)
        {
            double norm = 0;
            for (int i = 0; i < x.Length; i++)
            {
                var t = x[i];
                norm += t * t;
            }
            return norm;
        }
        
        /// <summary>
        /// Subtracts vectors x and y and stores the result to target.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="target"></param>
        public void SubtractFast(int x, int y, double[] target)
        {
            int u = x * Dimension, v = y * Dimension;
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = PositionData[u + i] - PositionData[v + i];
            }
        }
        
        /// <summary>
        /// Finds 4D normal vector.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normal"></param>
        void FindNormalVector4D(int[] vertices, double[] normal)
        {
            SubtractFast(vertices[1], vertices[0], ntX);
            SubtractFast(vertices[2], vertices[1], ntY);
            SubtractFast(vertices[3], vertices[2], ntZ);

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

        /// <summary>
        /// Finds 3D normal vector.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normal"></param>
        void FindNormalVector3D(int[] vertices, double[] normal)
        {
            SubtractFast(vertices[1], vertices[0], ntX);
            SubtractFast(vertices[2], vertices[1], ntY);

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

        /// <summary>
        /// Finds 2D normal vector.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normal"></param>
        void FindNormalVector2D(int[] vertices, double[] normal)
        {
            SubtractFast(vertices[1], vertices[0], ntX);

            var x = ntX;

            var nx = -x[1];
            var ny = x[0];

            double norm = System.Math.Sqrt(nx * nx + ny * ny);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
        }
        
        void Normalize(double[] x)
        {
            double norm = 0;
            for (int i = 0; i < x.Length; i++)
            {
                var t = x[i];
                norm += t * t;
            }
            double f = 1.0 / Math.Sqrt(norm);
            for (int i = 0; i < x.Length; i++) x[i] *= f;
        }

        void FindNormalVectorND(int[] vertices, double[] normalData)
        {
            for (var i = 0; i < nDNormalSolveVector.Length; i++) nDNormalSolveVector[i] = 1.0;
            for (var i = 0; i < vertices.Length; i++)
            {
                var row = jaggedNDMatrix[i];
                var offset = vertices[i] * Dimension;
                for (int j = 0; j < row.Length; j++) row[j] = PositionData[offset + j];
            }
            GaussElimination(Dimension, jaggedNDMatrix, nDNormalSolveVector, normalData);
            Normalize(normalData);
        }
        
        /// <summary>
        /// Finds normal vector of a hyper-plane given by vertices.
        /// Stores the results to normalData.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normalData"></param>
        public void FindNormalVector(int[] vertices, double[] normalData)
        {
            switch (Dimension)
            {
                case 2: FindNormalVector2D(vertices, normalData); break;
                case 3: FindNormalVector3D(vertices, normalData); break;
                case 4: FindNormalVector4D(vertices, normalData); break;
                default: FindNormalVectorND(vertices, normalData); break;
            }
        }


        /// <summary>
        /// Calculates the normal and offset of the hyper-plane given by the face's vertices.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public bool CalculateFacePlane(ConvexFaceInternal face, double[] center)
        {
            var vertices = face.Vertices;
            var normal = face.Normal;
            FindNormalVector(vertices, normal);

            if (double.IsNaN(normal[0]))
            {
                return false;
            }

            double offset = 0.0;
            double centerDistance = 0.0;
            var fi = vertices[0] * Dimension; 
            for (int i = 0; i < Dimension; i++)
            {
                double n = normal[i];
                offset += n * PositionData[fi + i];
                centerDistance += n * center[i];
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
        /// Check if the vertex is "visible" from the face.
        /// The vertex is "over face" if the return value is > Constants.PlaneDistanceTolerance.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="f"></param>
        /// <returns>The vertex is "over face" if the result is positive.</returns>
        public double GetVertexDistance(int v, ConvexFaceInternal f)
        {
            double[] normal = f.Normal;
            int x = v * Dimension;
            double distance = f.Offset;
            for (int i = 0; i < normal.Length; i++) distance += normal[i] * PositionData[x + i];
            return distance;
        }

        /// <summary>
        /// Computes the volume of an n-dimensional simplex.
        /// Buffer needs to be array of shape Dimension x Dimension.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="vertices"></param>
        /// <param name="buffer">Needs to be array of shape Dimension x Dimension</param>
        /// <returns></returns>
        public static double GetSimplexVolume(ConvexFaceInternal cell, IList<IVertex> vertices, double[][] buffer)
        {
            var xs = cell.Vertices;
            var pivot = vertices[xs[0]].Position;
            double f = 1.0;
            for (int i = 1; i < xs.Length; i++)
            {
                f *= i + 1;
                var point = vertices[xs[i]].Position;
                for (int j = 0; j < point.Length; j++) buffer[i - 1][j] = point[j] - pivot[j];
            }

            return Math.Abs(DeterminantDestructive(buffer)) / f;
        }

        #region Determinants
        /// <summary>
        /// Modifies the matrix during the computation if the dimension > 3.
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static double DeterminantDestructive(double[][] A)
        {
            switch (A.Length)
            {
                case 0: return 0.0;
                case 1: return A[0][0];
                case 2: return (A[0][0] * A[1][1]) - (A[0][1] * A[1][0]);
                case 3: return (A[0][0] * A[1][1] * A[2][2])
                       + (A[0][1] * A[1][2] * A[2][0])
                       + (A[0][2] * A[1][0] * A[2][1])
                       - (A[0][0] * A[1][2] * A[2][1])
                       - (A[0][1] * A[1][0] * A[2][2])
                       - (A[0][2] * A[1][1] * A[2][0]);
                default: return DeterminantBigDestructive(A);
            }
        }

        static double DeterminantBigDestructive(double[][] A)
        {
            LUDecompositionInPlace(A);
            var result = 1.0;
            for (var i = 0; i < A.Length; i++)
                if (double.IsNaN(A[i][i]))
                    return 0;
                else result *= A[i][i];
            return result;
        }
        static void LUDecompositionInPlace(double[][] A)
        {
            int length = A.Length;
            // normalize row 0
            for (var i = 1; i < A.Length; i++) A[0][i] /= A[0][0];

            for (var i = 1; i < A.Length; i++)
            {
                for (var j = i; j < A.Length; j++)
                {
                    // do a column of L
                    var sum = 0.0;
                    for (var k = 0; k < i; k++)
                        sum += A[j][k] * A[k][i];
                    A[j][i] -= sum;
                }
                if (i == length - 1) continue;
                for (var j = i + 1; j < A.Length; j++)
                {
                    // do a row of U
                    var sum = 0.0;
                    for (var k = 0; k < i; k++) sum += A[i][k] * A[k][j];
                    A[i][j] = (A[i][j] - sum) / A[i][i];
                }
            }
        }

        #endregion

        public MathHelper(int dimension, double[] positions)
        {
            this.PositionData = positions;
            this.Dimension = dimension;

            ntX = new double[Dimension];
            ntY = new double[Dimension];
            ntZ = new double[Dimension];
            
            nDNormalSolveVector = new double[Dimension];
            jaggedNDMatrix = new double[Dimension][];
            for (var i = 0; i < Dimension; i++)
            {
                nDNormalSolveVector[i] = 1.0;
                jaggedNDMatrix[i] = new double[Dimension];
            }
        }
    }
}
