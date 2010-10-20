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
namespace TestEXE_for_MIConvexHull2D
{
    using System;
    using MIConvexHullPluginNameSpace;


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
            //var vertices = new List<IVertexConvHull>();
            //for (int i = 0; i < NumberOfVertices; i++)
            //    vertices.Add(new vertex(size * r.NextDouble(), size * r.NextDouble()));

            var vertices = new object[NumberOfVertices];
            for (var i=0; i<NumberOfVertices; i++)
                vertices[i] = new vertex(size * r.NextDouble(), size * r.NextDouble());
            Console.WriteLine("Running...");
            var now = DateTime.Now;
            ConvexHull.InputVertices(vertices);
            var convexHullVertices = ConvexHull.FindConvexHull();
            var interval = DateTime.Now - now;
            Console.WriteLine("Out of the " + NumberOfVertices + " vertices, there are " +
                convexHullVertices.Count + " in the convex hull.");
            Console.WriteLine("time = " + interval);
            Console.ReadLine();
        }


    }
}
