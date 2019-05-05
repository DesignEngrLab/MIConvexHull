using System;
using System.Windows;

namespace OuelletConvexHull
{
	public class PointInfo
	{
		// ************************************************************************
		public double X;
		public double Y;
		public double SlopeToNext;

		// ************************************************************************
		public PointInfo(double x, double y, double slopeToNext)
		{
			X = x;
			Y = y;
			SlopeToNext = slopeToNext;
		}

		// ************************************************************************
		public PointInfo(double x, double y)
		{
			X = x;
			Y = y;
			SlopeToNext = Double.NaN;
		}

		// ************************************************************************
		public PointInfo(Point pt)
		{
			X = pt.X;
			Y = pt.Y;
			SlopeToNext = Double.NaN;
		}

		// ************************************************************************
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if ((obj is PointInfo))
			{
				// Want to know if really equals, no epsilon here.
				if (X == ((PointInfo) obj).X && Y == ((PointInfo) obj).Y)
				{
					return true;
				}
			}

			Point pt = (Point) obj;
			return (pt.X == X) && (pt.Y == Y);
		}

		// ************************************************************************
		public bool Equals(PointInfo dpi)
		{
			// Want to know if really equals, no epsilon here.
			if (X == dpi.X && Y == dpi.Y)
			{
				return true;
			}

			return false;
		}

		// ************************************************************************
		public override int GetHashCode()
		{
			// Not perfect but if should do the job
			return X.GetHashCode() + Y.GetHashCode();
		}

		// ************************************************************************
		public override string ToString()
		{
			return "(" + X + " | " + Y + ")";
		}

		// ************************************************************************
		public Point ToPoint()
		{
			return new Point(X, Y);
		}

		// ************************************************************************
	}
}
