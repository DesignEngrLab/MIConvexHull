using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OuelletConvexHull
{
	public class Limit
	{
		// ************************************************************************
		public Point Q1Top, Q2Top, Q2Left, Q3Left, Q3Bottom, Q4Bottom, Q4Right, Q1Right;

		// ************************************************************************
		public Limit(Point pt)
		{
			Q1Top = pt;
			Q2Top = pt;
			Q2Left = pt;
			Q3Left = pt;
			Q3Bottom = pt;
			Q4Bottom =pt;
			Q4Right = pt;
			Q1Right = pt;
		}

		// ************************************************************************
		private Limit()
		{
			
		}

		// ************************************************************************
		public Limit Copy()
		{
			Limit limit = new Limit();

			limit.Q1Top = Q1Top;
			limit.Q2Top = Q2Top;
			limit.Q2Left = Q2Left;
			limit.Q3Left = Q3Left;
			limit.Q3Bottom = Q3Bottom;
			limit.Q4Bottom = Q4Bottom;
			limit.Q4Right = Q4Right;
			limit.Q1Right = Q1Right;

			return limit;
		}

		// ************************************************************************
	}
}
