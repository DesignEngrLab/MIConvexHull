using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIConvexHull
{
    class VertexSort:IComparer<VertexWrap>
    {
        private readonly int dimension;
        public List<VertexWrap> Duplicates { get; private set; } 

        public VertexSort(int Dimension)
        {
            // TODO: Complete member initialization
            this.dimension = Dimension;
            Duplicates = new List<VertexWrap>();
        }
        public int Compare(VertexWrap x, VertexWrap y)
        {
            for (int i = 0; i < dimension; i++)
            {
                if (x.PositionData[i] < y.PositionData[i]) return -1;
                if (x.PositionData[i] > y.PositionData[i]) return 1;
            }
            if (isANewDuplicate(x)) Duplicates.Add(x);
            return 0;
        }

        private bool isANewDuplicate(VertexWrap x)
        {
            for (int i = 0; i < Duplicates.Count; i++)
            {
                if (SamePosition(x.PositionData, Duplicates[i].PositionData, dimension))
                    return false;
            }
            return true;
        }
        public static bool SamePosition(double[] pt1, double[] pt2, int dimension)
        {
            return (StarMath.norm2(StarMath.subtract(pt1,pt2,dimension), dimension,true) < Constants.epsilonSquared);
        }

    }
}
