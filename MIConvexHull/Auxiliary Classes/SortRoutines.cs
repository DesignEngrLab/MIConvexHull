using System.Collections.Generic;

namespace MIConvexHull
{


    /// <summary>
    /// Sort doubles from highest to lowest.
    /// </summary>
    internal class noEqualSortMaxtoMinDouble : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            if (x > y) return -1;
            return 1;
        }
    }

    /// <summary>
    /// Sort integers from highest to lowest. 
    /// </summary>
    internal class noEqualSortMaxtoMinInt : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x > y) return -1;
            return 1;
        }
    }

    class lexComp : IComparer<IVertexConvHull>
    {
        int dim;

        public int Compare(IVertexConvHull vx, IVertexConvHull vy)
        {
            var x = vx.coordinates;
            var y = vy.coordinates;
            for (int i = 0; i < dim; i++)
            {
                if (x[i] < y[i]) return -1;
                if (x[i] > y[i]) return 1;
            }

            return 0;
        }

        public lexComp(int dim)
        {
            this.dim = dim;
        }
    }
}
