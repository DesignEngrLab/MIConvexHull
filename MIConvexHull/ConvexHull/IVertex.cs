
namespace MIConvexHull
{
    public interface IVertex
    {
        double[] Position { get; set; }
    }

    public class DefaultVertex : IVertex
    {
        public double[] Position { get; set; }
    }

}
