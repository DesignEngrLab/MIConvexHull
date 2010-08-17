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


    internal class noEqualSortMaxtoMinInt : IEqualityComparer<int>
    {
        public bool Equals(int x, int y)
        {
            return false;
        }


        public int GetHashCode(int x)
        {
            return -x;
        }

    }
}
