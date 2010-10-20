/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with MIConvexHull.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://miconvexhull.codeplex.com
 *************************************************************************/
namespace MIConvexHullPluginNameSpace
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// MIConvexHull.
    /// </summary>
    public static partial class ConvexHull
    {
        static List<IVertexConvHull> origVertices;
        static List<IVertexConvHull> convexHull;
        static List<IVertexConvHull> voronoiNodes;
        static List<Tuple<IVertexConvHull, IVertexConvHull>> voronoiEdges;
        static SortedList<double, FaceData> convexFaces;
        static List<FaceData> delaunayFaces;
        static int dimension;
        static double[] center;
        private const double coeffNumVertices = 0.25;
        private const double coeffDimensions = 2;
        private const double coeffOffset = 1250;
        const double minHeight = 0.00000000001;
        private static Boolean convexHullAnalysisComplete;
        private static Boolean delaunayAnalysisComplete;

        static void Initialize()
        {
            convexHull = new List<IVertexConvHull>();
            convexFaces = new SortedList<double, FaceData>(new noEqualSortMaxtoMinDouble());
            center = new double[dimension];
        }
    }
}