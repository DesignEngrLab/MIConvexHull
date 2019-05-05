using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace OuelletConvexHull
{
	public abstract class Quadrant
	{
		// ************************************************************************
		public Point FirstPoint;
		public Point LastPoint;
		public Point RootPoint;

		public readonly List<Point> HullPoints = null;
		protected IReadOnlyList<Point> _listOfPoint;

		// ************************************************************************
		// Very important the Quadrant should be always build in a way where dpiFirst has minus slope to center and dpiLast has maximum slope to center
		public Quadrant(IReadOnlyList<Point> listOfPoint, int initialResultGuessSize)
		{
			_listOfPoint = listOfPoint;
			HullPoints = new List<Point>(initialResultGuessSize);
		}

		// ************************************************************************
		/// <summary>
		/// Initialize every values needed to extract values that are parts of the convex hull.
		/// This is where the first pass of all values is done the get maximum in every directions (x and y).
		/// </summary>
		protected abstract void SetQuadrantLimits();

		// ************************************************************************
		public void Calc(bool isSkipSetQuadrantLimits = false)
		{
			if (!_listOfPoint.Any())
			{
				// There is no points at all. Hey don't try to crash me.
				return;
			}

			if (!isSkipSetQuadrantLimits)
			{
				SetQuadrantLimits();
			}

			// Begin : General Init
			HullPoints.Add(FirstPoint);
			if (FirstPoint.Equals(LastPoint))
			{
				return; // Case where for weird distribution (like triangle or diagonal) there could be one or more quadrants without points.
			}
			HullPoints.Add(LastPoint);

			// Main Loop to extract ConvexHullPoints
			foreach (Point point in _listOfPoint)
			{
				if (!IsGoodQuadrantForPoint(point))
				{
					continue;
				}

				int indexLow = TryAdd(point.X, point.Y);

				if (indexLow == -1)
				{
					continue;
				}

				Point p1 = HullPoints[indexLow];
				Point p2 = HullPoints[indexLow + 1];

				if (!IsPointToTheRightOfOthers(p1, p2, point))
				{
					continue;
				}

				int indexHi = indexLow + 1;

				// Find lower bound (remove point invalidate by the new one that come before)
				while (indexLow > 0)
				{
					if (IsPointToTheRightOfOthers(HullPoints[indexLow - 1], point, HullPoints[indexLow]))
					{
						break; // We found the lower index limit of points to keep. The new point should be added right after indexLow.
					}
					indexLow--;
				}

				// Find upper bound (remove point invalidate by the new one that come after)
				int maxIndexHi = HullPoints.Count - 1;
				while (indexHi < maxIndexHi)
				{
					if (IsPointToTheRightOfOthers(point, HullPoints[indexHi + 1], HullPoints[indexHi]))
					{
						break; // We found the higher index limit of points to keep. The new point should be added right before indexHi.
					}
					indexHi++;
				}

				if (indexLow + 1 == indexHi)
				{
					// Insert Point
					HullPoints.Insert(indexHi, point);
				}
				else
				{
					HullPoints[indexLow + 1] = point;

					// Remove any invalidated points if any
					if (indexLow + 2 < indexHi)
					{
						HullPoints.RemoveRange(indexLow + 2, indexHi - indexLow - 2);
					}
				}

			}
		}

		// ************************************************************************
		/// <summary>
		/// To know if to the right. It is meaninful when p1 is first and p2 is next.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="ptToCheck"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool IsPointToTheRightOfOthers(Point p1, Point p2, Point ptToCheck)
		{
			return ((p2.X - p1.X) * (ptToCheck.Y - p1.Y)) - ((p2.Y - p1.Y) * (ptToCheck.X - p1.X)) < 0;
		}

		// ************************************************************************
		/// <summary>
		/// Tell if should try to add and where. -1 ==> Should not add.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected abstract int TryAdd(double x, double y);

		// ************************************************************************
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract bool IsGoodQuadrantForPoint(Point pt);

		// ************************************************************************

	}
}
