using System;
using System.Collections.Generic;
using MIConvexHullPluginNameSpace;

namespace TestEXE_for_MIConvexHull_Benchmarking
{
    class Program
    {
        static void Main()
        {
            const int NumberOfVertices = 1000;
            const double size = 1000;
            const int dimension = 5;

            var r = new Random();
            Console.WriteLine("Ready? Push Return/Enter to start.");
            Console.ReadLine();

            Console.WriteLine("Making " + NumberOfVertices + " random vertices.");
            var vertices = new List<vertex>();
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var location = new double[dimension];
                for (var j = 0; j < dimension; j++)
                    location[j] = size * r.NextDouble();
                vertices.Add(new vertex(location));
            }
            Console.WriteLine("Running...");
            var now = DateTime.Now;
            ConvexHull.InputVertices(vertices);
            List<IVertexConvHull> vnodes;
            List<Tuple<IVertexConvHull, IVertexConvHull>> vedges;
            ConvexHull.FindVoronoiGraph(out vnodes, out vedges, typeof(vertex));
            var interval = DateTime.Now - now;
            Console.WriteLine("Out of the " + NumberOfVertices + " vertices, there are " +
                vnodes.Count + " voronoi points and " + vedges.Count + " voronoi edges.");
            Console.WriteLine("time = " + interval);
            Console.ReadLine();
        }
    }
}
