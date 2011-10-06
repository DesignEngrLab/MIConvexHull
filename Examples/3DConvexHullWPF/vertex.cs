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
namespace ExampleWithGraphics
{
    using MIConvexHull;
    using Petzold.Media3D;
    using System.Windows.Media.Media3D;
    using System.Windows.Media;
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class vertex : Sphere, IVertex
    {
        

        /// <summary>
        /// Initializes a new instance of the <see cref="vertex"/> class.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        public vertex(double x, double y, double z)
        {
            Position = new double[] { x, y, z };
            Center = new Point3D(x, y, z);
            Radius = 0.5;

            BackMaterial = new DiffuseMaterial(Brushes.Black);
        }

        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>The X position.</value>
        private double X { get { return Position[0]; } set { Position[0] = value; } }

        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>The Y position.</value>
        private double Y { get { return Position[1]; } set { Position[1] = value; } }


        /// <summary>
        /// Gets or sets the Z. Not used by MIConvexHull2D.
        /// </summary>
        /// <value>The Z position.</value>
        private double Z { get { return Position[2]; } set { Position[2] = value; } }

        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        public double[] Position
        {
            get;
            set;
        }
    }
}
