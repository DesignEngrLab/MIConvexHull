using System.Collections.Generic;

namespace MIConvexHullPluginNameSpace
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
}
