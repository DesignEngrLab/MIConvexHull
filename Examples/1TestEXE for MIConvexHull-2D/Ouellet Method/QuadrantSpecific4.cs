using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace OuelletConvexHull
{
	public class QuadrantSpecific4 : Quadrant
	{
		// ************************************************************************
		public QuadrantSpecific4(IReadOnlyList<Point> listOfPoint, int initialResultGuessSize) :
			base(listOfPoint, initialResultGuessSize)
		{
		}

		// ******************************************************************
		protected override void SetQuadrantLimits()
		{
			Point firstPoint = this._listOfPoint.First();

			double rightX = firstPoint.X;
			double rightY = firstPoint.Y;

			double bottomX = rightX;
			double bottomY = rightY;

			foreach (var point in _listOfPoint)
			{
				if (point.X >= rightX)
				{
					if (point.X == rightX)
					{
						if (point.Y < rightY)
						{
							rightY = point.Y;
						}
					}
					else
					{
						rightX = point.X;
						rightY = point.Y;
					}
				}

				if (point.Y <= bottomY)
				{
					if (point.Y == bottomY)
					{
						if (point.X > bottomX)
						{
							bottomX = point.X;
						}
					}
					else
					{
						bottomX = point.X;
						bottomY = point.Y;
					}
				}
			}

			FirstPoint = new Point(bottomX, bottomY);
			LastPoint = new Point(rightX, rightY);
			RootPoint = new Point(bottomX, rightY);
		}

		// ******************************************************************
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override bool IsGoodQuadrantForPoint(Point pt)
		{
			if (pt.X > this.RootPoint.X && pt.Y < this.RootPoint.Y)
			{
				return true;
			}

			return false;
		}

		// ******************************************************************
		protected override int TryAdd(double x, double y)
		{
			int indexLow = 0;
			int indexHi = HullPoints.Count - 1;

			while (indexLow != indexHi - 1)
			{
				int index = ((indexHi - indexLow) >> 1) + indexLow;

				if (x <= HullPoints[index].X && y >= HullPoints[index].Y)
				{
					return -1; // No calc needed
				}

				if (x > HullPoints[index].X)
				{
					indexLow = index;
					continue;
				}

				if (x < HullPoints[index].X)
				{
					indexHi = index;
					continue;
				}

				if (x == HullPoints[index].X)
				{
					if (y < HullPoints[index].Y)
					{
						indexLow = index;
					}
					else
					{
						return -1;
					}
				}

				break;
			}

			if (y >= HullPoints[indexHi].Y)
			{
				return -1; // Eliminated without slope calc
			}

			return indexLow;
		}

		// ******************************************************************
	}
}
