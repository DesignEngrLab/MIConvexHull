using System;
using System.Runtime.CompilerServices;

namespace OuelletConvexHull
{
	public class Geometry
	{
		// ******************************************************************
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double CalcSlope(double x1, double y1, double x2, double y2)
		{
			//if (Math.Abs(x2 - x1) <= Double.Epsilon)
			//{
			//	return Double.NaN;
			//}

			return (y2 - y1) / (x2 - x1);
		}

		// ******************************************************************
	}
}
