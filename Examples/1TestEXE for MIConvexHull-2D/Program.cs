﻿/*************************************************************************
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
namespace TestEXE_for_MIConvexHull2D
{
    using System;
    using MIConvexHull;
    using System.Collections.Generic;
    using System.Linq;


    static class Program
    {
        static void Main()
        {
            const int NumberOfVertices = 1000000;
            const double size = 1000;

            var r = new Random();
            Console.WriteLine("Ready? Push Return/Enter to start.");
            Console.ReadLine();

            Console.WriteLine("Making " + NumberOfVertices + " random vertices.");

            var vertices = new vertex[NumberOfVertices];
            for (var i = 0; i < NumberOfVertices; i++)
                vertices[i] = new vertex(size * r.NextDouble(), size * r.NextDouble());

            Console.WriteLine("Running...");
            var now = DateTime.Now;
            var convexHull = ConvexHull.Create(vertices).Points;
            var interval = DateTime.Now - now;
            Console.WriteLine("Out of the " + NumberOfVertices + " 2D vertices, there are " +
                convexHull.Count() + " on the convex hull.");
            Console.WriteLine("time = " + interval);

            Console.ReadLine();
        }
    }
}
