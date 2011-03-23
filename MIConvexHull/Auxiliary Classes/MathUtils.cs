using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIConvexHull
{
    static class MathUtils
    {
        /// <summary>
        /// Same as StarMath multiplyDot, only without dimension check
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public static double multiplyDotFast(int[] A, double[] B, int dim)
        {
            var c = 0.0;
            for (var i = 0; i != dim; i++)
                c += A[i] * B[i];
            return c;
        }
        
        public static double multiplyDotFast(double[] A, double[] B, int dim)
        {
            var c = 0.0;
            for (var i = 0; i != dim; i++)
                c += A[i] * B[i];
            return c;
        }

        public static double subtractAndDot(double[] n, double[] l, double[] r, int dim)
        {
            double acc = 0;
            for (int i = 0; i < dim; i++)
            {
                double t = l[i] - r[i];
                acc += n[i] * t;
            }

            return acc;
        }

        public static double[] subtractFast(IList<double> A, IList<double> B, int dim)
        {
            var c = new double[dim];
            for (var i = 0; i != dim; i++)
                c[i] = A[i] - B[i];
            return c;
        }
    }
}
