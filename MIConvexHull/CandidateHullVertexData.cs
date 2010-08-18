using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIConvexHullPluginNameSpace
{
    internal class CandidateHullVertexData
    {
        public IVertexConvHull vertex;
        public SortedList<double, IFaceConvHull> otherFaces;
    }


    internal class noEqualSortMaxtoMinDouble : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            if (x > y) return -1;
            else return 1;
        }
    }

    internal class noEqualSortMaxtoMinInt : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x > y) return -1;
            else return 1;
        }
    }
}
