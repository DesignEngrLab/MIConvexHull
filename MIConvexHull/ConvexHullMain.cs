/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2006, 2010 Matthew Ira Campbell, PhD.
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
        static SortedList<double, FaceData> convexFaces;
        static int dimension;
        static Type faceType;
        static double[] center;

        static void Initialize(int dimensions)
        {
            dimension = dimensions;
            convexHull = new List<IVertexConvHull>();
            convexFaces = new SortedList<double, FaceData>(new noEqualSortMaxtoMinDouble());
            faceType = null;
            center = new double[dimension];
        }
    }
}