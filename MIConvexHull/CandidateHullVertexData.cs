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

    internal class CandidateHullFaceData
    {
        public IFaceConvHull face;
        public SortedList<double, IVertexConvHull> verticesBeyond;

        public CandidateHullFaceData(IFaceConvHull face, IVertexConvHull currentVertex, double k)
        {
            this.face = face;
            verticesBeyond = new SortedList<double, IVertexConvHull>(new noEqualSortMaxtoMinDouble());
            verticesBeyond.Add(k, currentVertex);
        }

        public CandidateHullFaceData(IFaceConvHull face)
        {
            this.face = face;
        }
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
