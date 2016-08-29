using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIConvexHull;
using Xunit;

namespace UnitTesting
{
    public class TestSTLs
    {

        [Fact]
        public void PassingTest()
        {
            List<DefaultVertex> vertices;
            var v3D = Presenter.MakeModelVisual3D("../../../TestFiles/cvxHull.stl", out vertices);
            var convexHull = MIConvexHull.ConvexHull.Create(vertices);
            Presenter.ShowWithConvexHull(v3D, convexHull);
        }

        [Fact]
        public void FailingTest()
        {
            Assert.Equal(5, Add(2, 2));
        }

        int Add(int x, int y)
        {
            return x + y;
        }
    }
}
