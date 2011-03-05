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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace ExampleWithGraphics
{
    using MIConvexHull;
    using System.Windows.Media;
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class vertex : Shape, IVertexConvHull
    {

        protected override Geometry DefiningGeometry
        {
            get
            {
                return new EllipseGeometry
                {
                    Center = new Point(coordinates[0], coordinates[1]),
                    RadiusX = 2,
                    RadiusY = 2
                };
            }
        }
        public vertex()
        {
            Fill = Brushes.Black;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="vertex"/> class.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        public vertex(double x, double y)
            : this()
        {
            coordinates = new[] { x, y };
        }


        /// <summary>
        /// Gets or sets the Z. Not used by MIConvexHull2D.
        /// </summary>
        /// <value>The Z position.</value>
        // private double Z { get; set; }

        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        public double[] coordinates { get; set; }
        //{
        //    get { return new[] { X, Y}; }
        //    set
        //    {
        //        X = value[0];
        //        Y = value[1];
        //       // Z = value[2];
        //    }
        //}
    }
}
