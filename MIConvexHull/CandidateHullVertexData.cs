using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIConvexHullPluginNameSpace
{
    internal class CandidateHullVertexData
    {
        public IVertexConvHull vertex;
      //  public int faceIndex;
        public SortedList<double, IFaceConvHull> otherFaces;
    }


    /// <summary>
    /// A comparer for optimization that can be used for either 
    /// minimization or maximization.
    /// </summary>
    internal class noEqualSort : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            if (x < y) return -1;
            else return 1;
        }
    }
}
