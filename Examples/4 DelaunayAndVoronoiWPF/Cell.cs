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
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ExampleWithGraphics
{
    using MIConvexHull;
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Cell : TriangulationCell<Vertex, Cell>
    {
        public class FaceVisual : Shape
        {
            Cell f;

            protected override Geometry DefiningGeometry
            {
                get
                {
                    var myPathGeometry = new PathGeometry();
                    var pathFigure1 = new PathFigure
                    {
                        StartPoint = new Point(f.Vertices[0].Position[0], f.Vertices[0].Position[1])
                    };
                    for (int i = 1; i < f.Vertices.GetLength(0); i++)
                        pathFigure1.Segments.Add(
                            new LineSegment(
                                new Point(f.Vertices[i].Position[0],
                                          f.Vertices[i].Position[1]), true));
                    pathFigure1.IsClosed = true;
                    myPathGeometry.Figures.Add(pathFigure1);


                    return myPathGeometry;
                }
            }

            public FaceVisual(Cell f)
            {
                Stroke = Brushes.Black;
                StrokeThickness = 1.0;
                Opacity = 0.5;
                this.f = f;
            }
        }

        Point GetCircumcenter()
        {
            var points = Vertices;

            double[,] m = new double[3, 3];

            // x, y, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 0] = points[i].Position[0];
                m[i, 1] = points[i].Position[1];
                m[i, 2] = 1;
            }
            var a = StarMath.determinant(m);

            // size, y, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 0] = StarMath.norm2(points[i].Position, true);
            }
            var dx = -StarMath.determinant(m);

            // size, x, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 1] = points[i].Position[0];
            }
            var dy = StarMath.determinant(m);

            // size, x, y
            for (int i = 0; i < 3; i++)
            {
                m[i, 2] = points[i].Position[1];
            }
            var c = -StarMath.determinant(m);
            
            var s = 1.0 / (2.0 * System.Math.Abs(a));
            var r = System.Math.Abs(s) * System.Math.Sqrt(dx * dx + dy * dy - 4 * a * c);
            return new Point(s * dx, s * dy);
        }

        public Shape Visual { get; private set; }
        Point? circumCenter;
        public Point Circumcenter
        {
            get
            {
                circumCenter = circumCenter ?? GetCircumcenter();
                return circumCenter.Value;
            }
        }

        public Cell()
        {
            Visual = new FaceVisual(this);
        }      
    }
}
