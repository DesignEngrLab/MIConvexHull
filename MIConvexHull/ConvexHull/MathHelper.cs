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

    /// <summary>
    /// A helper class mostly for normal computation. If convex hulls are computed
    /// in higher dimensions, it might be a good idea to add a specific
    /// FindNormalVectorND function.
    /// </summary>
    class MathHelper
    {
        readonly int Dimension;

        double[] ntX, ntY, ntZ;
        double[] nDNormalSolveVector;
        double[,] nDMatrix;
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

        void Normalize(double[] x)
        {
            double norm = 0;
            for (int i = 0; i < Dimension; i++)
            {
                var t = x[i];
                norm += t * t;
            }
            double f = 1.0 / Math.Sqrt(norm);
            for (int i = 0; i < Dimension; i++) x[i] *= f;
        }

        /// <summary>
        /// Subtracts vectors x and y and stores the result to target.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="target"></param>
        public void SubtractFast(double[] x, double[] y, double[] target)
        {
            for (int i = 0; i < Dimension; i++)
            {
                target[i] = x[i] - y[i];
            }
        }

        /// <summary>
        /// Finds 4D normal vector.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normal"></param>
        void FindNormalVector4D(VertexWrap[] vertices, double[] normal)
        {
            SubtractFast(vertices[1].PositionData, vertices[0].PositionData, ntX);
            SubtractFast(vertices[2].PositionData, vertices[1].PositionData, ntY);
            SubtractFast(vertices[3].PositionData, vertices[2].PositionData, ntZ);

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
        void FindNormalVector3D(VertexWrap[] vertices, double[] normal)
        {
            SubtractFast(vertices[1].PositionData, vertices[0].PositionData, ntX);
            SubtractFast(vertices[2].PositionData, vertices[1].PositionData, ntY);

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
        void FindNormalVector2D(VertexWrap[] vertices, double[] normal)
        {
            SubtractFast(vertices[1].PositionData, vertices[0].PositionData, ntX);

            var x = ntX;

            var nx = -x[1];
            var ny = x[0];

            double norm = System.Math.Sqrt(nx * nx + ny * ny);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
        }

        /// <summary>
        /// Finds normal vector of a hyper-plane given by vertices.
        /// Stores the results to normalData.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normalData"></param>
        public void FindNormalVector(VertexWrap[] vertices, double[] normalData)
        {
            switch (Dimension)
            {
                case 2: FindNormalVector2D(vertices, normalData); break;
                case 3: FindNormalVector3D(vertices, normalData); break;
                case 4: FindNormalVector4D(vertices, normalData); break;
                default:
                    {
                        for (var i = 0; i < Dimension; i++) nDNormalSolveVector[i] = 1.0;
                        for (var i = 0; i < Dimension; i++)
                        {
                            var row = jaggedNDMatrix[i];
                            var pos = vertices[i].Vertex.Position;
                            for (int j = 0; j < Dimension; j++) row[j] = pos[j];
                        }
                        GaussElimination(Dimension, jaggedNDMatrix, nDNormalSolveVector, normalData);
                        Normalize(normalData);
                        break;
                    }
            }
        }


        /// <summary>
        /// Check if the vertex is "visible" from the face.
        /// The vertex is "over face" if the return value is > Constants.PlaneDistanceTolerance.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="f"></param>
        /// <returns>The vertex is "over face" if the result is positive.</returns>
        public double GetVertexDistance(VertexWrap v, ConvexFaceInternal f)
        {
            double[] normal = f.Normal;
            double[] p = v.PositionData;
            double distance = f.Offset;
            for (int i = 0; i < Dimension; i++) distance += normal[i] * p[i];
            return distance;
        }

        public MathHelper(int dimension)
        {
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
            nDMatrix = new double[Dimension, Dimension];
        }
    }
}
